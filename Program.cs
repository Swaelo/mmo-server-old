// ================================================================================================================================
// File:        Program.cs
// Description: The programs main entry point
// ================================================================================================================================

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Connect to the sql database
            Data.Database.InitializeDatabase("192.168.0.5", "3306");
            //Start listening for new client connections
            Networking.ConnectionManager.InitializeManager();
            //Open the monogame window (which will start up the physics simulation) to see whats going on
            new Rendering.Window(800, 600);
            Rendering.Window.Instance.Run();
        }
    }
}
