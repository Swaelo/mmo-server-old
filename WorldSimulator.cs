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

        public GameEntity PrincessFox;
        public GameEntity PrincessTarget;
        //public ServerEntity WaypointTest;

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


            //Load the world terrain mesh data
            Vector3[] TerrainVertices;
            int[] TerrainIndices;
            Model TerrainCollision = Globals.game.Content.Load<Model>("LowDetailTerrain");
            ModelDataExtractor.GetVerticesAndIndicesFromModel(TerrainCollision, out TerrainVertices, out TerrainIndices);
            StaticMesh TerrainMesh = new StaticMesh(TerrainVertices, TerrainIndices, new AffineTransform(new Vector3(0, 0, 0)));
            Globals.TerrainVerts = TerrainVertices;
            Globals.TerrainInds = TerrainIndices;
            Globals.space.Add(TerrainMesh);
            Globals.game.ModelDrawer.Add(TerrainMesh);
            
            //Create a new mesh node object for each unique location in the navigation mesh
            for(int i = 0; i < Globals.TerrainVerts.Length; i++)
            {
                //Dont create mesh nodes at locations already been used
                if (!NavMeshNodes.IsLocationAvailable(Globals.TerrainVerts[i]))
                    continue;

                //Add a new mesh node object to every other space in the terrain verts that we find to be available
                NavMeshNodes.AddNode(new NavMeshNode(Globals.TerrainVerts[i]));
            }

            //Now sort all of the mesh nodes into a dictionary sorted by their index in the level
            //Start with the first column of mesh nodes
            for(int i = 1; i < 10; i++)
            {
                Vector2 MeshIndex = new Vector2(i, 1);
                int MeshNodeIndex = (i - 1) * 2;
                NavMeshNode MeshNode = NavMeshNodes.MeshNodes[MeshNodeIndex];
                NavMeshDictionary.AddNode(MeshNode, MeshIndex);
            }
            //Then the second column of mesh nodes
            int CurrentNodeIndex = 1;
            for(int i = 1; i < 10; i++)
            {
                Vector2 MeshIndex = new Vector2(i, 2);
                int NodeIndex = CurrentNodeIndex;
                CurrentNodeIndex += 2;
                NavMeshNode MeshNode = NavMeshNodes.MeshNodes[NodeIndex];
                MeshNode.NodeIndex = MeshIndex;
                NavMeshDictionary.AddNode(MeshNode, MeshIndex);
            }
            //Add the other remaining columns
            int ListIndex = 18;
            for(int Column = 3; Column < 10; Column++)
            {
                for(int j = 1; j < 10; j++)
                {
                    Vector2 MeshIndex = new Vector2(j, Column);
                    NavMeshNode MeshNode = NavMeshNodes.MeshNodes[ListIndex];
                    MeshNode.NodeIndex = MeshIndex;
                    ListIndex++;
                    NavMeshDictionary.AddNode(MeshNode, MeshIndex);
                }
            }

            //Add two game entities into the scene, the first entity will pathfind its way to the 2nd entity
            PrincessFox = new GameEntity(new Vector3(-17.74f, 2.15f, 10.54f));
            PrincessTarget = new GameEntity(new Vector3(-41.17f, 0.89f, 24.15f));
            //Figure out which node each of these entities is closest two, these will be the ends of the pathway
            NavMeshNode StartNode = NavMeshDictionary.MeshDictionary[new Vector2(3, 4)];
            NavMeshNode EndNode = NavMeshDictionary.MeshDictionary[new Vector2(7, 8)];

            //Find our pathway between the two entities
            List<NavMeshNode> NodePath = AStarSearch.FindPath(StartNode, EndNode, new Vector2(9, 9));
            //Now assign this path to the princess fox entity and have her navigate along to reach her target
            
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
