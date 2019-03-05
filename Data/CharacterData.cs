using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUutilities;

namespace Server.Data
{
    public class CharacterData
    {
        public string Account;
        public Vector3 Position;
        public Quaternion Rotation;
        public string Name;
        public int Experience;
        public int ExperienceToLevel;
        public int Level;
        public bool IsMale;
    }
}
