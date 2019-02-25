using Microsoft.Xna.Framework;

namespace Swaelo_Server
{
    /// <summary>
    /// Helps convert between XNA math types and the BEPUphysics replacement math types.
    /// A version of this converter could be created for other platforms to ease the integration of the engine.
    /// </summary>
    public static class MathConverter
    {
        //Vector2
        public static Microsoft.Xna.Framework.Vector2 Convert(Vector2 bepuVector)
        {
            Microsoft.Xna.Framework.Vector2 toReturn;
            toReturn.X = bepuVector.X;
            toReturn.Y = bepuVector.Y;
            return toReturn;
        }

        public static void Convert(ref Vector2 bepuVector, out Microsoft.Xna.Framework.Vector2 xnaVector)
        {
            xnaVector.X = bepuVector.X;
            xnaVector.Y = bepuVector.Y;
        }

        public static Vector2 Convert(Microsoft.Xna.Framework.Vector2 xnaVector)
        {
            Vector2 toReturn;
            toReturn.X = xnaVector.X;
            toReturn.Y = xnaVector.Y;
            return toReturn;
        }

        public static void Convert(ref Microsoft.Xna.Framework.Vector2 xnaVector, out Vector2 bepuVector)
        {
            bepuVector.X = xnaVector.X;
            bepuVector.Y = xnaVector.Y;
        }

        //Vector3
        public static Microsoft.Xna.Framework.Vector3 Convert(Vector3 bepuVector)
        {
            Microsoft.Xna.Framework.Vector3 toReturn;
            toReturn.X = bepuVector.X;
            toReturn.Y = bepuVector.Y;
            toReturn.Z = bepuVector.Z;
            return toReturn;
        }

        public static void Convert(ref Vector3 bepuVector, out Microsoft.Xna.Framework.Vector3 xnaVector)
        {
            xnaVector.X = bepuVector.X;
            xnaVector.Y = bepuVector.Y;
            xnaVector.Z = bepuVector.Z;
        }

        public static Vector3 Convert(Microsoft.Xna.Framework.Vector3 xnaVector)
        {
            Vector3 toReturn;
            toReturn.X = xnaVector.X;
            toReturn.Y = xnaVector.Y;
            toReturn.Z = xnaVector.Z;
            return toReturn;
        }

        public static void Convert(ref Microsoft.Xna.Framework.Vector3 xnaVector, out Vector3 bepuVector)
        {
            bepuVector.X = xnaVector.X;
            bepuVector.Y = xnaVector.Y;
            bepuVector.Z = xnaVector.Z;
        }

        public static Microsoft.Xna.Framework.Vector3[] Convert(Vector3[] bepuVectors)
        {
            Microsoft.Xna.Framework.Vector3[] xnaVectors = new Microsoft.Xna.Framework.Vector3[bepuVectors.Length];
            for (int i = 0; i < bepuVectors.Length; i++)
            {
                Convert(ref bepuVectors[i], out xnaVectors[i]);
            }
            return xnaVectors;

        }

        public static Vector3[] Convert(Microsoft.Xna.Framework.Vector3[] xnaVectors)
        {
            var bepuVectors = new Vector3[xnaVectors.Length];
            for (int i = 0; i < xnaVectors.Length; i++)
            {
                Convert(ref xnaVectors[i], out bepuVectors[i]);
            }
            return bepuVectors;

        }

        //Matrix
        public static Microsoft.Xna.Framework.Matrix Convert(Matrix matrix)
        {
            Microsoft.Xna.Framework.Matrix toReturn;
            Convert(ref matrix, out toReturn);
            return toReturn;
        }

        public static Matrix Convert(Microsoft.Xna.Framework.Matrix matrix)
        {
            Matrix toReturn;
            Convert(ref matrix, out toReturn);
            return toReturn;
        }

        public static void Convert(ref Matrix matrix, out Microsoft.Xna.Framework.Matrix xnaMatrix)
        {
            xnaMatrix.M11 = matrix.M11;
            xnaMatrix.M12 = matrix.M12;
            xnaMatrix.M13 = matrix.M13;
            xnaMatrix.M14 = matrix.M14;

            xnaMatrix.M21 = matrix.M21;
            xnaMatrix.M22 = matrix.M22;
            xnaMatrix.M23 = matrix.M23;
            xnaMatrix.M24 = matrix.M24;

            xnaMatrix.M31 = matrix.M31;
            xnaMatrix.M32 = matrix.M32;
            xnaMatrix.M33 = matrix.M33;
            xnaMatrix.M34 = matrix.M34;

            xnaMatrix.M41 = matrix.M41;
            xnaMatrix.M42 = matrix.M42;
            xnaMatrix.M43 = matrix.M43;
            xnaMatrix.M44 = matrix.M44;

        }

