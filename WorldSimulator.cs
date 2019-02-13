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
        public PathFindingEntity PrincessFox;

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
            Globals.space = new Space(parallelLooper);
            game.Camera.LockedUp = Vector3.Up;
            game.Camera.ViewDirection = new Vector3(0, -1.5f, 1);
            game.Camera.Position = new Vector3(25.35f, 39.28f, -10.35f);
            ServerCamera = new FreeCamera(100, game.Camera, game);

            //Add some force of gravity to the simulation
            Globals.space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            //Load the world terrain mesh data
            Vector3[] TerrainVertices;
            int[] TerrainIndices;
            Model TerrainCollision = Globals.game.Content.Load<Model>("Terrain");
            ModelDataExtractor.GetVerticesAndIndicesFromModel(TerrainCollision, out TerrainVertices, out TerrainIndices);
            StaticMesh TerrainMesh = new StaticMesh(TerrainVertices, TerrainIndices, new AffineTransform(new Vector3(0,0,0)));
            Globals.space.Add(TerrainMesh);
            Globals.game.ModelDrawer.Add(TerrainMesh);

            //add princess fox wandering around the forest
            PrincessFox = new PathFindingEntity(new Vector3(10.94f, 7.22f, 12.35f));
            Globals.space.Add(PrincessFox.Entity);
            Globals.game.ModelDrawer.Add(PrincessFox);
            EntityManager.AddNewEntity(PrincessFox);
        }

        public void Update(float dt)
        {
            //Send delta time to the entity manager, it will handle everything else that it needs to with them
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
