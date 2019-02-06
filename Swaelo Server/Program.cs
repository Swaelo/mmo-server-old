using System;

namespace Swaelo_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.InitializeServer();  //start the game server
            Globals.database.Connect(); //connect to the database
           // Globals.ground_items.LoadGroundItems();
            Console.ReadLine();
        }
    }
}