        public static void Convert(ref Microsoft.Xna.Framework.Matrix matrix, out Matrix bepuMatrix)
        {
            bepuMatrix.M11 = matrix.M11;
            bepuMatrix.M12 = matrix.M12;
            bepuMatrix.M13 = matrix.M13;
            bepuMatrix.M14 = matrix.M14;

            bepuMatrix.M21 = matrix.M21;
            bepuMatrix.M22 = matrix.M22;
            bepuMatrix.M23 = matrix.M23;
            bepuMatrix.M24 = matrix.M24;

            bepuMatrix.M31 = matrix.M31;
            bepuMatrix.M32 = matrix.M32;
            bepuMatrix.M33 = matrix.M33;
            bepuMatrix.M34 = matrix.M34;

            bepuMatrix.M41 = matrix.M41;
            bepuMatrix.M42 = matrix.M42;
            bepuMatrix.M43 = matrix.M43;
            bepuMatrix.M44 = matrix.M44;

        }

        public static Microsoft.Xna.Framework.Matrix Convert(Matrix3x3 matrix)
        {
            Microsoft.Xna.Framework.Matrix toReturn;
            Convert(ref matrix, out toReturn);
            return toReturn;
        }

        public static void Convert(ref Matrix3x3 matrix, out Microsoft.Xna.Framework.Matrix xnaMatrix)
        {
            xnaMatrix.M11 = matrix.M11;
            xnaMatrix.M12 = matrix.M12;
            xnaMatrix.M13 = matrix.M13;
            xnaMatrix.M14 = 0;

            xnaMatrix.M21 = matrix.M21;
            xnaMatrix.M22 = matrix.M22;
            xnaMatrix.M23 = matrix.M23;
            xnaMatrix.M24 = 0;

            xnaMatrix.M31 = matrix.M31;
            xnaMatrix.M32 = matrix.M32;
            xnaMatrix.M33 = matrix.M33;
            xnaMatrix.M34 = 0;

            xnaMatrix.M41 = 0;
            xnaMatrix.M42 = 0;
            xnaMatrix.M43 = 0;
            xnaMatrix.M44 = 1;
        }

        public static void Convert(ref Matrix matrix, out Matrix3x3 bepuMatrix)
        {
            bepuMatrix.M11 = matrix.M11;
            bepuMatrix.M12 = matrix.M12;
            bepuMatrix.M13 = matrix.M13;

            bepuMatrix.M21 = matrix.M21;
            bepuMatrix.M22 = matrix.M22;
            bepuMatrix.M23 = matrix.M23;

            bepuMatrix.M31 = matrix.M31;
            bepuMatrix.M32 = matrix.M32;
            bepuMatrix.M33 = matrix.M33;

        }

        //Quaternion
        public static Microsoft.Xna.Framework.Quaternion Convert(Quaternion quaternion)
        {
            Microsoft.Xna.Framework.Quaternion toReturn;
            toReturn.X = quaternion.X;
            toReturn.Y = quaternion.Y;
            toReturn.Z = quaternion.Z;
            toReturn.W = quaternion.W;
            return toReturn;
        }

        public static Quaternion Convert(Microsoft.Xna.Framework.Quaternion quaternion)
        {
            Quaternion toReturn;
            toReturn.X = quaternion.X;
            toReturn.Y = quaternion.Y;
            toReturn.Z = quaternion.Z;
            toReturn.W = quaternion.W;
            return toReturn;
        }

        public static void Convert(ref Quaternion bepuQuaternion, out Microsoft.Xna.Framework.Quaternion quaternion)
        {
            quaternion.X = bepuQuaternion.X;
            quaternion.Y = bepuQuaternion.Y;
            quaternion.Z = bepuQuaternion.Z;
            quaternion.W = bepuQuaternion.W;
        }

        public static void Convert(ref Microsoft.Xna.Framework.Quaternion quaternion, out Quaternion bepuQuaternion)
        {
            bepuQuaternion.X = quaternion.X;
            bepuQuaternion.Y = quaternion.Y;
            bepuQuaternion.Z = quaternion.Z;
            bepuQuaternion.W = quaternion.W;
        }

