using System;
using System.Diagnostics;
using BEPUphysics;
using BEPUutilities;
using BEPUutilities.Threading;

namespace Server.Physics
{
    public static class WorldSimulator
    {
        private static Rendering.Window Window;
        private static int AccumulatedPhysicsFrames;
        private static double AccumulatedPhysicsTime;
        private static double PreviousTimeMeasurement;
        private static ParallelLooper ParallelLooper;
        public static double PhysicsTime { get; private set; }
        public static Space Space;

        public static void InitializeSimulation(Rendering.Window GameWindow)
        {
            Window = GameWindow;

            //Initialize the parallel looper and assign any extra cpu threads to it
            ParallelLooper = new ParallelLooper();
            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                    ParallelLooper.AddThread();
            }

            //Set up the bepu physics world space simulation
            Space = new Space(ParallelLooper);
            Space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            //Load in the physics for the world terrain mesh
            Data.TerrainMesh LevelTerrain = new Data.TerrainMesh("TerrainMesh", GameWindow);
            GameWindow.ModelDrawer.Add(LevelTerrain.MeshCollider);

            //Load in the AI navigation mesh
            Pathfinding.NavMesh NavigationMesh = new Pathfinding.NavMesh("NavMesh");
            //GameWindow.ModelDrawer.Add(NavigationMesh.MeshData);

            //Place an enemy entity into the game world
            Entities.EnemyEntity SkeletonWarrior = new Entities.EnemyEntity("Skeleton Warrior", new Vector3(-65.96f, 18.24f, -32.09f));
        }

        public static void Update(float DeltaTime)
        {
            Entities.EntityManager.UpdateEntities(DeltaTime);
            
            long UpdateStart = Stopwatch.GetTimestamp();

            Space.Update();

            long UpdateEnd = Stopwatch.GetTimestamp();
            AccumulatedPhysicsTime += (UpdateEnd - UpdateStart) / (double)Stopwatch.Frequency;
            AccumulatedPhysicsFrames++;
            PreviousTimeMeasurement += DeltaTime;
            if(PreviousTimeMeasurement > .3f)
            {
                PreviousTimeMeasurement -= .3f;
                PhysicsTime = AccumulatedPhysicsTime / AccumulatedPhysicsFrames;
                AccumulatedPhysicsTime = 0;
                AccumulatedPhysicsFrames = 0;
            }

            Window.Camera.Update(DeltaTime);
        }

        public static void CleanUp()
        {
            if(ParallelLooper != null)
                ParallelLooper.Dispose();
        }
    }
}
