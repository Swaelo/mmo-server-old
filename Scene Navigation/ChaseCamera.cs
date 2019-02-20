using System;

namespace Swaelo_Server
{
    public class ChaseCamera
    {
        public WorldRenderer Game { get; private set; }
        public Camera Camera { get; private set; }

        public Entity ChasedEntity { get; set; }
        public Vector3 OffsetFromChaseTarget { get; set; }
        private bool TransformOffset { get; set; }
        public float DistanceToTarget { get; set; }
        public float ChaseCameraMargin { get; set; }

        Func<BroadPhaseEntry, bool> rayCastFilter;
        bool RayCastFilter(BroadPhaseEntry entry)
        {
            return entry != ChasedEntity.CollisionInformation && (entry.CollisionRules.Personal <= CollisionRule.Normal);
        }

        public ChaseCamera(Entity chasedEntity, Vector3 offsetFromChaseTarget, bool transformOffset, float distanceToTarget, Camera camera, WorldRenderer game)
        {
            Camera = camera;
            Game = game;
            ChasedEntity = chasedEntity;
            OffsetFromChaseTarget = offsetFromChaseTarget;
            TransformOffset = transformOffset;
            DistanceToTarget = distanceToTarget;
            ChaseCameraMargin = 1;

            rayCastFilter = RayCastFilter;
        }

        public void Update(float dt)
        {
            //Only turn if the mouse is controlled by the game.
            if (!Game.IsMouseVisible)
            {
                Camera.Yaw((200 - Game.MouseInput.X) * dt * .12f);
                Camera.Pitch((200 - Game.MouseInput.Y) * dt * .12f);
            }

            Vector3 offset = TransformOffset ? Matrix3x3.Transform(OffsetFromChaseTarget, ChasedEntity.BufferedStates.InterpolatedStates.OrientationMatrix) : OffsetFromChaseTarget;
            Vector3 lookAt = ChasedEntity.BufferedStates.InterpolatedStates.Position + offset;
            Vector3 backwards = -Camera.ViewDirection;

            //Find the earliest ray hit that isn't the chase target to position the camera appropriately.
            RayCastResult result;
            float cameraDistance = ChasedEntity.Space.RayCast(new Ray(lookAt, backwards), DistanceToTarget, rayCastFilter, out result) ? result.HitData.T : DistanceToTarget;

            Camera.Position = lookAt + (Math.Max(cameraDistance - ChaseCameraMargin, 0)) * backwards; //Put the camera just before any hit spot.
        }
    }
}
