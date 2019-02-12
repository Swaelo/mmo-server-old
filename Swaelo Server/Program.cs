using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Swaelo_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //Start the game server
            Server.InitializeServer();
            Globals.database.Connect();

            using (var game = new DemosGame())
            {
                game.Run();
            }
        }
    }
}
