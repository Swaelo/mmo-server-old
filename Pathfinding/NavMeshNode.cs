// ================================================================================================================================
// File:        NavMeshNode.cs
// Description: Defines one of the triangle meshes which make up part of the navigation mesh, lists its neighbours and the vertices
//              which make up the 3 points of the triangle
// ================================================================================================================================

using System.Collections.Generic;
using BEPUutilities;

namespace Server.Pathfinding
{
    public class NavMeshNode
    {
        public List<NavMeshNode> Neighbours = new List<NavMeshNode>();  //The list of nodes which are neighbours to this mesh node
        public List<NavMeshVertex> NodeVertices = new List<NavMeshVertex>();    //The 3 vertex locations for each corner of this node
        public List<Vector3> VertexLocations = new List<Vector3>();

        public NavMeshNode(Vector3[] VertexPositions)
        {
            for (int i = 0; i < 3; i++)
                VertexLocations.Add(VertexPositions[i]);
        }
    }
}
