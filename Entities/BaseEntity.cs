// ================================================================================================================================
// File:        BaseEntity.cs
// Description: Base Entity type to implement from when defining more advanced entity types like EnemyEntity or a boss type entity
//              All entities once created, are stored in the EntityManager in this base class type to be kept in a single list
// ================================================================================================================================

using BEPUphysics.Entities;
using BEPUutilities;

namespace Server.Entities
{
    public abstract class BaseEntity
    {
        public string ID = "-1";
        public string Type = "NULL";
        public Vector3 Scale = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Position { get { return Entity.Position; } }
        public Entity Entity;
        public abstract void Update(float dt);
        public int HealthPoints = 3;
    }
}
