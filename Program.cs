// ================================================================================================================================
// File:        Program.cs
// Description: The programs main entry point
// ================================================================================================================================

using System;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            if(StartServer())
            {
                RunServer();
                CloseServer();
            }
        }
        
        private static bool StartServer()
        {
            //Connect to the sql database
            if (!Data.Database.InitializeDatabase("localhost", "3306"))
            {
                Console.WriteLine("failed to establish database connection");
                return false;
            }

            //Start listening for new network client connections
            Networking.ConnectionManager.InitializeManager();

            //Open the monogame window (which will start up the physics simulation) to see whats going on
            Rendering.Window GameWindow = new Rendering.Window(800, 600);// 1700, 20);
            return true;
        }

        private static void RunServer()
        {
            l.og("Server is now running");
            Rendering.Window.Instance.Run();
        }

        private static void CloseServer()
        {
            Console.WriteLine("server is now shutting down");
            Items.ItemManager.Backup();
        }
    }
}
