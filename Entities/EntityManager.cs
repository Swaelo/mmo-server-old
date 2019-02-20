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

        //Simply add an entity into the list with the rest of them
        public static void AddEntity(BaseEntity NewEntity)
        {
            ActiveEntities.Add(NewEntity);
            NewEntity.ID = EntityIDGenerator.GetNextID();
        }
    }

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