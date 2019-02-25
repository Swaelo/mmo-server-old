using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Swaelo_Server
{
    public class FreeCamera
    {
        public WorldRenderer Game { get; private set; }
        public Camera Camera { get; private set; }

        public float Speed { get; set; }
        public FreeCamera(float speed, Camera camera, WorldRenderer game)
        {
            Speed = speed;
            Camera = camera;
            Game = game;
        }

        public void Update(float dt)
        {
            //Only turn if the mouse is controlled by the game.
            if (!Game.IsMouseVisible)
            {
                Camera.Yaw((200 - Game.MouseInput.X) * dt * .12f);
                Camera.Pitch((200 - Game.MouseInput.Y) * dt * .12f);
            }

            //Only move around if the camera has control over its own position.
            float distance = Speed * dt;

            //WASD for normal movement
            if (Game.KeyboardInput.IsKeyDown(Keys.W))
                Camera.MoveForward(distance);
            if (Game.KeyboardInput.IsKeyDown(Keys.A))
                Camera.MoveRight(-distance);
            if (Game.KeyboardInput.IsKeyDown(Keys.S))
                Camera.MoveForward(-distance);
            if (Game.KeyboardInput.IsKeyDown(Keys.D))
                Camera.MoveRight(distance);

            //RF to move directly up/down
            if (Game.KeyboardInput.IsKeyDown(Keys.R))
                Camera.MoveUp(distance);
            if (Game.KeyboardInput.IsKeyDown(Keys.F))
                Camera.MoveUp(-distance);
        }
    }
}
