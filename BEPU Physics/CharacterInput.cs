using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace Swaelo_Server
{
    public class CharacterInput
    {
        public Camera Camera { get; private set; }
        public CharacterController CharacterController;
        public CharacterCamera CameraControlScheme { get; private set; }
        public bool IsActive { get; private set; }
        public Space Space { get; private set; }

        public CharacterInput(Space owningSpace, Camera camera, WorldRenderer game)
        {
            CharacterController = new CharacterController();
            Camera = camera;
            CameraControlScheme = new CharacterCamera(CharacterController, camera, game);

            Space = owningSpace;
        }

        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                Space.Add(CharacterController);
                //Offset the character start position from the camera to make sure the camera doesn't shift upward discontinuously.
                CharacterController.Body.Position = Camera.Position - new Vector3(0, CameraControlScheme.StandingCameraOffset, 0);
            }
        }

        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                Space.Remove(CharacterController);
            }
        }

        public void Update(float dt, KeyboardState previousKeyboardInput, KeyboardState keyboardInput, GamePadState previousGamePadInput, GamePadState gamePadInput)
        {
            if (IsActive)
            {
                CameraControlScheme.Update(dt);
                Vector2 totalMovement = Vector2.Zero;

                if (keyboardInput.IsKeyDown(Keys.E))
                {
                    totalMovement += new Vector2(0, 1);
                }
                if (keyboardInput.IsKeyDown(Keys.D))
                {
                    totalMovement += new Vector2(0, -1);
                }
                if (keyboardInput.IsKeyDown(Keys.S))
                {
                    totalMovement += new Vector2(-1, 0);
                }
                if (keyboardInput.IsKeyDown(Keys.F))
                {
                    totalMovement += new Vector2(1, 0);
                }

                if (totalMovement == Vector2.Zero)
                    CharacterController.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
                else
                    CharacterController.HorizontalMotionConstraint.MovementDirection = Vector2.Normalize(totalMovement);


                if (keyboardInput.IsKeyDown(Keys.X))
                    CharacterController.StanceManager.DesiredStance = Stance.Prone;
                else if (keyboardInput.IsKeyDown(Keys.Z))
                    CharacterController.StanceManager.DesiredStance = Stance.Crouching;
                else
                    CharacterController.StanceManager.DesiredStance = Stance.Standing;

                //Jumping
                if (previousKeyboardInput.IsKeyUp(Keys.A) && keyboardInput.IsKeyDown(Keys.A))
                {
                    CharacterController.Jump();
                }

                CharacterController.ViewDirection = Camera.WorldMatrix.Forward;
            }
        }
    }
}
