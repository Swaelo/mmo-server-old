// ================================================================================================================================
// File:        ServerPhysics.cs
// Description: Controls the BEPU physics space and all objects that have been added to it, used to check for collisions etc
// Author:      Harley Laurie          
// Notes:       Extract all the MonoGame / IO code into its own seperate class so this can be used later without rendering anything
// ================================================================================================================================

using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Swaelo_Server
{
    public class WorldSimulator
    {
        private int accumulatedPhysicsFrames;
        private double accumulatedPhysicsTime;
        private double previousTimeMeasurement;
        private ParallelLooper parallelLooper;
        public FreeCamera ServerCamera;
        public double PhysicsTime { get; private set; }
        public string Name { get { return "GameWorld"; } }
        public WorldRenderer Game { get; private set; }
        bool NodeInserted = false;
        public EnemyEntity FoxPrincess;
        //public GameEntity PrincessTarget;
        //public ServerEntity WaypointTest;
        public FSMEntity EntityTest;

        //We will want to keep clients updated on all entities every quarter of a second
        private float EntityUpdateInterval = 1;
        private float NextEntityUpdate = 1;

        public WorldSimulator(WorldRenderer game)
        {
            Game = game;
            parallelLooper = new ParallelLooper();
            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    parallelLooper.AddThread();
                }
            }

            //Set up the world space and camera for rendering
            Globals.space = new Space(parallelLooper);
            game.Camera.LockedUp = Vector3.Up;
            game.Camera.ViewDirection = new Vector3(0, -1.5f, 1);
            game.Camera.Position = new Vector3(-19.55f, 39.25f, -10.35f);
            ServerCamera = new FreeCamera(100, game.Camera, game);

            //Add some force of gravity to the simulation
            Globals.space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            //Load in the physics terrain mesh
            TerrainMesh LevelTerrain = new TerrainMesh("TerrainMesh");
            Globals.game.ModelDrawer.Add(LevelTerrain.MeshCollider);

            //Load in the Navigation Mesh
            NavMesh NavigationMesh = new NavMesh("NavMesh");
            Globals.game.ModelDrawer.Add(NavigationMesh.MeshData);



            ////now create a fox princess to wander around the level
            //FoxPrincess = new EnemyEntity(new Vector3(-3.66f, 0, -1.01f));
        }

        public void Update(float dt)
        {
            EntityManager.UpdateEntities(dt);

            long startTime = Stopwatch.GetTimestamp();
            Globals.space.Update();

            long endTime = Stopwatch.GetTimestamp();
            accumulatedPhysicsTime += (endTime - startTime) / (double)Stopwatch.Frequency;
            accumulatedPhysicsFrames++;
            previousTimeMeasurement += dt;
            if (previousTimeMeasurement > .3f)
            {
                previousTimeMeasurement -= .3f;
                PhysicsTime = accumulatedPhysicsTime / accumulatedPhysicsFrames;
                accumulatedPhysicsTime = 0;
                accumulatedPhysicsFrames = 0;
            }

            ServerCamera.Update(dt);
        }

        public void DrawUI()
        {

        }

        public void CleanUp()
        {
            ConfigurationHelper.ApplyDefaultSettings(Globals.space);
            parallelLooper.Dispose();
        }
    }
}
