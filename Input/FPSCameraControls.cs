// ================================================================================================================================
// File:        FPSCameraControls.cs
// Description: Based off the CharacterCameraControlScheme from the BEPU physics demos
// ================================================================================================================================

using System;
using BEPUphysics.Character;
using BEPUutilities;

namespace Server.Rendering
{
    //This code is based of the character camera control scheme from the BEPU physics demos
    public class FPSCameraControls : BaseCameraControls
    {
        public float StandingCameraOffset;  //The offset from the position of the character to the 'eyes' while the character is standing
        public float CrouchingCameraOffset; //The offset from the position of the character to the 'eyes' while the character is crouching

        public CharacterController CharacterController; //The  FPS character controller associated with this control scheme

        public bool UseCameraSmoothing; //Whether or not to smooth the motion of the camera when the characvter moves discontinuously

        //Default Constructor
        public FPSCameraControls(CharacterController CharacterController, Camera Camera, Window Game)
            : base(Camera, Game)
        {
            this.CharacterController = CharacterController;
            UseCameraSmoothing = true;
            StandingCameraOffset = 0.7f;
            CrouchingCameraOffset = 0.4f;
        }

        //Custom update function
        public override void Update(float DeltaTime)
        {
            base.Update(DeltaTime);

            //Get the current camera offset value based on the current stance
            float CameraOffset = CharacterController.StanceManager.CurrentStance == Stance.Standing ? StandingCameraOffset : CrouchingCameraOffset;

            //Apply camera controls based on whether or not smoothing is currently active
            if (UseCameraSmoothing)
            {
                //First, find where the camera is expected to be based on the last position and the current velocity.
                //Note: if the character were a free-floating 6DOF character, this would need to include an angular velocity contribution.
                //And of course, the camera orientation would be based on the character's orientation.

                //We use the space's time step since, in the demos, the simulation moves forward one time step per frame.
                //The passed-in dt, in contrast, does not necessarily correspond to a simulated state and tends to make the camera jittery.
                var SpaceDeltaTime = CharacterController.Space != null ? CharacterController.Space.TimeStepSettings.TimeStepDuration : DeltaTime;
                Camera.Position = Camera.Position + CharacterController.Body.LinearVelocity * SpaceDeltaTime;
                //Now compute where the camera should be according the physical body of the character and the stance
                Vector3 CharacterUpDirection = CharacterController.Body.OrientationMatrix.Up;
                Vector3 CharacterBodyPosition = CharacterController.Body.BufferedStates.InterpolatedStates.Position;
                Vector3 TargetPosition = CharacterBodyPosition + CharacterUpDirection * CameraOffset;

                //Usually, the camera position and the goal will be very close, if not matching completely.
                //However, if the character steps or has its position otherwise modified, then they will not match.
                //In this case, we need to correct the camera position.

                //To do this, first note that we can't correct infinite errors.  We need to define a bounding region that is relative to the character
                //in which the camera can interpolate around.  The most common discontinuous motions are those of upstepping and downstepping.
                //In downstepping, the character can teleport up to the character's MaximumStepHeight downwards.
                //In upstepping, the character can teleport up to the character's MaximumStepHeight upwards, and the body's CollisionMargin horizontally.
                //Picking those as bounds creates a constraining cylinder

                Vector3 Error = TargetPosition - Camera.Position;
                float VerticalError = Vector3.Dot(Error, CharacterUpDirection);
                Vector3 HorizontalError = Error - VerticalError * CharacterUpDirection;
                //Clamp the vertical component of the camera position within the bounding cylinder
                if (VerticalError > CharacterController.StepManager.MaximumStepHeight)
                {
                    Camera.Position -= CharacterUpDirection * (CharacterController.StepManager.MaximumStepHeight - VerticalError);
                    VerticalError = CharacterController.StepManager.MaximumStepHeight;
                }
                else if (VerticalError < -CharacterController.StepManager.MaximumStepHeight)
                {
                    Camera.Position -= CharacterUpDirection * (-CharacterController.StepManager.MaximumStepHeight - VerticalError);
                    VerticalError = -CharacterController.StepManager.MaximumStepHeight;
                }
                //Clamp the horizontal distance too
                float HorizontalErrorLength = HorizontalError.LengthSquared();
                float Margin = CharacterController.Body.CollisionInformation.Shape.CollisionMargin;
                if (HorizontalErrorLength > Margin * Margin)
                {
                    Vector3 PreviousHorizontalError = HorizontalError;
                    Vector3.Multiply(ref HorizontalError, Margin / (float)Math.Sqrt(HorizontalErrorLength), out HorizontalError);
                    Camera.Position -= HorizontalError - PreviousHorizontalError;
                }
                //Now that the error/camera position is known to lie within the constraining cylinder, we can perform a smooth correction.

                //This removes a portion of the error each frame.
                //Note that this is not framerate independent.  If fixed time step is not enabled,
                //a different smoothing method should be applied to the final error values.
                //float errorCorrectionFactor = .3f;

                //This version is framerate independent, although it is more expensive
                float ErrorCorrectionFactor = (float)(1 - Math.Pow(.000000001, DeltaTime));
                Camera.Position += CharacterUpDirection * (VerticalError * ErrorCorrectionFactor);
                Camera.Position += HorizontalError * ErrorCorrectionFactor;
            }
            else
                Camera.Position = CharacterController.Body.Position + CameraOffset * CharacterController.Body.OrientationMatrix.Up;
        }
    }
}
