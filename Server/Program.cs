using System;
using System.Threading;

namespace Server
{
    class Program
    {
        private static Thread threadConsole;
        private static bool consoleRunning;

        static void Main(string[] args)
        {
            threadConsole = new Thread(new ThreadStart(ConsoleThread));
            threadConsole.Start();

            //to setup the server, we need to register each packet type that we want it to be listening for
            Globals.networkHandleData.InitMessages();
            Globals.general.InitServer();
            Globals.serverLoop.Loop();
        }

        private static void ConsoleThread()
        {
            string line;
            consoleRunning = true;

            while (consoleRunning)
            {
                line = Console.ReadLine();

                if (String.IsNullOrWhiteSpace(line))
                {
                    consoleRunning = false;
                    return;
                }

                //else
                //{

                //}
            }
        }
    }
}
