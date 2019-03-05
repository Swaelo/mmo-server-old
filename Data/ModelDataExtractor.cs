using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Server.Data
{
    //Extracts vertices and indices from XNA models
    public static class ModelDataExtractor
    {
        //Gets an array of vertices and indices from the provided model
        public static void GetVerticesAndIndicesFromModel(Model CollisionModel, out BEPUutilities.Vector3[] Vertices, out int[] Indices)
        {
            List<Vector3> VerticesList = new List<Vector3>();
            List<int> IndicesList = new List<int>();
            Matrix[] Transforms = new Microsoft.Xna.Framework.Matrix[CollisionModel.Bones.Count];
            CollisionModel.CopyAbsoluteBoneTransformsTo(Transforms);

            Microsoft.Xna.Framework.Matrix Transform;
            foreach(ModelMesh Mesh in CollisionModel.Meshes)
            {
                if (Mesh.ParentBone != null)
                    Transform = Transforms[Mesh.ParentBone.Index];
                else
                    Transform = Microsoft.Xna.Framework.Matrix.Identity;
                AddMesh(Mesh, Transform, VerticesList, IndicesList);
            }

            List<BEPUutilities.Vector3> BEPUVertices = new List<BEPUutilities.Vector3>();
            foreach (Vector3 Vector in VerticesList)
                BEPUVertices.Add(new BEPUutilities.Vector3(Vector.X, Vector.Y, Vector.Z));

            Vertices = BEPUVertices.ToArray();
            Indices = IndicesList.ToArray();
        }

        //Adds a meshes vertices and indices to the given lists
        public static void AddMesh(ModelMesh CollisionModelMesh, Matrix Transform, List<Vector3> Vertices, List<int> Indices)
        {
            foreach(ModelMeshPart MeshPart in CollisionModelMesh.MeshParts)
            {
                int StartIndex = Vertices.Count;
                var MeshPartVertices = new Vector3[MeshPart.NumVertices];
                int Stride = MeshPart.VertexBuffer.VertexDeclaration.VertexStride;
                MeshPart.VertexBuffer.GetData(
                    MeshPart.VertexOffset * Stride,
                    MeshPartVertices,
                    0,
                    MeshPart.NumVertices,
                    Stride);

                //Transform it so its vertices are located in model space instead of mesh part space
                Vector3.Transform(MeshPartVertices, ref Transform, MeshPartVertices);
                Vertices.AddRange(MeshPartVertices);

                if(MeshPart.IndexBuffer.IndexElementSize == IndexElementSize.ThirtyTwoBits)
                {
                    var MeshIndices = new int[MeshPart.PrimitiveCount * 3];
                    MeshPart.IndexBuffer.GetData(MeshPart.StartIndex * 4, MeshIndices, 0, MeshPart.PrimitiveCount * 3);
                    for (int k = 0; k < MeshIndices.Length; k++)
                        Indices.Add(StartIndex + MeshIndices[k]);
                }
                else
                {
                    var MeshIndices = new ushort[MeshPart.PrimitiveCount * 3];
                    MeshPart.IndexBuffer.GetData(MeshPart.StartIndex * 2, MeshIndices, 0, MeshPart.PrimitiveCount * 3);
                    for (int k = 0; k < MeshIndices.Length; k++)
                        Indices.Add(StartIndex + MeshIndices[k]);
                }
            }
        }
    }
}
