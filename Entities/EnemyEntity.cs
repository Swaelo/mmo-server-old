using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Paths.PathFollowing;

namespace Server.Entities
{
    public enum EnemyState
    {
        Idle,
        Attack,
        Flee
    }

    public class EnemyEntity : BaseEntity
    {
        public EnemyState EntityState = EnemyState.Idle;

        private float AgroRange = 5f;
        private float AgroMaxDistance = 10f;
        public Networking.ClientConnection PlayerTarget = null;
        
        private readonly EntityMover mover; //entity mover defines a linear motor which tries to push the entity to the desired location
        private readonly EntityRotator rotator;

        public EnemyEntity(string EnemyType, Vector3 SpawnPosition)
        {
            Position = SpawnPosition;
            entity = new Entity(new Box(SpawnPosition, 1, 1, 1, 1).CollisionInformation, 0);
            entity.Position = SpawnPosition;

            Physics.WorldSimulator.Space.Add(entity);
            Rendering.GameWindow.CurrentWindow.ModelDrawer.Add(entity);
            EntityManager.AddEntity(this);

            mover = new EntityMover(entity);
            rotator = new EntityRotator(entity);
            Physics.WorldSimulator.Space.Add(mover);
            Physics.WorldSimulator.Space.Add(rotator);
            this.Type = EnemyType;
        }

        public override void Update(float dt)
        {
            Position = entity.Position;
            Rotation = entity.Orientation;

            switch (EntityState)
            {
                //While idle the entity should seek a new player target
                case (EnemyState.Idle):
                    //Find which player is the closest to the enemy, if there are any
                    int PlayerCount = Networking.ConnectionManager.GetActiveClients().Count;
                    if (PlayerCount >= 1)
                    {
                        Networking.ClientConnection ClosestClient = Networking.ConnectionManager.GetClosestActiveClient(Position);
                        //If the closest client is within attack range, then we change to attack state and make them our target
                        float ClosestDistance = Vector3.Distance(Position, ClosestClient.CharacterPosition);
                        if (ClosestDistance <= AgroRange)
                        {
                            EntityState = EnemyState.Attack;
                            PlayerTarget = ClosestClient;
                            break;
                        }
                    }

                    break;
                case (EnemyState.Attack):
                    //Always face toward our current target
                    Vector3 RelativePosition = PlayerTarget.CharacterPosition - Position;
                    Quaternion FacingRotation = LookRotation(RelativePosition, Vector3.Up);
                    rotator.TargetOrientation = FacingRotation;

                    //Check how far away they are
                    float TargetDistance = Vector3.Distance(entity.Position, PlayerTarget.CharacterPosition);

                    //Attack them if they are within range
                    if (TargetDistance <= 1)
                        AttackTarget();
                    //Move closer if they arent within our attack range yet
                    else if (TargetDistance > 1)
                        SeekTarget();
                    //If the target gets too far away, drop them as our target
                    else
                        DropTarget();

                    break;
                case (EnemyState.Flee):

                    break;
            }
        }

        public static Quaternion LookRotation(Vector3 Forward, Vector3 Up)
        {
            Forward.Normalize();
            Vector3 VectorA = Vector3.Normalize(Forward);
            Vector3 VectorB = Vector3.Normalize(Vector3.Cross(Up, VectorA));
            Vector3 VectorC = Vector3.Cross(VectorA, VectorB);
            var m00 = VectorB.X;
            var m01 = VectorB.Y;
            var m02 = VectorB.Z;
            var m10 = VectorC.X;
            var m11 = VectorC.Y;
            var m12 = VectorC.Z;
            var m20 = VectorA.X;
            var m21 = VectorA.Y;
            var m22 = VectorA.Z;

            float num8 = (m00 + m11) + m22;
            var quaternion = new Quaternion();
            if (num8 > 0f)
            {
                var num = (float)Math.Sqrt(num8 + 1f);
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (m12 - m21) * num;
                quaternion.W = (m20 - m02) * num;
                quaternion.Z = (m01 - m10) * num;
                return quaternion;
            }
            if ((m00 >= m11) && (m00 >= m22))
            {
                var num7 = (float)Math.Sqrt(((1f + m00) - m11) - m22);
                var num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return quaternion;
            }
            if (m11 > m22)
            {
                var num6 = (float)Math.Sqrt(((1f + m11) - m00) - m22);
                var num3 = 0.5f / num6;
                quaternion.X = (m10 + m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return quaternion;
            }
            var num5 = (float)Math.Sqrt(((1f + m22) - m00) - m11);
            var num2 = 0.5f / num5;
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;
            return quaternion;
        }

        public void DropTarget()
        {
            l.og("drop");
            //Drop the target, stop the mover/rotator and go back to our idle state
            EntityState = EnemyState.Idle;
            rotator.TargetOrientation = entity.Orientation;
            mover.TargetPosition = entity.Position;
            PlayerTarget = null;
        }

        public void AttackTarget()
        {
            l.og("attack");
        }

        public void SeekTarget()
        {
            l.og("seek");
            //mover.TargetPosition = entity.Position - entity.WorldTransform.Forward * 2.5f;
        }
    }
}
