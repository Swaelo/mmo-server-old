using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    SpawnOtherPlayers = 10,  //tells a client to spawn a list of other players to spawn into their world
    RemoveOtherPlayer = 11  //tells a client to remove someone elses character from their world
}

namespace Swaelo_Server
{
    static class PacketSender
    {
        //displays a message in the clients console log window
        public static void SendConsoleMessage(int ClientID, string Message)
        {
            Console.WriteLine("Sending console message to client");
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

        //tells a client if their account was registered
        public static void SendRegisterReply(int ClientID, bool Success, string Message)
        {
            Console.WriteLine("Telling a client their account registration request was " + (Success ? "accepted" : "denied"));
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
            Console.WriteLine("Telling a client their account login request was " + (Success ? "accepted" : "denied"));
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
            Console.WriteLine("telling a client all the information for each character they have created thus far");
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
        public static void SendPlayerEnterWorld(int ClientID, CharacterData Data)
        {
            Console.WriteLine("instructing " + Data.Account + " to enter the game world");
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.PlayerEnterWorld); //write the packet type
            PacketWriter.WriteString(Data.Account);
            PacketWriter.WriteString(Data.Name);
            PacketWriter.WriteInteger(Data.IsMale ? 1 : 0);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a client to update someone elses position info
        public static void SendPlayerUpdatePosition(int ClientID, string CharacterName, Vector3 NewPosition)
        {
            Console.WriteLine("spreading player location update to another client");
            //Create the packet to send through the network
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.PlayerUpdatePosition); //write the packet type
            PacketWriter.WriteString(CharacterName);//write the account name
            //write the position data
            PacketWriter.WriteFloat(NewPosition.x);
            PacketWriter.WriteFloat(NewPosition.y);
            PacketWriter.WriteFloat(NewPosition.z);
            //send the packet to the client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            //close the packet writer
            PacketWriter.Dispose();
        }

        //tells a client to spawn someone elses character into their world
        public static void SendSpawnOtherPlayer(int ClientID, string AccountName, CharacterData Data)
        {
            Console.WriteLine("instructing " + AccountName + " to spawn " + Data.Account + " character in their world");
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SpawnOtherPlayer);  //packet type
            PacketWriter.WriteString(Data.Account); //account name
            PacketWriter.WriteFloat(Data.Position.x);   //character world position
            PacketWriter.WriteFloat(Data.Position.y);
            PacketWriter.WriteFloat(Data.Position.z);
            PacketWriter.WriteString(Data.Name);    //character name
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a client to spawn a list of other players to spawn into their world
        public static void SendSpawnOtherPlayers(int ClientID, List<Client> OtherPlayers)
        {
            Console.WriteLine("telling the new client to spawn everyone else into their game world");
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SpawnOtherPlayers);  //packet type
            PacketWriter.WriteInteger(OtherPlayers.Count);  //amount of other clients to spawn in

            //Loop through the list of other clients that need to be spawned in
            foreach (Client Other in OtherPlayers)
            {
                //Write into the packet the information for each other player in the game right now
                PacketWriter.WriteString(Other.AccountName);
                PacketWriter.WriteFloat(Other.CharacterPosition.x);
                PacketWriter.WriteFloat(Other.CharacterPosition.y);
                PacketWriter.WriteFloat(Other.CharacterPosition.z);
                PacketWriter.WriteString(Other.CurrentCharacterName);
            }

            //Now send the packet to the client and close the reader
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
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
    }
}
