using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class ServerEntity
    {
        public Entity Entity;
        public string ID = "-1";
        public string Type = "";
        public Vector3 PreviousUpdatePosition;
    }
}
