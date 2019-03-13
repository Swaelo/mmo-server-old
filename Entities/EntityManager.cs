// ================================================================================================================================
// File:        EntityManager.cs
// Description: Keeps track of all entities currently active in the servers world simulation, used to keep them all up to date and
//              for sending all their updated information to any connected game clients to keep them updated on the entities states
// ================================================================================================================================

using System;
using System.Threading;
using System.Collections.Generic;

namespace Server.Entities
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

        //Returns from the list of active entities, the entities which clients should be told about
        public static List<BaseEntity> GetInteractiveEntities()
        {
            List<BaseEntity> InteractiveEntities = new List<BaseEntity>();

            foreach (BaseEntity Entity in ActiveEntities)
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
            foreach (BaseEntity Entity in ActiveEntities)
            {
                Entity.Update(dt);
            }

            //Count down the client update timer
            NextClientUpdate -= dt;
            if (NextClientUpdate <= 0.0f)
            {
                NextClientUpdate = ClientUpdateInterval;
                Networking.PacketManager.SendListEntityUpdates(Networking.ConnectionManager.GetActiveClients(), ActiveEntities);
            }
        }

        //Tells any enemies targetting this client to drop their target
        public static void DropTarget(Networking.ClientConnection TargetEntity)
        {
            foreach (var Entity in ActiveEntities)
            {
                //Check all of the active entities in the game
                EnemyEntity Enemy = (EnemyEntity)Entity;
                //Find any targetting them
                if (Enemy.PlayerTarget == TargetEntity)
                    Enemy.DropTarget();
            }
        }

        //Handles disconnection of a player from the game
        public static void HandleClientDisconnect(Networking.ClientConnection Client)
        {
            //Remove them from the scene
            Physics.WorldSimulator.Space.Remove(Client.ServerCollider);
            Rendering.Window.Instance.ModelDrawer.Remove(Client.ServerCollider);

            //Tell any enemies attacking them to drop their target
            Entities.EntityManager.DropTarget(Client);

            //Backup their character data in the database
            Data.Database.SaveCharacterLocation(Client.CharacterName, Maths.VectorTranslate.ConvertVector(Client.CharacterPosition));
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
