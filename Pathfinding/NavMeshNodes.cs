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
        public static Dictionary<Vector3, NavMeshNode> LocationMeshNodes = new Dictionary<Vector3, NavMeshNode>();

        //Adds a new mesh node objects to the list
        public static void AddNode(NavMeshNode Node)
        {
            MeshNodes.Add(Node);
            LocationMeshNodes.Add(Node.NodeLocation, Node);
        }

        //Gets a mesh node from the dictionary with its world location
        public static NavMeshNode GetNode(Vector3 NodePosition)
        {
            return LocationMeshNodes[NodePosition];
        }

        //Returns a list of all the nodes except for 1 that will not be in the list
        public static List<NavMeshNode> GetOtherNodes(NavMeshNode TargetNode)
        {
            List<NavMeshNode> OtherNodes = new List<NavMeshNode>();
            for(int i = 0; i < MeshNodes.Count; i++)
            {
                if (MeshNodes[i] != TargetNode)
                    OtherNodes.Add(MeshNodes[i]);
            }
            return OtherNodes;
        }

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

        public static NavMeshNode GetNearestNode(List<NavMeshNode> NodeList)
        {
            NavMeshNode ClosestNode = NodeList[0];
            for(int i = 0; i < NodeList.Count; i++)
            {
                if (NodeList[i].NeighbourDistance < ClosestNode.NeighbourDistance)
                    ClosestNode = NodeList[i];
            }
            return ClosestNode;
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
