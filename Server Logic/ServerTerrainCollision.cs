using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Swaelo_Server
{
    class ServerTerrainCollision
    {
        public Vector3[] TerrainVertices;
        public int[] TerrainIndices;
        public Model TerrainCollision;
        public StaticMesh TerrainMesh;


        //public default constructor
        public ServerTerrainCollision(string ContentFileName)
        {
            TerrainCollision = Globals.game.Content.Load<Model>("LowDetailTerrain");
            ModelDataExtractor.GetVerticesAndIndicesFromModel(TerrainCollision, out TerrainVertices, out TerrainIndices);
            TerrainMesh = new StaticMesh(TerrainVertices, TerrainIndices, new AffineTransform(new Vector3(0, 0, 0)));
            Globals.TerrainVerts = TerrainVertices;
            Globals.TerrainInds = TerrainIndices;
            Globals.space.Add(TerrainMesh);
        }
    }
}
