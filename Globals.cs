using System;
using System.Collections.Generic;
using System.Text;

namespace Swaelo_Server
{
    class Globals
    {
        public static Database database = new Database();
        public static WorldRenderer game = null;
        public static WorldSimulator world = null;
        public static Space space = null;
    }
}
