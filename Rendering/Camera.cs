// ================================================================================================================================
// File:        Camera.cs
// Description: Defines the camera used to render the scene to the monogame window
// ================================================================================================================================

using System;
using BEPUutilities;

namespace Server.Rendering
{
    public class Camera
    {
        public Vector3 Position;    //Cameras world position within the current server world simulation
        public Matrix ProjectionMatrix;
        public Matrix ViewMatrix { get { return Matrix.CreateViewRH(Position, viewDirection, upDirection); } }
        public Matrix WorldMatrix { get { return Matrix.CreateWorldRH(Position, viewDirection, upDirection); } }
        
        private Vector3 viewDirection = Vector3.Forward;    //Current direction the camera is facing
        public Vector3 ViewDirection
        {
            get { return viewDirection; }
            set
            {
                float LengthSquared = value.LengthSquared();
                if (LengthSquared > Toolbox.Epsilon)
                {
                    Vector3.Divide(ref value, (float)Math.Sqrt(LengthSquared), out value);
                    //Validate the input. A temporary violation of the maximum pitch is permitted as it will be fixed as the user looks around.
                    //However, we cannot allow a view direction parallel to the locked up direction.
                    float Dot;
                    Vector3.Dot(ref value, ref upDirection, out Dot);
                    if (Math.Abs(Dot) > 1 - Toolbox.BigEpsilon)
                    {
                        //The view direction must not be aligned with the locked up direction.
                        //Silently fail without changing the view direction.
                        return;
                    }
                    viewDirection = value;
                }
            }
        }

        private float maximumPitch = MathHelper.PiOver2 * 0.99f;
        public float MaximumPitch
        {
            get { return maximumPitch; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Maximum pitch corresponds to pitch magnitude; must be positive.");
                if (value >= MathHelper.PiOver2)
                    throw new ArgumentException("Maximum pitch must be less than Pi/2.");
                maximumPitch = value;
            }
        }

        private Vector3 upDirection = Vector3.Up;
        private Vector3 UpDirection
        {
            get { return upDirection; }
            set
            {
                var OldUp = upDirection;
                float LengthSquared = value.LengthSquared();
                if(LengthSquared > Toolbox.Epsilon)
                {
                    Vector3.Divide(ref value, (float)Math.Sqrt(LengthSquared), out upDirection);
                    //Move the view direction with the transform. This helps guarantee that the view direction won't end up aligned with the up vector.
                    Quaternion rotation;
                    Quaternion.GetQuaternionBetweenNormalizedVectors(ref OldUp, ref upDirection, out rotation);
                    Quaternion.Transform(ref viewDirection, ref rotation, out viewDirection);
                }
            }
        }

        public Camera(Vector3 position, float pitch, float yaw, Matrix projectionMatrix)
        {
            Position = position;
            Yaw(yaw);
            Pitch(pitch);
            ProjectionMatrix = projectionMatrix;
        }

        public void Yaw(float radians)
        {
            //Rotate around the up vector.
            Matrix3x3 rotation;
            Matrix3x3.CreateFromAxisAngle(ref upDirection, radians, out rotation);
            Matrix3x3.Transform(ref viewDirection, ref rotation, out viewDirection);

            //Avoid drift by renormalizing.
            viewDirection.Normalize();
        }

        public void Pitch(float radians)
        {
            //Do not allow the new view direction to violate the maximum pitch.
            float Dot;
            Vector3.Dot(ref viewDirection, ref upDirection, out Dot);

            //While this could be rephrased in terms of dot products alone, converting to actual angles can be more intuitive.
            //Consider +Pi/2 to be up, and -Pi/2 to be down.
            float CurrentPitch = (float)Math.Acos(MathHelper.Clamp(-Dot, -1, 1)) - MathHelper.PiOver2;
            //Compute our new pitch by clamping the current + change.
            float NewPitch = MathHelper.Clamp(CurrentPitch + radians, -maximumPitch, maximumPitch);
            float AllowedChange = NewPitch - CurrentPitch;

            //Compute and apply the rotation.
            Vector3 PitchAxis;
            Vector3.Cross(ref viewDirection, ref upDirection, out PitchAxis);
            //This is guaranteed safe by all interaction points stopping viewDirection from being aligned with lockedUp.
            PitchAxis.Normalize();
            Matrix3x3 Rotation;
            Matrix3x3.CreateFromAxisAngle(ref PitchAxis, AllowedChange, out Rotation);
            Matrix3x3.Transform(ref viewDirection, ref Rotation, out viewDirection);

            //Avoid drift by renormalizing.
            viewDirection.Normalize();
        }

        public void MoveCamera(Vector3 Movement)
        {
            Position += Movement;
        }
    }
}
