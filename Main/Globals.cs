// ================================================================================================================================
// File:        Globals.cs
// Description: The main, largest components which make up the entire server are stored here so they can easily access each other
// Author:      Harley Laurie          
// Notes:       
// ================================================================================================================================

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

        //Adds a sphere to the scene, only to be rendered, this will not interact with physics
        public static void AddSphere(Vector3 SphereLocation, float SphereRadius)
        {
            Sphere NewSphere = new Sphere(SphereLocation, SphereRadius);
            Globals.game.ModelDrawer.Add(NewSphere);
        }

        //Adds a box to the scene, only to be rendered, this will not interact with physics
        public static void AddBox(Vector3 BoxLocation, Vector3 BoxScale, Quaternion BoxRotation)
        {
            Box NewBox = new Box(BoxLocation, BoxScale.X, BoxScale.Y, BoxScale.Z);
            Globals.game.ModelDrawer.Add(NewBox);
        }

        //Adds a static mesh object to both the physics scene and the model drawer to be rendered
        public static void AddToBoth(StaticMesh MeshCollider)
        {
            Globals.space.Add(MeshCollider);
            Globals.game.ModelDrawer.Add(MeshCollider);
        }

        //Adds a space object to the server physics space with all the rest
        public static void AddToSpace(ISpaceObject SpaceObject)
        {
            Globals.space.Add(SpaceObject);
        }

        //Adds a display object to the model drawer to be rendered with everything else
        public static void AddToRenderer(ModelDisplayObject DisplayObject)
        {
            Globals.game.ModelDrawer.Add(DisplayObject);
        }
    }
}
