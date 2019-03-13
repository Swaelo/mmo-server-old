// ================================================================================================================================
// File:        BaseCameraControls.cs
// Description: Based off the CameraControlScheme class from the BEPU physics demos
// ================================================================================================================================

namespace Server.Rendering
{
    //Controls the behaviour of the camera contained within
    public abstract class BaseCameraControls
    {
        public Camera Camera;   //Returns the camera controlled by this class
        public Window Game; //Returns the game window associated with this camera

        //base constructor
        protected BaseCameraControls(Camera Camera, Window Game)
        {
            this.Camera = Camera;
            this.Game = Game;
        }

        //Updates the camera state according to which control scheme has been assigned to it
        public virtual void Update(float DeltaTime)
        {
            //Only turn if the mouse is controlled by the game.
            if (!Game.IsMouseVisible)
            {
                Camera.Yaw((200 - Game.MouseInput.X) * DeltaTime * .12f);
                Camera.Pitch((200 - Game.MouseInput.Y) * DeltaTime * .12f);
            }
        }
    }
}
