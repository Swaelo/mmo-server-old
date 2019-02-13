using System;

namespace Swaelo_Server
{
    class IdleEntity : ServerEntity
    {
        public IdleEntity(Vector3 SpawnPosition)
        {
            Type = "Beetle";
            Entity = new Box(SpawnPosition, 1, 1, 1);
            Globals.space.Add(Entity);
        }
    }
}
