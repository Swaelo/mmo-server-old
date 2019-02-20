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

        

        ////Takes in a list of all nav mesh nodes, and figures out which ones at the 9 closest neighbours
        //public void CalculateClosestNeighbours(NavMeshNode[] NavMeshNodes)
        //{
        //    //Assign the first 9 nodes in the nav mesh as the 9 currently closest node neighbours in the nav mesh
        //    NavMeshNode[] NodeNeighbours = new NavMeshNode[9];
        //    float[] NodeDistances = new float[9];
        //    for (int i = 0; i < 9; i++)
        //    {
        //        //Assign the first 9 neighbours and calculate their distances
        //        NodeNeighbours[i] = 
        //    }


        //    ////Create an ordered list which each neighbour will be added to, the list will keep them sorted
        //    //OrderedList OrderedNeighbours = new OrderedList();
        //    ////Add the first 9 nodes in the nav mesh to this list as the starting 9 neighbours for this node
        //    //for(int i = 0; i < 9; i++)
        //    //{
        //    //    //Create a new list object to store each neighbour node into the ordered list, do the first 9
        //    //    ListObject NeighbourObject = new ListObject(NavMeshNodes[i].NodeLocation);
        //    //    //Calculate the distance from the parent node to each of the new neighbour nodes as they are added to the list
        //    //    float NewNeighbourDistance = Vector3.Distance(NodeLocation, NeighbourObject.ObjectPosition);
        //    //    //Store that distance value inside each neighbour object that we create
        //    //    NeighbourObject.ListValue = NewNeighbourDistance;
        //    //    //Now the new neighbour object has been created and evaluated, place it into a auto sorted list by order of distance from parent node
        //    //    OrderedNeighbours.AddObject(NeighbourObject, NewNeighbourDistance);
        //public string NodeID = "-1";   //Unique NavMeshNode ID
        //public int NodeCounter = -1; //This nodes counter in the nav mesh map array
        //public Sphere NodeDisplay = null;  //Sphere rendered in server view to display were each node is
        //public Vector3 NodeLocation = new Vector3(0, 0, 0);    //The world location of this node

        ////Each Node needs to know who its 8 closest neighbours are, int is neighbour count, then the node itself
        //public Dictionary<int, NavMeshNode> NavMeshNeighbours = new Dictionary<int, NavMeshNode>();

        ////Each node in the nav mesh, has mapped in it, the distance from it to each other node in the entire nav mesh
        ////The dictionary is given two NavmeshNodeID numbers, and returns the distance between the two of them
        //public Dictionary<IDPair, float> NavMeshNodeDistances = new Dictionary<IDPair, float>();   //pass in a vector2, X and Y value being the node ID numbers that you want to find the distance between, the float value being that actual distance value between them

        ////Creates the new nav mesh node at the given location
        //public NavMeshNode(Vector3 StartLocation)
        //{
        //    //Generate a new ID for the new node
        //    NodeID = NavMeshNodeIDGenerator.GetNextID();
        //    //set its location
        //    NodeLocation = StartLocation;
        //    //create a sphere to display where it is
        //    NodeDisplay = new Sphere(NodeLocation, 0.25f);
        //    //add the sphere to the scene to be rendered automatically
        //    Globals.space.Add(NodeDisplay);
        //    Globals.game.ModelDrawer.Add(NodeDisplay);
        //}

        ////Calculate and store the distance between this node and each other node in the given list
        //public void CalculateNodeDistances(NavMeshNode[] NavMeshNodes)
        //{
        //    //Loop through the entire list of nav mesh nodes
        //    for(int i = 0; i < NavMeshNodes.Length; i++)
        //    {
        //        //Check each of the nodes that we need to compare distances with
        //        NavMeshNode CurrentNode = NavMeshNodes[i];
        //        //Find its location, then calculate how far away that is
        //        float NodeDistance = Vector3.Distance(NodeLocation, CurrentNode.NodeLocation);
        //        //Save into the node distance dictionary how far away this node is from its parent
        //        NavMeshNodeDistances.Add(new IDPair(this.NodeID, CurrentNode.NodeID), NodeDistance);
        //    }
        //}

        //    //    //Add each list object to the ordered list as they are created, the list will stay ordered

        //    //}

        //    ////set up lists to store all the neighbour nodes and the info about them
        //    //NavMeshNode[] Neighbours = new NavMeshNode[8];  //the 8 neighbours
        //    //float[] NeighbourDistances = new float[8];
        //    //OrderedList OrderedNeighbours = new OrderedList();
        //    ////Assign the 8 neighbours to first start off at the first 8 nodes found in the nav mesh
        //    //for (int i = 0; i < 8; i++)
        //    //{
        //    //    Neighbours[i] = NavMeshNodes[i];
        //    //    NeighbourDistances[i] = Vector3.Distance(NodeLocation, Neighbours[i].NodeLocation);
        //    //}

        //    ////Now the first 8 nodes in the nav mesh have been set as the default neighbour list
        //    ////Sort this list of 8 nodes in order with the closest nodes at the front

        //    //ListObject NewNeighbour = new ListObject(Neighbours[])
        //}

        ////Takes in a list of nodes and returns which one is closest to this node
        //public NavMeshNode FindClosestNeighbour(NavMeshNode[] NavMeshNodes)
        //{
        //    //We will start off assuming the very first node in the list is the closest neighbour
        //    NavMeshNode ClosestNeighbour = NavMeshNodes[0];
        //    //We need to know its distance from the current node
        //    float NeighbourDistance = Vector3.Distance(NodeLocation, ClosestNeighbour.NodeLocation);

        //    //Now go through the list of nodes
        //    for (int i = 1; i < NavMeshNodes.Length; i++)
        //    {
        //        //Check each node as we go through the list
        //        NavMeshNode OtherNode = NavMeshNodes[i];
        //        //Calculate how far away each of the other nodes is from this one
        //        float OtherNodeDistance = Vector3.Distance(NodeLocation, OtherNode.NodeLocation);
        //        //search until we find a node which is closer than any we have found so far
        //        if(OtherNodeDistance < NeighbourDistance)
        //        {
        //            //when we find a node thats closer than we have found yet, update variables to remember that
        //            ClosestNeighbour = OtherNode;
        //            NeighbourDistance = OtherNodeDistance;
        //        }
        //    }

        //    //After we have gone through the entire list, then we will now know which neighbour is the closest
        //    return ClosestNeighbour;
        //}
    }
}
