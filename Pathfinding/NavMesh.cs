using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using BEPUphysics.BroadPhaseEntries;
using BEPUutilities;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Server.Pathfinding
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
            ModelData = Rendering.GameWindow.CurrentWindow.Content.Load<Model>(MeshFile);
            //Extract the meshes vertices and indices
            Data.ModelDataExtractor.GetVerticesAndIndicesFromModel(ModelData, out Vertices, out Indices);
            //Use these to create a new static mesh object
            MeshData = new StaticMesh(Vertices, Indices, new AffineTransform(Vector3.Zero));

            //Display how much work needs to be done and track how much of it has been completed so far
            float VertexCount = Indices.Length;
            int SetCount = (int)VertexCount / 3;
            int CurrentPercentage = 0;
            l.og("Generating AI Navigation Mesh Links... 0%");

            //Loop through the entire set of faces making up this entire navigation mesh 
            for (int i = 0; i < SetCount; i++)
            {
                //Calculate how much of the set we have worked through so far
                int CurrentSet = i + 1;
                float ProgressFraction = (float)CurrentSet / (float)SetCount;
                int ProgressPercentage = (int)(100f * ProgressFraction);
                //If we have reached the next percentile, update the progress message
                if (ProgressPercentage > CurrentPercentage)
                {
                    CurrentPercentage = ProgressPercentage;
                    l.ogEdit("Generating AI Navigation Mesh Links... " + ProgressPercentage + "%");
                }

                //Grab the locations of the 3 vertices that make up this face in the mesh
                Vector3[] VertexLocations = GetNextLocations(i * 3);
                //Grab the mesh node objects assigned to these 3 locations
                NavMeshNode[] VertexNodes = NavMeshNodes.GetNodes(VertexLocations);
                //Create neighbour links between all of these 3 mesh nodes
                NavMeshNodes.AssignNeighbours(VertexNodes);
            }

            l.ogEdit("Generating AI Navigation Mesh Links... Complete!");
        }

        private Vector3[] GetNextLocations(int CurrentIndex)
        {
            Vector3[] NextLocations = new Vector3[3];
            NextLocations[0] = Vertices[Indices[CurrentIndex]];
            NextLocations[1] = Vertices[Indices[CurrentIndex + 1]];
            NextLocations[2] = Vertices[Indices[CurrentIndex + 2]];
            return NextLocations;
        }
    }
}
