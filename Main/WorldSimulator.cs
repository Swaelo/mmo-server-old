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
        public FSMEntity FoxPrincess;
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
            Globals.space = new Space(parallelLooper);
            game.Camera.LockedUp = Vector3.Up;
            game.Camera.ViewDirection = new Vector3(0, -1.5f, 1);
            game.Camera.Position = new Vector3(-19.55f, 39.25f, -10.35f);
            ServerCamera = new FreeCamera(100, game.Camera, game);

            //Add some force of gravity to the simulation
            Globals.space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);
            
            //Create some level geometry
            //  StaticEntity GroundPlane = new StaticEntity(new Vector3(0, 0, 0), new Vector3(500, 1, 500));
            EntityManager.AddStaticEntity(new Vector3(0, 2.5f, -10), new Vector3(5, 5, 1));
            EntityManager.AddStaticEntity(new Vector3(0, 2.5f, 10), new Vector3(5, 5, 1));
            EntityManager.AddStaticEntity(new Vector3(-10, 2.5f, 0), new Vector3(1, 5, 5));
            EntityManager.AddStaticEntity(new Vector3(10, 2.5f, 0), new Vector3(1, 5, 5));

            //Load the nav mesh
            Vector3[] NavVertices;
            int[] NavIndices;
            Model NavCollision = Globals.game.Content.Load<Model>("nav");
            ModelDataExtractor.GetVerticesAndIndicesFromModel(NavCollision, out NavVertices, out NavIndices);
            StaticMesh NavMesh = new StaticMesh(NavVertices, NavIndices, new AffineTransform(new Vector3(0, 0, 0)));
            Globals.NavMeshVertices = NavVertices;
            Globals.NavMeshIndices = NavIndices;
            Globals.space.Add(NavMesh);
            Globals.game.ModelDrawer.Add(NavMesh);

            //Each index represents a vertex in the nav mesh, at the index values location in the vertex array
            for (int i = 0; i < Globals.NavMeshIndices.Length; i+=3)
            {
                //Load in 3 vertices at a time, which is each triangle of the navigation mesh
                Vector3 Location1 = Globals.NavMeshVertices[Globals.NavMeshIndices[i]];
                Vector3 Location2 = Globals.NavMeshVertices[Globals.NavMeshIndices[i + 1]];
                Vector3 Location3 = Globals.NavMeshVertices[Globals.NavMeshIndices[i + 2]];
                //Acquire the 3 mesh node objects for these locations, create them if they dont exist yet
                NavMeshNode FirstNode = NavMeshNodes.IsLocationAvailable(Location1) ? new NavMeshNode(Location1) : NavMeshNodes.GetNode(Location1);
                NavMeshNode SecondNode = NavMeshNodes.IsLocationAvailable(Location2) ? new NavMeshNode(Location2) : NavMeshNodes.GetNode(Location2);
                NavMeshNode ThirdNode = NavMeshNodes.IsLocationAvailable(Location3) ? new NavMeshNode(Location3) : NavMeshNodes.GetNode(Location3);
                //link first and second node to each other
                if (!FirstNode.Neighbours.Contains(SecondNode))
                    FirstNode.Neighbours.Add(SecondNode);
                if (!SecondNode.Neighbours.Contains(FirstNode))
                    SecondNode.Neighbours.Add(FirstNode);
                //link second and third node to each other
                if (!SecondNode.Neighbours.Contains(ThirdNode))
                    SecondNode.Neighbours.Add(ThirdNode);
                if (!ThirdNode.Neighbours.Contains(SecondNode))
                    ThirdNode.Neighbours.Add(SecondNode);
                //link first and third node to each other
                if (!FirstNode.Neighbours.Contains(ThirdNode))
                    FirstNode.Neighbours.Add(ThirdNode);
                if (!ThirdNode.Neighbours.Contains(FirstNode))
                    ThirdNode.Neighbours.Add(FirstNode);
            }
            //Now all nodes know who their neighbour nodes are fuck yeah

            //now create a fox princess to wander around the level
            FoxPrincess = new FSMEntity(Vector3.Zero, new Vector3(13, 0, 0));
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
