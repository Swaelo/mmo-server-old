using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class NetworkSendData
    {
        //formats the players information ready for sending packets
        public byte[] FormatPlayerData(int PlayerIndex)
        {
            //Use our buffer dll to format packet data correctly
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();

            //Prevent out of bounds
            if (PlayerIndex <= 0 || PlayerIndex > Constants.MAX_PLAYERS)
                return null;

            //Write in packet type and client index
            buffer.WriteInteger((int)PacketTypes.ServerPackets.SPlayerInfo);
            buffer.WriteInteger(PlayerIndex);

            //Write in position info
            Vector3 PlayerPosition = Globals.general.GetPlayerPosition(PlayerIndex);
            buffer.WriteFloat(PlayerPosition.x);
            buffer.WriteFloat(PlayerPosition.y);
            buffer.WriteFloat(PlayerPosition.z);

            //write in rotation info
            Vector4 PlayerRotation = Globals.general.GetPlayerRotation(PlayerIndex);
            buffer.WriteFloat(PlayerRotation.x);
            buffer.WriteFloat(PlayerRotation.y);
            buffer.WriteFloat(PlayerRotation.z);
            buffer.WriteFloat(PlayerRotation.w);

            //write in account name
            buffer.WriteString(Globals.LivePlayers[PlayerIndex].AccountName);

            //return the formatted data
            return buffer.ToArray();
        }

        //sends a packet from the server to a single client
        public void SendDataTo(int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            Globals.Clients[index].myStream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
            buffer = null;
        }

        //sends a packet from the server to all the connected clients
        public void SendDataToAll(byte[] data)
        {
            for (int i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                if (Globals.LivePlayers[i].IsPlaying)
                {
                    SendDataTo(i, data);
                }
            }
        }

        //sends a packet to all but one of the connected clients
        public void SendDataToAllBut(int index, byte[] data)
        {
            for (int i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                if (Globals.Clients[i].Socket != null)
                {
                    if (i != index)
                    {
                        Console.WriteLine("sending data to: " + i);
                        SendDataTo(i, data);
                    }
                }
            }
        }

        //sends a custom alert message to a single client
        public void SendAlertMessage(int index, string alertMessage)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteInteger((int)PacketTypes.ServerPackets.SAlertMessage);
            buffer.WriteString(alertMessage);
            SendDataTo(index, buffer.ToArray());
        }

        //sends a message to a client accepting their login request and telling them they may enter the game now
        public async void SendIngame(int index)
        {
            Console.WriteLine(Globals.LivePlayers[index].AccountName + " has started playing.");
            //send the players data to everyone including the player himself
            SendDataToAll(FormatPlayerData(index));
            //send all other players data to this player
            for(int PlayerIterator = 1; PlayerIterator < Constants.MAX_PLAYERS; PlayerIterator++)
            {
                //only send data to the connected players
                if(Globals.general.IsClientConnected(PlayerIterator))
                {
                    if(PlayerIterator != index)
                    {
                        await Task.Delay(75);
                        SendDataTo(index, FormatPlayerData(PlayerIterator));
                    }
                }
            }
        }

        //sends a message to a client giving them a players information
        public void SendPlayerMovement(int PlayerIndex, Vector3 PlayerPosition, Vector4 PlayerRotation, string PlayerName)
        {
            //Create a buffer to format our network packet correctly
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteInteger((int)PacketTypes.ServerPackets.SPlayerMovement);

            //set the players index number
            buffer.WriteInteger(PlayerIndex);

            //set the players current position
            buffer.WriteFloat(PlayerPosition.x);
            buffer.WriteFloat(PlayerPosition.y);
            buffer.WriteFloat(PlayerPosition.z);

            //set the players current rotation
            buffer.WriteFloat(PlayerRotation.x);
            buffer.WriteFloat(PlayerRotation.y);
            buffer.WriteFloat(PlayerRotation.z);
            buffer.WriteFloat(PlayerRotation.w);

            //set the players username
            buffer.WriteString(PlayerName);

            //set this data to everyone else on the server so they know where this player is now
            SendDataToAllBut(PlayerIndex, buffer.ToArray());
        }
    }
}
