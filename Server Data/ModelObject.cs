using System;
using Microsoft.Xna.Framework.Graphics;

namespace Swaelo_Server
{
    public class ModelObject
    {
        Model ModelData;
        Vector3[] ModelVertices;
        int[] ModelIndices;
        StaticMesh MeshData;

        public void LoadModelData(string ModelName, WorldRenderer Game)
        {
            ModelData = Game.Content.Load<Model>(ModelName);
            ModelDataExtractor.GetVerticesAndIndicesFromModel(ModelData, out ModelVertices, out ModelIndices);

            MeshData = new StaticMesh(ModelVertices, ModelIndices, new AffineTransform(new Vector3(0, 0, 0)));
            Globals.space.Add(MeshData);
            Globals.game.ModelDrawer.Add(MeshData);
        }
    }
}
