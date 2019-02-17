using System;
using System.Collections.Generic;
using System.Text;

namespace Swaelo_Server
{
    class Globals
    {
        public static Database database = new Database();
        public static WorldRenderer game = null;
        public static WorldSimulator world = null;
        public static Space space = null;
        public static Vector3[] TerrainVerts;
        public static int[] TerrainInds;
        
        public static void AddSphere(Vector3 SphereLocation, float SphereRadius)
        {
            Sphere NewSphere = new Sphere(SphereLocation, SphereRadius);
           // Globals.space.Add(NewSphere);
            Globals.game.ModelDrawer.Add(NewSphere);
        }
    }
}
