// ================================================================================================================================
// File:        EnemyEntity.cs
// Description: Defines a single enemy currently active in the servers world simulation, controls all of their AI/Behaviour during play
// ================================================================================================================================

using System.Collections.Generic;
using BEPUutilities;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysicsDrawer.Models;
using Server.Physics;
using Server.Networking;
using Server.Pathfinding;

namespace Server.Entities
{
    public enum EnemyState
    {
        Idle,
        Seek,
        Attack,
        Flee
    }

    public class EnemyEntity : BaseEntity
    {
        public EnemyState EntityState = EnemyState.Idle;    //Current AI state of the entity
        private float AgroRange = 5f;   //How close players must be for the enemy to start targetting them
        private float AgroMaxDistance = 10f;    //How far players can run away before they will stop being targetted
        public ClientConnection PlayerTarget = null; //The current player the entity is targetting in combat
        private List<Vector3> NavigationPathway;    //Pathway of navmeshnodes for the entity to follow to reach its target location
        private Vector3 SpawnLocation;  //The enemy will return to this location after dropping its target
        private float SeekSpeed = 3;    //How fast the enemy moves while moving towards its current target
        private float FleeSpeed = 5;    //How fast the enemy moves while returning to its spawn location

        public EnemyEntity(Vector3 SpawnPosition)
        {

            //Store the entities spawn location so they know where to return to once combat has finished
            SpawnLocation = SpawnPosition;
            
            //Add the box object to the physics scene and start rendering it
            Entity = new Cylinder(SpawnPosition, 1.7f, 0.6f, 10f);

            Physics.WorldSimulator.Space.Add(Entity);
            Rendering.Window.Instance.ModelDrawer.Add(Entity);
            
            //Tell the entity manager this new entity has been created so it can handle it for us
            EntityManager.AddEntity(this);

            //Set the type of ingame enemy that this entity represents
            this.Type = "Skeleton Warrior";
        }

        void RemoveFriction(EntityCollidable sender, BroadPhaseEntry other, NarrowPhasePair pair)
        {
            var collidablePair = pair as CollidablePairHandler;
            if (collidablePair != null)
                collidablePair.UpdateMaterialProperties(new BEPUphysics.Materials.InteractionProperties());
        }

        public override void Update(float DeltaTime)
        {
            switch(EntityState)
            {
                case (EnemyState.Idle):
                    IdleState();
                    break;

                case (EnemyState.Seek):
                    SeekState(DeltaTime);
                    break;

                case (EnemyState.Attack):
                    AttackState(DeltaTime);
                    break;

                case (EnemyState.Flee):
                    FleeState(DeltaTime);
                    break;

                default:
                    break;
            }
        }

        //If the enemy is in the Seek or Attack states, it is forced to drop its current target, flee back to its spawn position then idle again
        public void DropTarget()
        {
            l.og("drop target");
            EntityState = EnemyState.Flee;
            PlayerTarget = null;
        }

        private void IdleState()
        {
            //Look at all the active players in the game world, find which one is the closest to this entity
            List<ClientConnection> ActivePlayers = ConnectionManager.GetActiveClients();
            if(ActivePlayers.Count > 0)
            {
                //Check how far away the closest player is
                ClientConnection ClosestPlayer = FindClosestPlayer(ActivePlayers);
                float PlayerDistance = Vector3.Distance(Entity.Position, ClosestPlayer.CharacterPosition);
                //If they are close enough they become our new target and we enter the combat state
                if(PlayerDistance <= AgroRange)
                {
                    l.og("new target!");
                    PlayerTarget = ClosestPlayer;
                    EntityState = EnemyState.Attack;
                    return;
                }
            }
        }

        //Returns whatever player character in the list is closest to this entity
        private ClientConnection FindClosestPlayer(List<ClientConnection> ActivePlayers)
        {
            //Assign the very first player in the list as the initial closest player
            ClientConnection ClosestPlayer = ActivePlayers[0];
            float ClosestPlayerDistance = Vector3.Distance(Entity.Position, ClosestPlayer.CharacterPosition);
            //Now loop through all of the remaining players and compare each of them to see if any of them are closer
            for(int i = 1; i < ActivePlayers.Count; i++)
            {
                //Compute the distance between this next player and the target location
                ClientConnection NextPlayer = ActivePlayers[i];
                float NextPlayerDistance = Vector3.Distance(Entity.Position, NextPlayer.CharacterPosition);
                //Compare the next players distance to the distance of the current closest player
                if(NextPlayerDistance < ClosestPlayerDistance)
                {
                    //Update the closest player if the next player is found to be closer
                    ClosestPlayer = NextPlayer;
                    ClosestPlayerDistance = NextPlayerDistance;
                }
            }
            //After each active player has been checked, return which one was found to be the closest to the target location
            return ClosestPlayer;
        }

        //Stops everything, finds a path to the target location, then starts navigation to it
        public void SeekLocation(Vector3 TargetLocation)
        {
            //Change to the seek state so once the pathway has been constructed the enemy will start travelling along it
            EntityState = EnemyState.Seek;
            //Grab the levels current nav mesh
            NavMesh NavMesh = WorldSimulator.TestLevelNavMesh;
            //Pass the entity and fps controllers positions into the navmesh to find a pathway between these two locations
            NavigationPathway = AStarSearch.ConstructNodePathway(NavMesh, Entity.Position, WorldSimulator.FPSController.Position);

            //Render a sphere at each step of the pathway so we can see what it looks like
            ModelDrawer ModelDrawer = Rendering.Window.Instance.ModelDrawer;
            foreach (Vector3 PathStep in NavigationPathway)
                ModelDrawer.Add(new Sphere(PathStep, 0.1f, 0));
        }

        private void SeekState(float DeltaTime)
        {
            //Travel along the navigation pathway until all steps have been taken
            float NextStepDistance = Vector3.Distance(Position, NavigationPathway[0]);
            if(NextStepDistance > 1f)
            {//Move towards the next step location
                Vector3 NextStepDirection = Position - NavigationPathway[0];
                NextStepDirection.Normalize();
                Entity.Position -= NextStepDirection * SeekSpeed * DeltaTime;
            }
            else
            {//Remove this step from the pathway
                NavigationPathway.Remove(NavigationPathway[0]);
                if(NavigationPathway.Count == 0)
                    EntityState = EnemyState.Idle;
            }
        }

        private void AttackState(float DeltaTime)
        {
            //Check how far away our target is, the current distance will determine what action we take during combat
            float TargetDistance = Vector3.Distance(Entity.Position, PlayerTarget.CharacterPosition);

            //Attack them if they are within range
            //if (TargetDistance <= 1)
            //    l.og("Attack!");
            //Move closer if they arent close enough to attack
            if (TargetDistance > 3 && TargetDistance < AgroMaxDistance)
                Entity.Position = Vector3.Lerp(Entity.Position, PlayerTarget.CharacterPosition, SeekSpeed * DeltaTime);
            //If they are out of range then drop them as our target
            else if(TargetDistance > AgroMaxDistance)
                DropTarget();
        }

        private void FleeState(float DeltaTime)
        {
            //Run back to our spawn position then idle once we get there
            Entity.Position = Vector3.Lerp(Entity.Position, SpawnLocation, FleeSpeed * DeltaTime);
            float SpawnDistance = Vector3.Distance(Entity.Position, SpawnLocation);
            if (SpawnDistance <= 1f)
                EntityState = EnemyState.Idle;
        }
    }
}
