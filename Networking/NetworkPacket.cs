using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Networking
{
    public class NetworkPacket
    {
        public delegate void NetworkPacketDelegate(int TargetNetworkID, byte[] PacketData);
    }
}
