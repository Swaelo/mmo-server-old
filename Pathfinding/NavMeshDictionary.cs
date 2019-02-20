using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    class NavMeshDictionary
    {
        public static Dictionary<Vector2, NavMeshNode> MeshDictionary = new Dictionary<Vector2, NavMeshNode>();

        public static void AddNode(NavMeshNode Node, Vector2 Index)
        {
            MeshDictionary.Add(Index, Node);
        }

        public static void ResetPathfinding()
        {
            foreach(var MeshNode in MeshDictionary)
            {
                MeshNode.Value.CameFrom = null;
                MeshNode.Value.GScore = float.MaxValue;
                MeshNode.Value.FScore = float.MaxValue;
            }
        }

        public static List<NavMeshNode> GetNeighbours(NavMeshNode TargetNode, Vector2 MeshSize, bool IncludeDiagonals)
        {
            Vector2 TargetNodeLocation = TargetNode.NodeIndex;
            List<NavMeshNode> Neighbours = new List<NavMeshNode>();

            //north neighbour
            if (TargetNodeLocation.Y + 1 < MeshSize.Y)
                Neighbours.Add(MeshDictionary[new Vector2(TargetNodeLocation.X, TargetNodeLocation.Y + 1)]);
            //east
            if (TargetNodeLocation.X + 1 < MeshSize.X)
                Neighbours.Add(MeshDictionary[new Vector2(TargetNodeLocation.X + 1, TargetNodeLocation.Y)]);
            //south
            if (TargetNodeLocation.Y - 1 > 0)
                Neighbours.Add(MeshDictionary[new Vector2(TargetNodeLocation.X, TargetNodeLocation.Y - 1)]);
            //west
            if (TargetNodeLocation.X - 1 > 0)
                Neighbours.Add(MeshDictionary[new Vector2(TargetNodeLocation.X - 1, TargetNodeLocation.Y)]);

            if (IncludeDiagonals)
            {
                //North-East
                if (TargetNodeLocation.Y + 1 < MeshSize.Y && TargetNodeLocation.X + 1 < MeshSize.X)
                    Neighbours.Add(MeshDictionary[new Vector2(TargetNodeLocation.X + 1, TargetNodeLocation.Y + 1)]);
                //South-East
                if (TargetNodeLocation.X + 1 < MeshSize.X && TargetNodeLocation.Y - 1 > 0)
                    Neighbours.Add(MeshDictionary[new Vector2(TargetNodeLocation.X + 1, TargetNodeLocation.Y - 1)]);
                //South-West
                if (TargetNodeLocation.X - 1 > 0 && TargetNodeLocation.Y - 1 > 0)
                    Neighbours.Add(MeshDictionary[new Vector2(TargetNodeLocation.X - 1, TargetNodeLocation.Y - 1)]);
                //North-West
                if (TargetNodeLocation.X - 1 > 0 && TargetNodeLocation.Y + 1 < MeshSize.Y)
                    Neighbours.Add(MeshDictionary[new Vector2(TargetNodeLocation.X - 1, TargetNodeLocation.Y + 1)]);
            }

                return Neighbours;
        }
    }
}
