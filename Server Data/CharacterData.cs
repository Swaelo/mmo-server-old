// ================================================================================================================================
// File:        CharacterData.cs
// Description: Stores all the information for a single player character which is currently being used by one of the ingame clients
// Author:      Harley Laurie          
// Notes:       
// ================================================================================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace Swaelo_Server
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
