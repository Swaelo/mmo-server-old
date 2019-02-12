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
            RunWithoutDemo();
        }

        private static void RunWithDemo()
        {
            StartServer();
            using (var game = new DemosGame())
            {
                game.Run();
            }
        }

        private static void RunWithoutDemo()
        {
            StartServer();
            Console.ReadLine();
        }

        private static void StartServer()
        {
            //Start the game server
            Server.InitializeServer();
            Globals.database.Connect();
        }
    }
}
