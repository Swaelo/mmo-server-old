using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Swaelo_Server
{
    class Globals
    {
        public static Database database;
        public static Form MainWindowForm;
        public static NetworkingThread NetworkingThread = new NetworkingThread();
        public static PhysicsThread PhysicsThread = new PhysicsThread();
        public static WorldRenderer game = null;
        public static WorldSimulator world = null;
        public static Space space = null;
        public static Vector3[] NavMeshVertices;
        public static int[] NavMeshIndices;
        public static void AddSphere(Vector3 SphereLocation, float SphereRadius)
        {
            Sphere NewSphere = new Sphere(SphereLocation, SphereRadius);
           // Globals.space.Add(NewSphere);
            Globals.game.ModelDrawer.Add(NewSphere);
        }
        public static void AddBox(Vector3 BoxLocation, Vector3 BoxScale, Quaternion BoxRotation)
        {
            Box NewBox = new Box(BoxLocation, BoxScale.X, BoxScale.Y, BoxScale.Z);
            Globals.game.ModelDrawer.Add(NewBox);
        }
    }
}
