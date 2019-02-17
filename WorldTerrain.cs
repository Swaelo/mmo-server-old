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
        }
    }
}
