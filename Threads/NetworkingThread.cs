// ================================================================================================================================
// File:        NetworkingThread.cs
// Description: This thread handles the TCP connections for every client who is currently playing the game, aswell as handling all
//              of the packet sending and recieving between the server and each of these clients
// Author:      Harley Laurie          
// Notes:       I feel its perfectely fine to have the networking thread run on the CPU, still very curious to find out how many
//              active clients it will take until the server starts lagging or even crashes
// ================================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class NetworkingThread
    {
        public static Thread Thread;
        public static NetworkThreadEndDelegate EndDelegate;
        public delegate void NetworkThreadEndDelegate();

        public void InitializeThread()
        {
            Thread = new Thread(new ParameterizedThreadStart(NetworkingThreadProc));
            EndDelegate = NetworkingThreadEnd;
            ((Main)Globals.MainWindowForm).SetNetworkingStatus(true, "Networking Thread: ");
        }

        public void StartThread()
        {
            Thread.Start(EndDelegate);
        }

        public static void NetworkingThreadProc(Object Data)
        {
            NetworkThreadEndDelegate NetworkEndDelegate = Data as NetworkThreadEndDelegate;
            if (NetworkEndDelegate != null)
                NetworkEndDelegate();
        }

        public static void NetworkingThreadEnd()
        {
            ((Main)Globals.MainWindowForm).SetNetworkingStatus(false, "Networking Thread: ");
        }
    }
}
