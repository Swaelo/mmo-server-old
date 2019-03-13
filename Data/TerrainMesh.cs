// ================================================================================================================================
// File:        TerrainMesh.cs
// Description: Used to load in the worlds terrain mesh collider that has been exported from the unity engine to then be used in
//              the servers world simulation for physics simulations
// ================================================================================================================================

using BEPUphysics.BroadPhaseEntries;
using BEPUutilities;
using Microsoft.Xna.Framework.Graphics;

namespace Server.Data
{
    public class TerrainMesh
    {
        public Vector3[] Vertices;
        public int[] Indices;
        public Model ModelData;
        public StaticMesh MeshCollider;

        public TerrainMesh(string MeshName)
        {
            l.og("Loading " + MeshName + " static level mesh...");
            //Load the terrain model data from file
            ModelData = Rendering.Window.Instance.Content.Load<Model>(MeshName);
            //Extract the models vertices and indices from the model data object
            ModelDataExtractor.GetVerticesAndIndicesFromModel(ModelData, out Vertices, out Indices);
            MeshCollider = new StaticMesh(Vertices, Indices, new AffineTransform(new Vector3(0,0,0)));
            
            //Add the mesh to the physics scene
            Physics.WorldSimulator.Space.Add(MeshCollider);
            l.ogEdit("Loading " + MeshName + " static level mesh... Complete!");
        }
    }
}
