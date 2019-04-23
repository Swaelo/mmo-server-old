// ================================================================================================================================
// File:        NetworkManager.cs
// Description: Handles sending network packets to their assigned packet handlers
// ================================================================================================================================

using System.Collections.Generic;

namespace Server.Networking
{
    public static class NetworkManager
    {
        public static Dictionary<int, NetworkPacket> NetworkPackets = new Dictionary<int, NetworkPacket>();

        public static void RegisterPacketHandler(PacketHandler PacketHandler)
        {
            
        }
    }
}
