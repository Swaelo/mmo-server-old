using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    class ServerPhysics
    {
        public int AccumlatedPhysicsFrames;
        public double AccumlatedPhysicsTime;
        public double PreviousTimeMeasurement;
        public ParallelLooper ParallelLooper;
        public double PhysicsTime { get; private set; }
        
        //Default Constructor
        public ServerPhysics()
        {
            //Run extra threads if they are available
            ParallelLooper = new ParallelLooper();
            if(Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    ParallelLooper.AddThread();
                }
            }
            Globals.space = new Space(ParallelLooper);

            //Set the gravity force in the physics simulation
            Globals.space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            //Load in the terrain mesh collsion data to use as the base of the game simulation
            
        }
    }
}
