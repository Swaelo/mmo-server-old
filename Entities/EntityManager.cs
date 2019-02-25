// ================================================================================================================================
// File:        EntityManager.cs
// Description: Handles and keeps track of all entities which are currently active in the game world, calling Update on the manager
//              will have Update called on every single entity that is currently being tracked by the manager
// Author:      Harley Laurie          
// Notes:       
// ================================================================================================================================

using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public static class EntityManager
    {
        //Store a list of all entities currently active in the game world
        public static List<BaseEntity> ActiveEntities = new List<BaseEntity>();

        //Every .25 seconds the server will update all clients on the state of all entities
        private static float ClientUpdateInterval = 0.25f;
        private static float NextClientUpdate = 0.25f;

        //Simply add an entity into the list with the rest of them
        public static void AddEntity(BaseEntity NewEntity)
        {
            ActiveEntities.Add(NewEntity);
            NewEntity.ID = EntityIDGenerator.GetNextID();
        }

        //Creates a new static entity with the given details, adds it to the scene and stores it with the rest
        public static void AddStaticEntity(Vector3 Position, Vector3 Scale)
        {
            StaticEntity NewEntity = new StaticEntity(Position, Scale);
        }

        //Returns from the list of active entities, the entities which clients should be told about
        public static List<BaseEntity> GetInteractiveEntities()
        {
            List<BaseEntity> InteractiveEntities = new List<BaseEntity>();

            foreach(BaseEntity Entity in ActiveEntities)
            {
                if (Entity.Type != "Static")
                    InteractiveEntities.Add(Entity);
            }

            return InteractiveEntities;
        }

        //Updates all the entities being managed right now
        public static void UpdateEntities(float dt)
        {
            //Update all the entities
            foreach(BaseEntity Entity in ActiveEntities)
            {
                Entity.Update(dt);
            }
            //Count down the client update timer
            NextClientUpdate -= dt;
            if(NextClientUpdate <= 0.0f)
            {
                NextClientUpdate = ClientUpdateInterval;
                PacketSenderLogic.SendListEntityUpdates(ClientManager.GetAllActiveClients(), ActiveEntities);
            }
        }
    }

    //Generates unique identifiers for every entity
    internal static class EntityIDGenerator
    {
        private static readonly string Encode = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
        private static long PreviousID = DateTime.UtcNow.Ticks;
        public static string GetNextID() => GenerateEntityID(Interlocked.Increment(ref PreviousID));
        private static unsafe string GenerateEntityID(long ID)
        {
            char* CharBuffer = stackalloc char[13];
            CharBuffer[0] = Encode[(int)(ID >> 60) & 31];
            CharBuffer[1] = Encode[(int)(ID >> 55) & 31];
            CharBuffer[2] = Encode[(int)(ID >> 50) & 31];
            CharBuffer[3] = Encode[(int)(ID >> 45) & 31];
            CharBuffer[4] = Encode[(int)(ID >> 40) & 31];
            CharBuffer[5] = Encode[(int)(ID >> 35) & 31];
            CharBuffer[6] = Encode[(int)(ID >> 30) & 31];
            CharBuffer[7] = Encode[(int)(ID >> 25) & 31];
            CharBuffer[8] = Encode[(int)(ID >> 20) & 31];
            CharBuffer[9] = Encode[(int)(ID >> 15) & 31];
            CharBuffer[10] = Encode[(int)(ID >> 10) & 31];
            CharBuffer[11] = Encode[(int)(ID >> 5) & 31];
            CharBuffer[12] = Encode[(int)ID & 31];

            return new string(CharBuffer, 0, 13);
        }
    }
}