using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class GameEntity : Updateable, IDuringForcesUpdateable
    {
        public Entity Entity = null;    //Bepu entity base, used for physics / rendering in the server
        public Vector3 Position = Vector3.Zero; //The entities current world position

        //public constructor
        public GameEntity(Vector3 Pos)
        {
            Position = Pos;
            //Create the base entity as a box object inside the physics seen just so we can see where it is
            Entity = new Entity(new Box(Pos, 1, 1, 1, 1).CollisionInformation, 1);
            Globals.space.Add(Entity);
            Globals.game.ModelDrawer.Add(Entity);
            Entity.Position = Pos;
        }

        //Define the entity update function
        void IDuringForcesUpdateable.Update(float dt)
        {
            //When the entity has a pathway to follow here is where it will navigate through it
        }
    }
}
