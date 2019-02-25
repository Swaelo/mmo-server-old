using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Swaelo_Server
{
    /// <summary>
    /// Helper class that can create shape mesh data.
    /// </summary>
    public static class DisplayTriangle
    {


        public static void GetShapeMeshData(EntityCollidable collidable, List<VertexPositionNormalTexture> vertices, List<ushort> indices)
        {
            var triangleShape = collidable.Shape as TriangleShape;
            if (triangleShape == null)
                throw new ArgumentException("Wrong shape type.");
            Microsoft.Xna.Framework.Vector3 normal = MathConverter.Convert(triangleShape.GetLocalNormal());
            vertices.Add(new VertexPositionNormalTexture(MathConverter.Convert(triangleShape.VertexA), -normal, new Microsoft.Xna.Framework.Vector2(0, 0)));
            vertices.Add(new VertexPositionNormalTexture(MathConverter.Convert(triangleShape.VertexB), -normal, new Microsoft.Xna.Framework.Vector2(0, 1)));
            vertices.Add(new VertexPositionNormalTexture(MathConverter.Convert(triangleShape.VertexC), -normal, new Microsoft.Xna.Framework.Vector2(1, 0)));

            vertices.Add(new VertexPositionNormalTexture(MathConverter.Convert(triangleShape.VertexA), normal, new Microsoft.Xna.Framework.Vector2(0, 0)));
            vertices.Add(new VertexPositionNormalTexture(MathConverter.Convert(triangleShape.VertexB), normal, new Microsoft.Xna.Framework.Vector2(0, 1)));
            vertices.Add(new VertexPositionNormalTexture(MathConverter.Convert(triangleShape.VertexC), normal, new Microsoft.Xna.Framework.Vector2(1, 0)));

            indices.Add(0);
            indices.Add(1);
            indices.Add(2);

            indices.Add(3);
            indices.Add(5);
            indices.Add(4);
        }
    }
}
