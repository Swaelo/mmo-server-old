// ================================================================================================================================
// File:        TerrainMesh.cs
// Description: Used to load in the terrain mesh file so it can be used for the server side physics calculations
// Author:      Harley Laurie          
// Notes:       
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
    public class TerrainMesh
    {
        public Vector3[] Vertices;
        public int[] Indices;
        public Model ModelData;
        public StaticMesh MeshCollider;

        public TerrainMesh(string MeshName)
        {
            //Load the terrain model data from file
            ModelData = Globals.game.Content.Load<Model>("TerrainMesh");
            //Extract the models vertices and indices from the model data object
            ModelDataExtractor.GetVerticesAndIndicesFromModel(ModelData, out Vertices, out Indices);
            MeshCollider = new StaticMesh(Vertices, Indices, new AffineTransform(new Vector3(0, 0, 0)));
            //Add the mesh to the physics scene
            Globals.space.Add(MeshCollider);
        }
    }
}
