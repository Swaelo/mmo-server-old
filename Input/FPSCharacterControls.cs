// ================================================================================================================================
// File:        FPSCharacterControls.cs
// Description: Based off the CharacterCameraControlScheme from the BEPU physics demos
// ================================================================================================================================

using BEPUphysics;
using BEPUphysics.Character;
using BEPUutilities;
using Microsoft.Xna.Framework.Input;

namespace Server.Rendering
{
    //this class is based off the character controller input class in the BEPU physics demos
    public class FPSCharacterControls
    {
        private float WalkSpeed = 1;  //How fast the character normally moves
        private float RunSpeed = 3.5f;  //How fast the character moves while holding shift
        public Camera Camera;   //The camera to use for input
        public bool IsActive; //Is this controller active right now
        public CharacterController CharacterController; //Physics representation of the character
        public FPSCameraControls CameraControls;    //Control sceme used by the same while it is active
        public Space Space; //physics space containing the character controller

        public Vector3 Position
        {
            get { return CharacterController.Body.Position; }
        }

        //Default Constructor
        public FPSCharacterControls(Space Space, Camera Camera, Window Game)
        {
            CharacterController = new CharacterController();
            this.Camera = Camera;
            CameraControls = new FPSCameraControls(CharacterController, Camera, Game);
            this.Space = Space;
            CharacterController.Body.Tag = "noDisplayObject";
        }

        //Assigns the camera to the character and actives the controls
        public void Activate()
        {
            if(!IsActive)
            {
                IsActive = true;
                Space.Add(CharacterController);
                CharacterController.Body.Position = Camera.Position - new Vector3(0, CameraControls.StandingCameraOffset, 0);
            }
        }

        //Deactivates the character controller and returns the camera to free controls
        public void Deactivate()
        {
            if(IsActive)
            {
                IsActive = false;
                Space.Remove(CharacterController);
            }
        }

        //Handles the input and movement of the character
        public void Update(float DeltaTime, KeyboardState PreviousKeyboardInput, KeyboardState KeyboardInput)
        {
            if(IsActive)
            {
                //Note that the character controller's update method is not called here; this is because it is handled within its owning space.
                //This method's job is simply to tell the character to move around
                CameraControls.Update(DeltaTime);

                //Compute character controller movement vector based on input, only while the controller is active
                if(IsActive)
                {
                    //Define the new movement vector to be applied to the character controller and the current movement speed
                    Vector2 CharacterMovement = Vector2.Zero;
                    float MovementSpeed = KeyboardInput.IsKeyDown(Keys.LeftShift) ? RunSpeed : WalkSpeed;

                    //W/S) Forward/Backward Movement
                    if (KeyboardInput.IsKeyDown(Keys.W))
                        CharacterMovement.Y += MovementSpeed;
                    if (KeyboardInput.IsKeyDown(Keys.S))
                        CharacterMovement.Y -= MovementSpeed;
                    //A/D Left/Right Movement
                    if (KeyboardInput.IsKeyDown(Keys.A))
                        CharacterMovement.X -= MovementSpeed;
                    if (KeyboardInput.IsKeyDown(Keys.D))
                        CharacterMovement.X += MovementSpeed;

                    //Zero out the movement input if not input was detected
                    if (CharacterMovement == Vector2.Zero)
                        CharacterController.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
                    //Otherwise apply the movement vector as normal
                    else
                        CharacterController.HorizontalMotionConstraint.MovementDirection = Vector2.Normalize(CharacterMovement);

                    //Crouch while holding C
                    if (KeyboardInput.IsKeyDown(Keys.C))
                        CharacterController.StanceManager.DesiredStance = Stance.Crouching;
                    //Otherwise stand up as normal
                    else
                        CharacterController.StanceManager.DesiredStance = Stance.Standing;

                    //Jump with spacebar, if they didnt jump the previous frame
                    if (PreviousKeyboardInput.IsKeyUp(Keys.Space) && KeyboardInput.IsKeyDown(Keys.Space))
                        CharacterController.Jump();

                    //Update the view direction based on the current camera position relative to the player
                    CharacterController.ViewDirection = Camera.WorldMatrix.Forward;
                }
            }
        }
    }
}
