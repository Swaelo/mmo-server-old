using System;
using System.Collections.Generic;
using System.Text;

public enum ServerPacketType
{
    ConsoleMessage = 1, //displays a message in the clients console log window
    PlayerMessage = 2,  //displays a message in the clients player chat window

    RegisterReply = 3,  //tells a client if their account was registered
    LoginReply = 4, //tells a client if they logged into the account
    CreateCharacterReply = 5,   //tells a client if their character was created

    SendCharacterData = 6,  //tells a client the info for each character they have created
    PlayerEnterWorld = 7,   //tells a client to enter into the game world
    PlayerUpdatePosition = 8,   //tells a client to update someone elses position info

    SpawnOtherPlayer = 9,   //tells a client to spawn someone elses character into their world
    RemoveOtherPlayer = 10  //tells a client to spawn a list of other players to spawn into their world
}

namespace Swaelo_Server
{
    static class PacketSender
    {
        //displays a message in the clients console log window
        public static void SendConsoleMessage(int ClientID, string Message)
        {
            //Create the packet to send through the network
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.ConsoleMessage);
            PacketWriter.WriteString(Message);
            //Send the packet to the target client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //displays a message in the clients player chat window
        public static void SendPlayerMessage(int ClientID, string Sender, string Message)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.PlayerMessage);
            PacketWriter.WriteString(Sender);
            PacketWriter.WriteString(Message);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //Spreads a players message to everyone in a list of clients
        public static void SendPlayersMessage(List<Client> TargetClients, string Sender, string Message)
        {
            foreach (Client OtherClient in TargetClients)
                SendPlayerMessage(OtherClient.ClientID, Sender, Message);
        }

        //tells a client if their account was registered
        public static void SendRegisterReply(int ClientID, bool Success, string Message)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.RegisterReply);
            PacketWriter.WriteInteger(Success ? 1 : 0);
            PacketWriter.WriteString(Message);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a client if they logged into the account
        public static void SendLoginReply(int ClientID, bool Success, string Message)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.LoginReply);
            PacketWriter.WriteInteger(Success ? 1 : 0);
            PacketWriter.WriteString(Message);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a client if their character was created
        public static void SendCreateCharacterReply(int ClientID, bool CreationSuccess, string ReplyMessage)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.CreateCharacterReply);
            PacketWriter.WriteInteger(CreationSuccess ? 1 : 0);
            PacketWriter.WriteString(ReplyMessage);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a client the info for each character they have created
        public static void SendCharacterData(int ClientID, string AccountName)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SendCharacterData);
            //First we need to look up in the database how many characters this user has created so far
            int CharacterCount = Globals.database.GetCharacterCount(ClientID, AccountName);
            PacketWriter.WriteInteger(CharacterCount);
            //Now loop through and add all the information for each character that has already been created
            for(int i = 0; i < CharacterCount; i++)
            {
                //Get the name of each of the players characters one at a time
                string CharacterName = Globals.database.GetCharacterName(AccountName, i + 1);
                //Get all the data for this character from the database
                CharacterData Data = Globals.database.GetCharacterData(CharacterName);
                //Save all of this information into the packet
                PacketWriter.WriteString(Data.Account);
                PacketWriter.WriteFloat(Data.Position.x);
                PacketWriter.WriteFloat(Data.Position.y);
                PacketWriter.WriteFloat(Data.Position.z);
                PacketWriter.WriteString(Data.Name);
                PacketWriter.WriteInteger(Data.Experience);
                PacketWriter.WriteInteger(Data.ExperienceToLevel);
                PacketWriter.WriteInteger(Data.Level);
                PacketWriter.WriteInteger(Data.IsMale ? 1 : 0);
            }
            //Send the packet to the client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a client to enter into the game world
        public static void SendPlayerEnterWorld(int ClientID)
        {
            Client Client = ClientManager.Clients[ClientID];
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.PlayerEnterWorld); //write the packet type

            //we also want to pass information about all of the other clients already playing the game right now
            List<Client> OtherPlayers = ClientManager.GetActiveClientsExceptFor(ClientID);
            PacketWriter.WriteInteger(OtherPlayers.Count);  //how many other players are in the game
            foreach(Client Other in OtherPlayers)
            {
                //Provide the information for each other player who is in the game
                PacketWriter.WriteString(Other.CurrentCharacterName); //characters name 
                //characters position
                PacketWriter.WriteFloat(Other.CharacterPosition.x);
                PacketWriter.WriteFloat(Other.CharacterPosition.y);
                PacketWriter.WriteFloat(Other.CharacterPosition.z);
            }
            
            //send the packet to the client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        public static void SendSpawnOther(int ClientID, string CharacterName, SwaeloMath.Vector3 Position)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SpawnOtherPlayer);  //packet type
            PacketWriter.WriteString(CharacterName);
            PacketWriter.WriteFloat(Position.x);
            PacketWriter.WriteFloat(Position.y);
            PacketWriter.WriteFloat(Position.z);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a client to update someone elses position info
        public static void SendPlayerUpdatePosition(int ClientID, string CharacterName, SwaeloMath.Vector3 NewPosition, SwaeloMath.Vector4 NewRotation)
        {
            //Create the packet to send through the network
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.PlayerUpdatePosition); //write the packet type
            PacketWriter.WriteString(CharacterName);//write the account name
            //write the position data
            PacketWriter.WriteFloat(NewPosition.x);
            PacketWriter.WriteFloat(NewPosition.y);
            PacketWriter.WriteFloat(NewPosition.z);
            //wrote the rotation data
            PacketWriter.WriteFloat(NewRotation.x);
            PacketWriter.WriteFloat(NewRotation.y);
            PacketWriter.WriteFloat(NewRotation.z);
            PacketWriter.WriteFloat(NewRotation.w);
            //send the packet to the client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            //close the packet writer
            PacketWriter.Dispose();
        }
        
        //tells a client to remove someone elses character from their world
        public static void SendRemoveOtherPlayer(int ClientID, string CharacterName)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.RemoveOtherPlayer);  //packet type
            PacketWriter.WriteString(CharacterName);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a list of clients to remove someones character from their world
        public static void SendListRemoveOtherPlayer(List<Client> ClientList, string CharacterName)
        {
            foreach(Client C in ClientList)
                SendRemoveOtherPlayer(C.ClientID, CharacterName);
        }
    }
}
