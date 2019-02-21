using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Swaelo_Server
{
    public class StaticEntity : BaseEntity
    {
        public static Vector3[] MeshVertices;
        public static int[] MeshIndices;

        //creates a 1x1x1 cube entity at the target position
        public StaticEntity(Vector3 Position)
        {
            
            this.Type = "Static";
            this.Position = Position;
            entity = new Entity(new Box(Position, 1, 1, 1, 1).CollisionInformation, 0);
            entity.Position = Position;
            Globals.space.Add(entity);
            Globals.game.ModelDrawer.Add(entity);
            EntityManager.AddEntity(this);
        }

        //creates a box of the given size at the target location
        public StaticEntity(Vector3 Position, Vector3 Scale)
        {
            this.Type = "Static";
            this.Position = Position;
            entity = new Entity(new Box(Position, Scale.X, Scale.Y, Scale.Z).CollisionInformation, 0);
            entity.Position = Position;
            Globals.space.Add(entity);
            Globals.game.ModelDrawer.Add(entity);
            EntityManager.AddEntity(this);
        }

        public override void Update(float dt)
        {
            
        }
    }
}
