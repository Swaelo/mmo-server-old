using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using BEPUphysics.BroadPhaseEntries;
using BEPUutilities;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Server.Data
{
    public class TerrainMesh
    {
        public Vector3[] Vertices;
        public int[] Indices;
        public Model ModelData;
        public StaticMesh MeshCollider;

        public TerrainMesh(string MeshName, Rendering.Window GameWindow)
        {
            l.og("Loading Terrain Mesh...");
            //Load the terrain model data from file
            ModelData = GameWindow.Content.Load<Model>("TerrainMesh");
            //Extract the models vertices and indices from the model data object
            ModelDataExtractor.GetVerticesAndIndicesFromModel(ModelData, out Vertices, out Indices);
            MeshCollider = new StaticMesh(Vertices, Indices, new AffineTransform(new Vector3(0, 0, 0)));
            //Add the mesh to the physics scene
            Physics.WorldSimulator.Space.Add(MeshCollider);
            l.og("Loading Terrain Mesh... Complete!");
        }
    }
}
