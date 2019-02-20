using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    class NavMeshNodes
    {
        //Stores a list of every nav mesh node object that has been added thus far
        public static List<NavMeshNode> MeshNodes = new List<NavMeshNode>();

        //Adds a new mesh node objects to the list
        public static void AddNode(NavMeshNode Node) { MeshNodes.Add(Node); }

        //Checks that none of the existing mesh nodes are in this location
        public static bool IsLocationAvailable(Vector3 Location)
        {
            for(int i = 0; i < MeshNodes.Count; i++)
            {
                if (MeshNodes[i].NodeLocation == Location)
                    return false;
            }
            return true;
        }
    }
}
