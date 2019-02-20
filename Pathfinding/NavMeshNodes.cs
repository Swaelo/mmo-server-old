using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class NavMeshNodes
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

        //Returns whatever nav mesh node is the closest to the target location
        public static NavMeshNode GetNearbyMeshNode(Vector3 NodeLocation)
        {
            NavMeshNode NearbyNode = MeshNodes[0];
            float NodeDistance = Vector3.Distance(NearbyNode.NodeLocation, NodeLocation);
            for(int i = 1; i < MeshNodes.Count; i++)
            {
                NavMeshNode NodeCompare = MeshNodes[i];
                float CompareDistance = Vector3.Distance(NodeCompare.NodeLocation, NodeLocation);
                if(CompareDistance < NodeDistance)
                {
                    NearbyNode = NodeCompare;
                    NodeDistance = CompareDistance;
                }
            }
            return NearbyNode;
        }
    }
}