        //Ray
        public static Ray Convert(Microsoft.Xna.Framework.Ray ray)
        {
            Ray toReturn;
            Convert(ref ray.Position, out toReturn.Position);
            Convert(ref ray.Direction, out toReturn.Direction);
            return toReturn;
        }

        public static void Convert(ref Microsoft.Xna.Framework.Ray ray, out Ray bepuRay)
        {
            Convert(ref ray.Position, out bepuRay.Position);
            Convert(ref ray.Direction, out bepuRay.Direction);
        }

        public static Microsoft.Xna.Framework.Ray Convert(Ray ray)
        {
            Microsoft.Xna.Framework.Ray toReturn;
            Convert(ref ray.Position, out toReturn.Position);
            Convert(ref ray.Direction, out toReturn.Direction);
            return toReturn;
        }

        public static void Convert(ref Ray ray, out Microsoft.Xna.Framework.Ray xnaRay)
        {
            Convert(ref ray.Position, out xnaRay.Position);
            Convert(ref ray.Direction, out xnaRay.Direction);
        }

        //BoundingBox
        public static Microsoft.Xna.Framework.BoundingBox Convert(BoundingBox boundingBox)
        {
            Microsoft.Xna.Framework.BoundingBox toReturn;
            Convert(ref boundingBox.Min, out toReturn.Min);
            Convert(ref boundingBox.Max, out toReturn.Max);
            return toReturn;
        }

        public static BoundingBox Convert(Microsoft.Xna.Framework.BoundingBox boundingBox)
        {
            BoundingBox toReturn;
            Convert(ref boundingBox.Min, out toReturn.Min);
            Convert(ref boundingBox.Max, out toReturn.Max);
            return toReturn;
        }

        public static void Convert(ref BoundingBox boundingBox, out Microsoft.Xna.Framework.BoundingBox xnaBoundingBox)
        {
            Convert(ref boundingBox.Min, out xnaBoundingBox.Min);
            Convert(ref boundingBox.Max, out xnaBoundingBox.Max);
        }

        public static void Convert(ref Microsoft.Xna.Framework.BoundingBox boundingBox, out BoundingBox bepuBoundingBox)
        {
            Convert(ref boundingBox.Min, out bepuBoundingBox.Min);
            Convert(ref boundingBox.Max, out bepuBoundingBox.Max);
        }

        //BoundingSphere
        public static Microsoft.Xna.Framework.BoundingSphere Convert(BoundingSphere boundingSphere)
        {
            Microsoft.Xna.Framework.BoundingSphere toReturn;
            Convert(ref boundingSphere.Center, out toReturn.Center);
            toReturn.Radius = boundingSphere.Radius;
            return toReturn;
        }

        public static BoundingSphere Convert(Microsoft.Xna.Framework.BoundingSphere boundingSphere)
        {
            BoundingSphere toReturn;
            Convert(ref boundingSphere.Center, out toReturn.Center);
            toReturn.Radius = boundingSphere.Radius;
            return toReturn;
        }

        public static void Convert(ref BoundingSphere boundingSphere, out Microsoft.Xna.Framework.BoundingSphere xnaBoundingSphere)
        {
            Convert(ref boundingSphere.Center, out xnaBoundingSphere.Center);
            xnaBoundingSphere.Radius = boundingSphere.Radius;
        }

        public static void Convert(ref Microsoft.Xna.Framework.BoundingSphere boundingSphere, out BoundingSphere bepuBoundingSphere)
        {
            Convert(ref boundingSphere.Center, out bepuBoundingSphere.Center);
            bepuBoundingSphere.Radius = boundingSphere.Radius;
        }

        //Plane
        public static Microsoft.Xna.Framework.Plane Convert(Plane plane)
        {
            Microsoft.Xna.Framework.Plane toReturn;
            Convert(ref plane.Normal, out toReturn.Normal);
            toReturn.D = plane.D;
            return toReturn;
        }

        public static Plane Convert(Microsoft.Xna.Framework.Plane plane)
        {
            Plane toReturn;
            Convert(ref plane.Normal, out toReturn.Normal);
            toReturn.D = plane.D;
            return toReturn;
        }

        public static void Convert(ref Plane plane, out Microsoft.Xna.Framework.Plane xnaPlane)
        {
            Convert(ref plane.Normal, out xnaPlane.Normal);
            xnaPlane.D = plane.D;
        }

        public static void Convert(ref Microsoft.Xna.Framework.Plane plane, out Plane bepuPlane)
        {
            Convert(ref plane.Normal, out bepuPlane.Normal);
            bepuPlane.D = plane.D;
        }
    }
}
