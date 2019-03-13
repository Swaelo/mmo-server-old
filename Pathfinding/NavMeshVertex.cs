// ================================================================================================================================
// File:        NavMeshVertex.cs
// Description: Defines a single vertex in a NavMesh, lists which other vertices in the NavMesh it is linked to, and stored values
//              used for pathfinding while computing a pathway with A* searches
// ================================================================================================================================

using System.Collections.Generic;
using BEPUutilities;

namespace Server.Pathfinding
{
    public class NavMeshVertex
    {
        public Vector3 VertexLocation = Vector3.Zero;   //This vertices world location

        //Neighbouring Vertices
        public List<NavMeshVertex> VertexNeighbours = new List<NavMeshVertex>();    //The adjacent verices connected to
        public void LinkVertices(NavMeshVertex OtherVertex)
        {//Assign the two vertices as neighbours to one another
            if (!VertexNeighbours.Contains(OtherVertex))
                VertexNeighbours.Add(OtherVertex);
            if (!OtherVertex.VertexNeighbours.Contains(this))
                OtherVertex.VertexNeighbours.Add(this);
        }

        //Pathfinding values
        public NavMeshVertex Parent = null; //Which node to travel to next to traverse along the computed pathway
        public float GScore;    //Cost to travel from the starting vertex to this vertex
        public float FScore;    //Cost to travel from this vertex to the ending vertex
        
        //Default Constructor
        public NavMeshVertex(Vector3 VertexLocation)
        {
            this.VertexLocation = VertexLocation;
        }
    }
}
