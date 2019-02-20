using System;
using System.Text;
using System.Timers;
using System.Threading;
using System.Collections.Generic;

namespace Swaelo_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //Network Socket Communication is all handled on the first thread
            Thread NetworkingThread = new Thread(new ParameterizedThreadStart(NetworkingThreadProc));
            NetworkingThreadEndDelegate NetworkingEndDelegate = NetworkingThreadEnd;
            NetworkingThread.Start(NetworkingEndDelegate);

            //Physics Simulation and Server Logic is processed on the second thread
            //Rendering is also done on this thread while debugging
            Thread PhysicsThread = new Thread(new ParameterizedThreadStart(PhysicsThreadProc));
            PhysicsThreadEndDelegate PhysicsEndDelegate = PhysicsThreadEnd;
            PhysicsThread.Start(PhysicsEndDelegate);
        }

        //Define custom functions to perform each thread workload and trigger a callback event
        //when they have completed their tasks
        public static void NetworkingThreadProc(Object Data)
        {
            //Perform all the servers networking tasks in this thread
            Server.InitializeServer();
            Globals.database.Connect();
            Console.ReadLine();

            //Have the thread tell the main program when its finished
            NetworkingThreadEndDelegate NetworkingEndDelegate = Data as NetworkingThreadEndDelegate;
            if (NetworkingEndDelegate != null)
                NetworkingEndDelegate();
        }
        delegate void NetworkingThreadEndDelegate();
        static protected void NetworkingThreadEnd()
        {
            Console.WriteLine("Networking thread finished");
        }

        //Functions to perform physics thread workload and callback events
        public static void PhysicsThreadProc(Object Data)
        {
            //Run thread tasks
            Globals.game = new WorldRenderer();
            Globals.game.Run();

            //Trigger thread end event
            PhysicsThreadEndDelegate PhysicsEndDelegate = Data as PhysicsThreadEndDelegate;
            if (PhysicsEndDelegate != null)
                PhysicsEndDelegate();
        }
        delegate void PhysicsThreadEndDelegate();
        static protected void PhysicsThreadEnd()
        {
            Console.WriteLine("Physics thread finished");
        }
    }
}