using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class FSMEntity : BaseEntity
    {
        private NavMeshNode StartNode, EndNode;
        private NavMeshNode PreviousTargetNode;

        public FSMEntity(Vector3 StartPosition, Vector3 EndPosition)
        {
            StartNode = NavMeshNodes.GetNearbyMeshNode(StartPosition);
            EndNode = NavMeshNodes.GetNearbyMeshNode(EndPosition);
            EntityPath = AStarSearch.FindPath(StartNode, EndNode, new Vector2(9, 9));
            PreviousTargetNode = EndNode;

            Position = StartPosition;
            entity = new Entity(new Box(StartPosition, 1, 1, 1, 1).CollisionInformation, 1);
            entity.Position = StartPosition;
            Globals.space.Add(entity);
            Globals.game.ModelDrawer.Add(entity);
            EntityManager.AddEntity(this);
            mover = new EntityMover(entity);
            rotator = new EntityRotator(entity);
            Globals.space.Add(mover);
            Globals.space.Add(rotator);
        }

        private readonly Path<Vector3> positionPath;
        private readonly EntityMover mover;
        private readonly EntityRotator rotator;
        private float pathTime;

        public List<NavMeshNode> EntityPath;
        

        public override void Update(float dt)
        {
            Position = entity.Position;
            Rotation = entity.Orientation;
            //Check out distance from the node at the front of the pathway
            float NodeDistance = Vector3.Distance(entity.Position, EntityPath[0].NodeLocation);
            //Once we are close enough to the next node, then we remove it from the list and proceed to the next one
            if (NodeDistance <= 0.05f)
            {
                //If we have reached the final node in the path, then we want to calculate a new path back were we just came from
                if (EntityPath.Count == 1)
                {
                    if(PreviousTargetNode == EndNode)
                    {
                        EntityPath = AStarSearch.FindPath(EndNode, StartNode, new Vector2(9, 9));
                        PreviousTargetNode = StartNode;
                    }
                    else
                    {
                        EntityPath = AStarSearch.FindPath(StartNode, EndNode, new Vector2(9, 9));
                        PreviousTargetNode = EndNode;
                    }
                }
                //Otherwise we remove this node from the list and move to the next one
                EntityPath.Remove(EntityPath[0]);
            }
            //Continue on towards out current target
            pathTime += Globals.space.TimeStepSettings.TimeStepDuration;
            mover.TargetPosition = EntityPath[0].NodeLocation;
        }
    }
}