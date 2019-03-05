using System;
using BEPUutilities;
using Microsoft.Xna.Framework.Input;

namespace Server.Rendering
{
    public class Camera
    {
        //The current window for rendering the game server
        public Window Window { get; private set; }

        //The current movement speed of the camera
        public float Speed = 100;

        //gets or sets the position of the camera
        public Vector3 Position { get; set; }
        //gets of sets the projection matrix of the camera
        public Matrix ProjectionMatrix { get; set; }

        //gets the view matrix of the camera
        public Matrix ViewMatrix
        {
            get { return Matrix.CreateViewRH(Position, viewDirection, lockedUp); }
        }
        //gets the world matrix of the camera
        public Matrix WorldMatrix
        {
            get { return Matrix.CreateWorldRH(Position, viewDirection, lockedUp); }
        }

        //gets or sets the view direction of the camera
        private Vector3 viewDirection = Vector3.Forward;
        public Vector3 ViewDirection
        {
            get { return viewDirection; }
            set
            {
                float lengthSquared = value.LengthSquared();
                if (lengthSquared > Toolbox.Epsilon)
                {
                    Vector3.Divide(ref value, (float)Math.Sqrt(lengthSquared), out value);
                    //Validate the input. A temporary violation of the maximum pitch is permitted as it will be fixed as the user looks around.
                    //However, we cannot allow a view direction parallel to the locked up direction.
                    float dot;
                    Vector3.Dot(ref value, ref lockedUp, out dot);
                    if (Math.Abs(dot) > 1 - Toolbox.BigEpsilon)
                    {
                        //The view direction must not be aligned with the locked up direction.
                        //Silently fail without changing the view direction.
                        return;
                    }
                    viewDirection = value;
                }
            }
        }

        //gets or sets how far the camera can look up or down (in radians)
        private float maximumPitch = MathHelper.PiOver2 * .99f;
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

        //gets or sets the current locked up vector of the camera
        private Vector3 lockedUp = Vector3.Up;
        public Vector3 LockedUp
        {
            get { return lockedUp; }
            set
            {
                var oldUp = lockedUp;
                float lengthSquared = value.LengthSquared();
                if (lengthSquared > Toolbox.Epsilon)
                {
                    Vector3.Divide(ref value, (float)Math.Sqrt(lengthSquared), out lockedUp);
                    //Move the view direction with the transform. This helps guarantee that the view direction won't end up aligned with the up vector.
                    Quaternion rotation;
                    Quaternion.GetQuaternionBetweenNormalizedVectors(ref oldUp, ref lockedUp, out rotation);
                    Quaternion.Transform(ref viewDirection, ref rotation, out viewDirection);
                }
            }
        }

        //constructor
        public Camera(Vector3 Position, float Pitch, float Yaw, Matrix ProjectionMatrix, Window Window)
        {
            this.Position = Position;
            this.Yaw(Yaw);
            this.Pitch(Pitch);
            this.ProjectionMatrix = ProjectionMatrix;
            this.Window = Window;
        }

        //Moves the camera
        public void MoveCamera(Vector3 MovementVector)
        {
            Position += WorldMatrix.Forward * MovementVector.Z; //move the camera forward/back
            Position += WorldMatrix.Right * MovementVector.X;   //move the camera left/right
            Position += new Vector3(0, MovementVector.Y, 0);    //move the camera up/down
        }

        //Rotates the camera around its locked up vector
        public void Yaw(float radians)
        {
            //Rotate around the up vector.
            Matrix3x3 rotation;
            Matrix3x3.CreateFromAxisAngle(ref lockedUp, radians, out rotation);
            Matrix3x3.Transform(ref viewDirection, ref rotation, out viewDirection);

            //Avoid drift by renormalizing.
            viewDirection.Normalize();
        }

        //Rotates the view direction up or down relative to the locked up vector
        public void Pitch(float radians)
        {
            //Do not allow the new view direction to violate the maximum pitch.
            float dot;
            Vector3.Dot(ref viewDirection, ref lockedUp, out dot);

            //While this could be rephrased in terms of dot products alone, converting to actual angles can be more intuitive.
            //Consider +Pi/2 to be up, and -Pi/2 to be down.
            float currentPitch = (float)Math.Acos(MathHelper.Clamp(-dot, -1, 1)) - MathHelper.PiOver2;
            //Compute our new pitch by clamping the current + change.
            float newPitch = MathHelper.Clamp(currentPitch + radians, -maximumPitch, maximumPitch);
            float allowedChange = newPitch - currentPitch;

            //Compute and apply the rotation.
            Vector3 pitchAxis;
            Vector3.Cross(ref viewDirection, ref lockedUp, out pitchAxis);
            //This is guaranteed safe by all interaction points stopping viewDirection from being aligned with lockedUp.
            pitchAxis.Normalize();
            Matrix3x3 rotation;
            Matrix3x3.CreateFromAxisAngle(ref pitchAxis, allowedChange, out rotation);
            Matrix3x3.Transform(ref viewDirection, ref rotation, out viewDirection);

            //Avoid drift by renormalizing.
            viewDirection.Normalize();

        }

        //Allows control of the camera while the server is running
        public void Update(float DeltaTime)
        {
            //Only turn the camera if the mouse is controlled by the game right now
            if(!Window.IsMouseVisible)
            {
                Yaw((200 - Window.MouseInput.X) * DeltaTime * .12f);
                Pitch((200 - Window.MouseInput.Y) * DeltaTime * .12f);
            }

            //Only move around if the camera has control over its own position
            float Distance = Speed * DeltaTime;

            //WASD for normal movement
            if (Window.KeyboardInput.IsKeyDown(Keys.W))
                MoveCamera(new Vector3(0, 0, Distance));    //forward
            if (Window.KeyboardInput.IsKeyDown(Keys.A))
                MoveCamera(new Vector3(-Distance, 0, 0));   //left
            if (Window.KeyboardInput.IsKeyDown(Keys.S))
                MoveCamera(new Vector3(0, 0, -Distance));   //back
            if (Window.KeyboardInput.IsKeyDown(Keys.D))
                MoveCamera(new Vector3(Distance, 0, 0));    //right

            //RF to move directly up/down
            if (Window.KeyboardInput.IsKeyDown(Keys.R))
                MoveCamera(new Vector3(0, Distance, 0));    //up
            if (Window.KeyboardInput.IsKeyDown(Keys.F))
                MoveCamera(new Vector3(0, -Distance, 0));    //down
        }
    }
}
