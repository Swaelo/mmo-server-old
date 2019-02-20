using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class NavMeshNode
    {
        public Vector3 NodeLocation = new Vector3(0, 0, 0);
        public Sphere NodeDisplay;
        public NavMeshNode[] NodeNeighbours = new NavMeshNode[8];
        
        public NavMeshNode CameFrom = null; //Each node knows which other node it can most efficiently be reached from
        public float GScore = float.MaxValue;   //For each node, the cost of to it from the start node
        public float FScore = float.MaxValue;   //For each node, the total cost of getting to it from the start note

        public Vector2 NodeIndex = new Vector2(0, 0);

        public NavMeshNode(Vector3 Location)
        {
            NodeLocation = Location;
            NodeDisplay = new Sphere(Location, 0.1f);
        }
    }
}
