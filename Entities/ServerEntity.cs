using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class ServerEntity : Updateable, IDuringForcesUpdateable
    {
        public string Type = "ServerEntity";
        public string ID = "-1";
        public Entity entity = null;
        public SingleEntityLinearMotor linearMotor = null;
        private Vector3 position = new Vector3(0, 0, 0);
        public Vector3 targetPosition = new Vector3(0, 0, 0);
        public Vector3 previousUpdatePosition = new Vector3(0, 0, 0);

        public ServerEntity()
        { }

        public ServerEntity(Entity e)
        {
            IsUpdatedSequentially = false;
            linearMotor = new SingleEntityLinearMotor(e, e.Position);
            entity = e;
            linearMotor.Entity = e;
            linearMotor.Settings.Mode = MotorMode.Servomechanism;
            targetPosition = e.Position;
        }

        public ServerEntity(Entity e, SingleEntityLinearMotor linearMotor)
        {
            IsUpdatedSequentially = false;
            this.linearMotor = linearMotor;
            entity = e;
            linearMotor.Entity = e;
            linearMotor.Settings.Mode = MotorMode.Servomechanism;
            targetPosition = e.Position;
        }

        public ServerEntity(Vector3 Pos)
        {
            position = Pos;
            entity = new Entity(new Box(Pos, 1, 1, 1, 1).CollisionInformation, 1);
        }

        public Vector3 LocalOffset
        {
            get { return linearMotor.LocalPoint; }
            set { linearMotor.LocalPoint = value; }
        }

        void IDuringForcesUpdateable.Update(float dt)
        {
            if (linearMotor != null && entity != linearMotor.Entity)
                throw new InvalidOperationException("EntityMover's entity differs from EntityMover's motors' entities.");

            if(entity.IsDynamic)
            {
                if(linearMotor != null)
                {
                    linearMotor.IsActive = true;
                    linearMotor.Settings.Servo.Goal = targetPosition;
                }
            }
            else
            {
                if(linearMotor != null)
                {
                    linearMotor.IsActive = false;
                    Vector3 worldMovedPoint = Matrix3x3.Transform(LocalOffset, entity.orientationMatrix);
                }
            }
        }
    }
}
