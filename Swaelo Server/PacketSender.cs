using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ServerPacketType
{
    Message = 1,    //server sends a message to the client <int:PacketType, string:Message>
    EnterGame = 2,   //server sends character info data to the client and tells them to spawn into the game <int:PacketType, float:XPos, float:YPos, float:ZPos, float:XRot, float:YRot, float:ZRot, float:WRot>
    SpawnExternalClient = 3, //Sends a message to a client to spawn someone elses character into the game world <int:PacketType, string:AccountName, vec3:Position, vec4:rotation>
    UpdatePlayer = 4,    //Sends a message to a client with one of the other clients updated position and rotation data <int:PacketType, Vec3:Position, Vec4:Rotation>
    RemovePlayer = 5,    //Tells clients to remove someone elses character who has disconnected from the game
    PlayerMessage = 6 
}

namespace Swaelo_Server
{
    static class PacketSender
    {
        //Sends a message to a client
        public static void SendMessage(int ClientID, string Message)
        {
            //Create the packet to send through the network
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.Message);
            PacketWriter.WriteString(Message);
            //Send the packet to the target client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //Sends a message to a client to enter into the game world <int:PacketType, float:XPos, float:YPos, float:ZPos, float:XRot, float:YRot, float:ZRot, float:WRot>
        public static void SendEnterGame(int ClientID, string AccountName, Vector3 CharacterPosition, Vector4 CharacterRotation)
        {
            //Create the packet to send through the network
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.EnterGame); //write the packet type
            PacketWriter.WriteString(AccountName);
            //write character position data
            PacketWriter.WriteFloat(CharacterPosition.x);
            PacketWriter.WriteFloat(CharacterPosition.y);
            PacketWriter.WriteFloat(CharacterPosition.z);
            //write character rotation data
            PacketWriter.WriteFloat(CharacterRotation.x);
            PacketWriter.WriteFloat(CharacterRotation.y);
            PacketWriter.WriteFloat(CharacterRotation.z);
            PacketWriter.WriteFloat(CharacterRotation.w);
            //Send the packet to the client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            //close the packet writer
            PacketWriter.Dispose();
        }

        //Sends a message to a client to spawn someone elses character into the game world <int:PacketType, string:AccountName, vec3:Position, vec4:rotation>
        public static void SendSpawnOther(int ClientID, string AccountName, Vector3 CharacterPosition, Vector4 CharacterRotation)
        {
            //Create the packet to send through the network
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SpawnExternalClient); //write the packet type
            PacketWriter.WriteString(AccountName);  //write the name of the client being spawned in
            //write position data
            PacketWriter.WriteFloat(CharacterPosition.x);
            PacketWriter.WriteFloat(CharacterPosition.y);
            PacketWriter.WriteFloat(CharacterPosition.z);
            //write rotation data
            PacketWriter.WriteFloat(CharacterRotation.x);
            PacketWriter.WriteFloat(CharacterRotation.y);
            PacketWriter.WriteFloat(CharacterRotation.z);
            PacketWriter.WriteFloat(CharacterRotation.w);
            //send the packet to the client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            //close the writer
            PacketWriter.Dispose();
        }

        //Sends a message to a client with one of the other clients updated position and rotation data <int:PacketType, Vec3:Position, Vec4:Rotation>
        public static void SendPlayerUpdate(int ClientID, string AccountName, Vector3 NewPosition)
        {
            //Create the packet to send through the network
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.UpdatePlayer); //write the packet type
            PacketWriter.WriteString(AccountName);//write the account name
            //write the position data
            PacketWriter.WriteFloat(NewPosition.x);
            PacketWriter.WriteFloat(NewPosition.y);
            PacketWriter.WriteFloat(NewPosition.z);
            //send the packet to the client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            //close the packet writer
            PacketWriter.Dispose();
        }

        public static void SendRemovePlayer(int ClientID, string AccountName)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.RemovePlayer);
            PacketWriter.WriteString(AccountName);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        public static void SendPlayerMessage(int ClientID, string Sender, string Message)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.PlayerMessage);
            PacketWriter.WriteString(Sender);
            PacketWriter.WriteString(Message);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }
    }
}
