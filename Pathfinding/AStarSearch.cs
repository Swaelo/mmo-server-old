using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public static class AStarSearch
    {
        private static List<NavMeshNode> ClosedSet = new List<NavMeshNode>();
        private static List<NavMeshNode> OpenSet = new List<NavMeshNode>();



        public static List<NavMeshNode> FindPath(NavMeshNode StartNode, NavMeshNode EndNode, Vector2 NavMeshSize)
        {
            //Initialize the Open and Closed sets
            ClosedSet = new List<NavMeshNode>();
            OpenSet = new List<NavMeshNode>();
            OpenSet.Add(StartNode);
            //Reset nodes CameFrom references in preperation for finding our new pathway
            NavMeshDictionary.ResetPathfinding();
            //Cost of travelling from the start node to the start node is 0
            StartNode.GScore = 0;
            //The FScore for the starting node should be completely heuristic
            StartNode.FScore = Vector3.Distance(StartNode.NodeLocation, EndNode.NodeLocation);

            //Iterate over the OpenSet until there are no more nodes left to evaluate
            while(OpenSet.Count > 0)
            {
                //The current node each time will be the node in OpenSet with the lowest FScore
                NavMeshNode CurrentNode = GetCurrentNode(OpenSet);

                //If this is the end node then the path is ready
                if(CurrentNode == EndNode)
                {
                    //When the path is found, we start at the end node, and keep following back each nodes CameFrom reference, until we end up at the start node
                    List<NavMeshNode> FinishedPath = new List<NavMeshNode>();
                    FinishedPath.Add(EndNode);
                    NavMeshNode PathNode = EndNode;
                    while(PathNode != StartNode)
                    {
                        PathNode = PathNode.CameFrom;
                        FinishedPath.Add(PathNode);
                    }
                    FinishedPath.Add(StartNode);
                    FinishedPath.Reverse();
                    return FinishedPath;
                }

                //Move the current node over to the closed set
                OpenSet.Remove(CurrentNode);
                ClosedSet.Add(CurrentNode);

                //Find all of the current nodes neighbours
                List<NavMeshNode> CurrentNeighbours = NavMeshDictionary.GetNeighbours(CurrentNode, new Vector2(9, 9), true);
                for(int i = 0; i < CurrentNeighbours.Count; i++)
                {
                    //Check through all of them
                    NavMeshNode CurrentNeighbour = CurrentNeighbours[i];

                    //Ignore neighbours in the closed set as they have already been evaluated
                    if (ClosedSet.Contains(CurrentNeighbour))
                        continue;

                    //Calculate the distance from start to this neighbour
                    float TentativeGScore = CurrentNode.GScore + Vector3.Distance(CurrentNode.NodeLocation, CurrentNeighbour.NodeLocation);

                    //Add all newly discovered nodes into the open set
                    if (!OpenSet.Contains(CurrentNeighbour))
                        OpenSet.Add(CurrentNeighbour);
                    //make sure this node isnt more expensive to travel from
                    else if (TentativeGScore >= CurrentNeighbour.GScore)
                        continue;

                    //If we get this far the current neighbour has been found to be the cheapest node to travel from
                    CurrentNeighbour.CameFrom = CurrentNode;
                    CurrentNeighbour.GScore = TentativeGScore;
                    CurrentNeighbour.FScore = CurrentNeighbour.GScore + Vector3.Distance(CurrentNeighbour.NodeLocation, EndNode.NodeLocation);
                }
            }

            return new List<NavMeshNode>();
        }

        public static NavMeshNode GetCurrentNode(List<NavMeshNode> OpenSet)
        {
            NavMeshNode CurrentNode = OpenSet[0];
            float CurrentFScore = CurrentNode.FScore;
            for(int i = 1; i < OpenSet.Count; i++)
            {
                NavMeshNode CompareNode = OpenSet[i];
                float CompareFScore = CompareNode.FScore;
                //We are trying to find which node has the lowest FScore value
                if(CompareFScore < CurrentFScore)
                {
                    CurrentNode = CompareNode;
                    CurrentFScore = CompareFScore;
                }
            }
            return CurrentNode;
        }
    }
}
