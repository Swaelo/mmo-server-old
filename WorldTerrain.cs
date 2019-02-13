using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    class WorldTerrain
    {
        public float[,] heights;
        public Terrain terrain;

        public WorldTerrain(int Length, int Width)
        {
            //load terrain heights
            int XLength = 48;
            int ZLength = 34;

            float XSpacing = 8f;
            float ZSpacing = 8f;

            heights = new float[XLength, ZLength];
            for(int i = 0; i < XLength; i++)
            {
                for (int j = 0; j < ZLength; j++)
                {
                    float x = i - XLength / 2;
                    float z = j - ZLength / 2;
                    heights[i, j] = (float)(10 * (Math.Sin(x / 8) + Math.Sin(z / 8)));
                }
            }
            //create terrain object with the height values
            terrain = new Terrain(heights, new AffineTransform(
                new Vector3(XSpacing, 1, ZSpacing),
                Quaternion.Identity,
                new Vector3(-XLength * XSpacing / 2, 0, -ZLength * ZSpacing / 2)));
            terrain.Shape.QuadTriangleOrganization = QuadTriangleOrganization.BottomLeftUpperRight;
            terrain.Thickness = 5;
            /*
            float LengthSpacing = 8f;
            float WidthSpacing = 8f;
            //x and y, in terms of heightmaps, refer to their local x and y coordinates.  In world space, they correspond to x and z.
            //Setup the heights of the terrain.
            //[The size here is limited by the Reach profile the demos use- the drawer draws the terrain as a big block and runs into primitive drawing limits.
            //The physics can support far larger terrains!]
            heights = new float[Width, Length];
            for(int WidthIter = 0; WidthIter < Width; WidthIter++)
            {
                for(int LengthIter = 0; LengthIter < Length; LengthIter++)
                {
                    float x = WidthIter - Width / 2;
                    float z = LengthIter - Length / 2;
                    heights[WidthIter, LengthIter] = (float)(10 * (Math.Sin(x / 8) + Math.Sin(z / 8)));
                }
            }

            //Create the terrain
            terrain = new Terrain(heights, new AffineTransform(
                new Vector3(WidthSpacing, 1, LengthSpacing),
                Quaternion.Identity,
                new Vector3(-Width * WidthSpacing / 2, 0, -Length * LengthSpacing / 2)));
            terrain.Shape.QuadTriangleOrganization = QuadTriangleOrganization.BottomRightUpperLeft;
            */
        }
    }
}
