using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public static class NavMeshManager
    {



        //public static List<NavMeshNode> NavMeshNodes = new List<NavMeshNode>();
        //public static void AddNewNode(Vector3 NewNodeLocation)
        //{
        //    NavMeshNodes.Add(new NavMeshNode(NewNodeLocation));
        //}
        //public static void AddExistingNode(NavMeshNode ExistingNode)
        //{
        //    NavMeshNodes.Add(ExistingNode);
        //}

        //public static bool IsNodeLocationAvailable(Vector3 NewNodeLocation)
        //{
        //    for (int i = 0; i < NavMeshNodes.Count; i++)
        //    {
        //        NavMeshNode CurrentNode = NavMeshNodes[i];
        //        if (CurrentNode.NodeLocation == NewNodeLocation)
        //            return false;
        //    }
        //    return true;
        //}
    }
}
















        /*
         * 
         * //Checks if any node already exists at this location or not
        public static bool IsNodeLocationAvailable(Vector3 NewNodeLocation)
        {
            //Go through the entire list of nodes that already exist in the nav mesh already
            for(int i = 0; i < NavMeshNodeCount; i++)
            {
                //Check each of the nodes
                NavMeshNode CurrentNode = NavMeshNodes[i];
                //Compare its distance to see if its already taken by this one
                if (CurrentNode.NodeLocation == NewNodeLocation)
                    return false;
            }
            //After checking all nodes without finding a match we know the new location is available
            return true;
        }

        //Track an active list of every node that has been added to the nav mesh, and how many nodes have been added to it
        public static Dictionary<int, NavMeshNode> NavMeshNodes = new Dictionary<int, NavMeshNode>();
        public static int NavMeshNodeCount = 0;

        //Store the nav mesh nodes into a dictionary sorted by their array index vectors
        public static Dictionary<Vector2, NavMeshNode> NavMeshNodeDictionary = new Dictionary<Vector2, NavMeshNode>();
        public static void AddNode(Vector2 NodeIndex, NavMeshNode MeshNode)
        {
            NavMeshNodeDictionary.Add(NodeIndex, MeshNode);
        }
        public static NavMeshNode GetNode(Vector2 NodeIndex)
        {
            return NavMeshNodeDictionary[NodeIndex];
        }

        //Returns the calculated path between the two given nodes
        public static List<NavMeshNode> FindPath(NavMeshNode StartNode, NavMeshNode EndNode)
        {
            //Initialise everything as it needs to be, ready to perform a new pathfinding search
            List<NavMeshNode> ClosedSet = new List<NavMeshNode>();
            List<NavMeshNode> OpenSet = new List<NavMeshNode>();
            OpenSet.Add(StartNode);
            NavMeshManager.ResetPathfindingValues();
            StartNode.GScore = 0;
            StartNode.FScore = Vector3.Distance(StartNode.NodeLocation, EndNode.NodeLocation);

            //Iterate over the open set until its empty or we find the pathway
            while(OpenSet.Count > 0)
            {
                //The current node is the member of the open set with the lowest f score value
                NavMeshNode CurrentNode = NavMeshManager.GetLowestFScore(OpenSet);
                //If this is the end node, then we path is complete
                if(CurrentNode == EndNode)
                {
                    Log.Out("path found");
                    return new List<NavMeshNode>();
                }
                //Otherwise we move the current node onto the closed list as it has been evaluated now
                OpenSet.Remove(CurrentNode);
                ClosedSet.Remove(CurrentNode);

                //Now we need to get the current nodes neighbours and search through them all
                
            }

            return new List<NavMeshNode>();
        }

        //Resets the pathfinding values of all nodes in the nav mesh
        public static void ResetPathfindingValues()
        {
            for(int i = 0; i < NavMeshNodeCount; i++)
            {
                NavMeshNodes[i].CameFrom = null;
                NavMeshNodes[i].GScore = float.MaxValue;
                NavMeshNodes[i].FScore = float.MaxValue;
            }
        }
        
        public static float HeuristicCostEstimate(NavMeshNode Start, NavMeshNode End)
        {
            return Vector3.Distance(Start.NodeLocation, End.NodeLocation);
        }

        //Returns the node in the open set with the lowest fscore value
        public static NavMeshNode GetLowestFScore(List<NavMeshNode> OpenSet)
        {
            NavMeshNode CurrentNode = OpenSet[0];
            float NodeScore = CurrentNode.FScore;
            for(int i = 1; i < OpenSet.Count; i++)
            {
                NavMeshNode OtherNode = OpenSet[i];
                float OtherNodeScore = OtherNode.FScore;
                if(OtherNodeScore < NodeScore)
                {
                    CurrentNode = OtherNode;
                    NodeScore = OtherNodeScore;
                }
            }
            return CurrentNode;
        }

        
        //Adds a new nav mesh node to the nav mesh
        public static void AddNewNode(NavMeshNode NewNode)
        {
            NavMeshNodes.Add(NavMeshNodeCount, NewNode);
            NavMeshNodeCount++;
        }

        //Adds a new nav mesh node to this location if its not already taken
        public static void AddNewNode(Vector3 NodeLocation)
        {
            bool LocationAvailable = IsNodeLocationAvailable(NodeLocation);
            if(LocationAvailable)
            {
                NavMeshNode NewNode = new NavMeshNode(NodeLocation);
                NavMeshNodes.Add(NavMeshNodeCount, NewNode);
                NavMeshNodeCount++;


                Globals.space.Add(new Sphere(NewNode.NodeLocation, 0.1f));
                Globals.game.ModelDrawer.Add(NewNode.NodeDisplay);
            }
        }

        //Searches through the entire list of nodes that have been added, and returns which one is closest to the target location
        public static NavMeshNode GetClosestNode(Vector3 TargetPosition)
        {
            //Start off assuming the first node is the closest one
            NavMeshNode ClosestNode = NavMeshNodes[0];
            float ClosestNodeDistance = Vector3.Distance(TargetPosition, ClosestNode.NodeLocation);
            //Compare it against all other nodes to check if any others are closer than this one, update if they are
            for(int i = 1; i < NavMeshNodeCount; i++)
            {
                NavMeshNode CompareNode = NavMeshNodes[i];
                float CompareNodeDistance = Vector3.Distance(TargetPosition, CompareNode.NodeLocation);
                if(CompareNodeDistance < ClosestNodeDistance)
                {
                    ClosestNode = CompareNode;
                    ClosestNodeDistance = CompareNodeDistance;
                }
            }
            //After searching through all of them, now we know which one is closest
            return ClosestNode;
        }

        public static List<NavMeshNode> GetOtherNodes(NavMeshNode TargetNode)
        {
            List<NavMeshNode> OtherNodes = new List<NavMeshNode>();
            //Go through the entire list of nodes that exist in the nav mesh
            for(int i = 0; i < NavMeshNodeCount; i++)
            {
                NavMeshNode OtherNode = NavMeshNodes[i];
                //Place into the list every node that isnt this one
                if (TargetNode == OtherNode)
                    continue;
                OtherNodes.Add(OtherNode);
            }
            //Return the list after all other nodes have been added to it
            return OtherNodes;
        }

        public static List<NavMeshNode> GetNodeList()
        {
            List<NavMeshNode> NodeList = new List<NavMeshNode>();
            for(int i = 0; i < NavMeshNodeCount; i++)
            {
                NodeList.Add(NavMeshNodes[i]);
            }
            return NodeList;
        }
        

        public static List<Vector2> GetNeighbouringNodeLocations(Vector2 TargetNode, Vector2 MeshSize)
        {
            //Define a list to which we will add all of a target nodes neighbouring node locations
            List<Vector2> NeighbourNodes = new List<Vector2>();

            //Each node will have a maximum of 8 neighbours depending on their location within the mesh
            //Nodes along the bottom row cannot have southern neighbours, top row cant have northern neighbours etc
            //North Neighbour
            if (TargetNode.Y + 1 <= MeshSize.Y)
                NeighbourNodes.Add(new Vector2(TargetNode.X, TargetNode.Y + 1));
            //North-East Neighbour
            if (TargetNode.X + 1 <= MeshSize.X && TargetNode.Y + 1 <= MeshSize.Y)
                NeighbourNodes.Add(new Vector2(TargetNode.X + 1, TargetNode.Y + 1));
            //East Neighbour
            if (TargetNode.X + 1 <= MeshSize.X)
                NeighbourNodes.Add(new Vector2(TargetNode.X + 1, TargetNode.Y));
            //South-East Neighbour
            if (TargetNode.X + 1 <= MeshSize.X && TargetNode.Y - 1 > 0)
                NeighbourNodes.Add(new Vector2(TargetNode.X + 1, TargetNode.Y - 1));
            //South Neighbour
            if (TargetNode.Y - 1 > 0)
                NeighbourNodes.Add(new Vector2(TargetNode.X, TargetNode.Y - 1));
            //South-West Neighbour
            if (TargetNode.X - 1 > 0 && TargetNode.Y - 1 > 0)
                NeighbourNodes.Add(new Vector2(TargetNode.X - 1, TargetNode.Y - 1));
            //West Neighbour
            if (TargetNode.X - 1 > 0)
                NeighbourNodes.Add(new Vector2(TargetNode.X - 1, TargetNode.Y));
            //North-West Neighbour
            if (TargetNode.X - 1 > 0 && TargetNode.Y + 1 <= MeshSize.Y)
                NeighbourNodes.Add(new Vector2(TargetNode.X - 1, TargetNode.Y + 1));

            //return all the neighbour locations
            return NeighbourNodes;
        }
    }
    */
    
