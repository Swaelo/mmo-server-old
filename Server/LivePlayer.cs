using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//tracks all the information about a player who is currently active in the game world
namespace Server
{
    class LivePlayer
    {
        public string AccountName;
        public int ClientIndex;
        public Vector3 PlayerPosition = new Vector3();
        public Vector4 PlayerRotation = new Vector4();
        public bool IsPlaying = false;
    }
}
