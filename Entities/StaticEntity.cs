using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class StaticEntity : BaseEntity
    {
        public StaticEntity(Vector3 Position)
        {
            this.Position = Position;
            entity = new Entity(new Box(Position, 1, 1, 1, 1).CollisionInformation, 0);
            entity.Position = Position;
            Globals.space.Add(entity);
            Globals.game.ModelDrawer.Add(entity);
        }
    }
}
