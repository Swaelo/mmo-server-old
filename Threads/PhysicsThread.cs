// ================================================================================================================================
// File:        PhysicsThread.cs
// Description: Certain components of the server are executed on seperate processing threads
//              This thread handles the BEPU Physics Simulation and the MonoGame Rendering Window
// Author:      Harley Laurie          
// Notes:       Are these threads on the CPU? How can they be done on the GPU? GPU processing would be much faster wouldnt it?
// ================================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class PhysicsThread
    {
        public static Thread Thread;
        public static PhysicsThreadEndDelegate EndDelegate;
        public delegate void PhysicsThreadEndDelegate();

        public void InitializeThread()
        {
            Thread = new Thread(new ParameterizedThreadStart(PhysicsThreadProc));
            EndDelegate = PhysicsThreadEnd;
            ((Main)Globals.MainWindowForm).SetPhysicsStatus(true, "Physics Thread: ");
        }

        public void StartThread()
        {
            Thread.Start(EndDelegate);
        }

        public static void PhysicsThreadProc(Object Data)
        {
            Globals.game = new WorldRenderer(800, 600);
            Globals.game.Run();
            PhysicsThreadEndDelegate PhysicsEndDelegate = Data as PhysicsThreadEndDelegate;
            if (PhysicsEndDelegate != null)
                PhysicsEndDelegate();
        }

        public static void PhysicsThreadEnd()
        {
            ((Main)Globals.MainWindowForm).SetPhysicsStatus(false, "Physics Thread: ");
        }
    }
}
