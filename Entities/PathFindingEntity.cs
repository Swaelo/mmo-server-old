using System;

namespace Swaelo_Server
{
    public class PathFindingEntity : ServerEntity
    {
        public readonly Path<Quaternion> orientationPath;
        public readonly Path<Vector3> positionPath;

        public PathFindingEntity(Entity e)
        {
            IsUpdatedSequentially = false;
            linearMotor = new SingleEntityLinearMotor(e, e.Position);
            entity = e;
            linearMotor.Settings.Mode = MotorMode.Servomechanism;
            targetPosition = e.Position;
        }

        public PathFindingEntity(Entity e, SingleEntityLinearMotor linearMotor)
        {
            IsUpdatedSequentially = false;
            this.linearMotor = linearMotor;
            entity = e;
            linearMotor.Entity = entity;
            linearMotor.Settings.Mode = MotorMode.Servomechanism;
            targetPosition = e.Position;
        }

        public PathFindingEntity(Vector3 StartPosition)
        {
            Type = "Fox Princess";
            entity = new Entity(new Box(StartPosition, 1, 1, 1).CollisionInformation, 1);
            entity.Position = StartPosition;
        }
    }
}
