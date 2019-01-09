using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class PacketTypes
    {
        public enum ServerPackets
        {
            //packet type that the server can send to its clients
            SAlertMessage = 1,
            SPlayerInfo = 2,
            SPlayerMovement = 3
        }

        public enum ClientPackets
        {
            //packet type that the clients can send to their server
            CNewAccount = 1,
            CLoginAccount = 2,
            CMovePlayer = 3
        }
    }
}
