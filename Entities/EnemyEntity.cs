using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
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

        private readonly EntityMover mover;
        private readonly EntityRotator rotator;
        

        public EnemyEntity(Vector3 SpawnPosition)
        {
            Position = SpawnPosition;
            entity = new Entity(new Box(SpawnPosition, 1, 1, 1, 1).CollisionInformation, 1);
            entity.Position = SpawnPosition;
            Globals.space.Add(entity);
            Globals.game.ModelDrawer.Add(entity);
            EntityManager.AddEntity(this);
            mover = new EntityMover(entity);
            rotator = new EntityRotator(entity);
            Globals.space.Add(mover);
            Globals.space.Add(rotator);
        }

        public override void Update(float dt)
        {
            switch(EntityState)
            {
                case (EnemyState.Idle):

                    break;
                case (EnemyState.Attack):

                    break;
                case (EnemyState.Flee):

                    break;
            }
        }
    }
}
