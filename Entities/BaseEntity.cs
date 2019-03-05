using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUutilities;

namespace Server.Entities
{
    public abstract class BaseEntity
    {
        public string ID = "-1";
        public string Type = "NULL";
        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Entity entity;
        public abstract void Update(float dt);
        public int HealthPoints = 3;
    }
}
