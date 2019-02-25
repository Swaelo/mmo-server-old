// ================================================================================================================================
// File:        NavMesh.cs
// Description: Used to calculate the pathfinding values for the entire navigation mesh, aswell as saving and loading them from file
// Author:      Harley Laurie          
// Notes:       Try to store as much information as possible that may be useful to have ready immediately, so the least amount
//              possible needs to be calculated to find pathways during runtime
// ================================================================================================================================

using System;
using System.IO;    //nav mesh pathfinding values may have already been pre-calculated and saved to a data file for quicker server startup times
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Swaelo_Server
{
    public class NavMesh
    {
        public Vector3[] Vertices;
        public int[] Indices;
        public Model ModelData;
        public StaticMesh MeshData;

        public NavMesh(string MeshFile)
        {
            //Load the nav mesh from file
            ModelData = Globals.game.Content.Load<Model>(MeshFile);
            //Extract the meshes vertices and indices
            ModelDataExtractor.GetVerticesAndIndicesFromModel(ModelData, out Vertices, out Indices);
            //Use these to create a new static mesh object
            MeshData = new StaticMesh(Vertices, Indices, new AffineTransform(Vector3.Zero));

            for (int IndexIter = 0; IndexIter < Indices.Length - 2; IndexIter += 2)
            {
                //Get the locations for the next 3 vertices
                Vector3 Location1 = Vertices[Indices[IndexIter]];
                Vector3 Location2 = Vertices[Indices[IndexIter + 1]];
                Vector3 Location3 = Vertices[Indices[IndexIter + 2]];
                //Get the mesh node objects for these 3 locations
                NavMeshNode Node1 = NavMeshNodes.GetNode(Location1);
                NavMeshNode Node2 = NavMeshNodes.GetNode(Location2);
                NavMeshNode Node3 = NavMeshNodes.GetNode(Location3);
                //Let each node know it is a neighbour to the other two nodes
                NavMeshNodes.AssignNeighbours(Node1, Node2, Node3);
            }



            ////Check if there is file containing all the nav mesh nodes neighbours and pathfinding values
            //string FileName = Directory.GetCurrentDirectory() + "\\NavMeshValues.txt";
            //bool FileExists = File.Exists(FileName);
            ////If no save file exists, we need to create all mesh nodes and assign their values manually, then output it to file to be used next time as this takes a long time to do
            //if(!FileExists)
            //{
            //    //Each of the Indices defines the location of a vertex in the navigation mesh, loop through all of them in groups of 3 for each tri in the mesh, assign all of their values
            //    string StartTime = DateTime.Now.ToString("h:mm:ss tt");
            //    l.o(StartTime + ": calculating new nav mesh values");

            //    for (int IndexIter = 0; IndexIter < Indices.Length - 2; IndexIter += 2)
            //    {
            //        //Get the locations for the next 3 vertices
            //        Vector3 Location1 = Vertices[Indices[IndexIter]];
            //        Vector3 Location2 = Vertices[Indices[IndexIter + 1]];
            //        Vector3 Location3 = Vertices[Indices[IndexIter + 2]];
            //        //Get the mesh node objects for these 3 locations
            //        NavMeshNode Node1 = NavMeshNodes.GetNode(Location1);
            //        NavMeshNode Node2 = NavMeshNodes.GetNode(Location2);
            //        NavMeshNode Node3 = NavMeshNodes.GetNode(Location3);
            //        //Let each node know it is a neighbour to the other two nodes
            //        NavMeshNodes.AssignNeighbours(Node1, Node2, Node3);
            //    }

            //    //Now all nodes have been created and their values have all been assigned, we need to save all of this information into a text document for later use
            //    //We start off by defining an array of strings, each string of the array will represent a single line in the file that is going to be saved for later
            //    int NodeCount = NavMeshNodes.MeshNodes.Count;
            //    string[] FileLines = new string[NodeCount + 1];
            //    FileLines[0] = "NodeCount:" + NodeCount;
            //    for (int NodeIter = 0; NodeIter < NodeCount; NodeIter++)
            //    {
            //        //First add to the string all of this nodes information
            //        NavMeshNode Node = NavMeshNodes.MeshNodes[NodeIter];
            //        FileLines[NodeIter + 1] = "Node#" + (NodeIter + 1) + " " +
            //            Node.NodeLocation.X + "," + Node.NodeLocation.Y + "," + Node.NodeLocation.Z + " " +
            //            "FScore:" + Node.FScore + " " +
            //            "GScore:" + Node.GScore + " " +
            //            "NeighbourCount:" + Node.Neighbours.Count + " ";
            //        //Now loop through and add what each of this nodes neighbours are
            //        for(int NeighbourIter = 0; NeighbourIter < Node.Neighbours.Count; NeighbourIter++)
            //        {
            //            NavMeshNode Neighbour = Node.Neighbours[NeighbourIter];
            //            FileLines[NodeIter + 1] +=
            //                "Neighbour#" + (NeighbourIter + 1) + ":" +
            //                Neighbour.NodeLocation.X + "," + Neighbour.NodeLocation.Y + "," + Neighbour.NodeLocation.Z + " ";
            //        }
            //    }

            //    //Now we are ready to save all of this information to the file
            //    System.IO.File.WriteAllLines(FileName, FileLines);
            //    string EndTime = DateTime.Now.ToString("h:mm:ss tt");
            //    l.o(EndTime + ": finished");
            //}
            //else
            //{
            //    string StartTime = DateTime.Now.ToString("h:mm:ss tt");
            //    l.o(StartTime + ": loading nav mesh values from file");
            //    //Open the nav mesh save file into an array with each index being a single line of the file
            //    string[] FileLines = System.IO.File.ReadAllLines(FileName);
            //    for(int LineIter = 1; LineIter < FileLines.Length; LineIter++)
            //    {
            //        //Get each line of the file as we load through the whole thing
            //        string Line = FileLines[LineIter];
            //        //l.o(Line);
            //        int LineLength = Line.Length;
            //        //Split up the string to get the node number and and locations
            //        string NodeNumber = Line.Substring(0, Line.IndexOf(" "));
            //        int NodeNumberLength = NodeNumber.Length;
            //        //Get the rest of the remaining string, then split up all the remaining values by spaces
            //        string LineRemainder = Line.Substring(Line.IndexOf(" "), (LineLength - NodeNumberLength));
            //        string[] LineValues = LineRemainder.Split(' ');

            //        //Get the location of this navmesh node
            //        string LocationValues = LineValues[1];
            //        string[] LocationSplit = LocationValues.Split(',');
            //        string LocationX = LocationSplit[0];
            //        string LocationY = LocationSplit[1];
            //        string LocationZ = LocationSplit[2];
            //        Vector3 Location = new Vector3(float.Parse(LocationX), float.Parse(LocationY), float.Parse(LocationZ));

            //        //With its location, we can grab this node from the list
            //        NavMeshNode Node = NavMeshNodes.GetNode(Location);

            //        //Get the FScore and GScore of this node
            //        string FScoreString = LineValues[2];
            //        string[] FSplit = FScoreString.Split(':');
            //        float FScore = float.Parse(FSplit[1]);
            //        string GScoreString = LineValues[3];
            //        string[] GSplit = GScoreString.Split(':');
            //        float GScore = float.Parse(GSplit[1]);
            //        //Assign these pathfinding values to the new node
            //        Node.FScore = FScore;
            //        Node.GScore = GScore;

            //        //Find out how many neighbours this node has
            //        string NeighbourCountString = LineValues[4];
            //        string[] NeighbourSplit = NeighbourCountString.Split(':');
            //        int NeighbourCount = Int32.Parse(NeighbourSplit[1]);
            //        //Loop through an extract all of the information for each of our neighbouring nodes
            //        for(int NeighbourIter = 0; NeighbourIter < NeighbourCount; NeighbourIter++)
            //        {
            //            //first extract the location values away from the neighbour counter indicator
            //            string NeighbourString = LineValues[5 + NeighbourIter];
            //            string[] NeighbourStringSplit = NeighbourString.Split(':');
            //            //now split the location values and parse them into a new vector3
            //            string[] NeighbourValues = NeighbourStringSplit[1].Split(',');
            //            Vector3 NeighbourLocation = new Vector3(float.Parse(NeighbourValues[0]), float.Parse(NeighbourValues[1]), float.Parse(NeighbourValues[2]));
            //            //with its location, grab this node from the list and assign it as one of our neighbours
            //            NavMeshNode NeighbourNode = NavMeshNodes.GetNode(NeighbourLocation);
            //            NavMeshNodes.AssignNeighbours(Node, NeighbourNode);
            //        }
            //    }
            //    string EndTime = DateTime.Now.ToString("h:mm:ss tt");
            //    l.o(EndTime + ": finished");
        }
    }
}
