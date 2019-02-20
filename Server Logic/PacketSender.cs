using System;
using System.Collections.Generic;
using System.Text;

public enum ServerPacketType
{
    ConsoleMessage = 1, //message from server to display a message in the console log window
    PlayerMessage = 2,  //message from server to display a players chat message in the chat window

    RegisterReply = 3,  //reply from server if our account registration was successful
    LoginReply = 4, //reply from server if our account login was successful
    CreateCharacterReply = 5,    //reply from server if our new character creation was successful

    SendCharacterData = 6,  //reply from server with all our created characters information
    PlayerEnterWorld = 7,    //server telling us to enter the game world with our selected character
    PlayerUpdatePosition = 8,   //server giving us another players updated position information

    SpawnActiveEntityList = 9,  //server gives us a list of all the active entities in the game for us to spawn in
    SendEntityUpdates = 10, //server is giving us the updated info for all the entities active in the game right now

    SpawnOtherPlayer = 11,   //server telling us to spawn another clients character into our world
    RemoveOtherPlayer = 12  //server telling us to remove a disconnected clients character from the world
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
                PacketWriter.WriteFloat(Data.Position.X);
                PacketWriter.WriteFloat(Data.Position.Y);
                PacketWriter.WriteFloat(Data.Position.Z);
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

        //tells a client where all the active entities are when they are first entering the server
        public static void SendActiveEntities(int ClientID)
        {
            //Initialise the header data of the network packet
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SpawnActiveEntityList);
            //Find out how many active entities there are and write the count value in the packet data
            int EntityCount = EntityManager.ActiveEntities.Count;
            PacketWriter.WriteInteger(EntityCount);
            //Loop through each of the entities that are active in the scene right now
            List<ServerEntity> EntityList = EntityManager.GetEntityList();
            foreach(ServerEntity entity in EntityList)
            {
                //We need to save each entities ID, Type, and World Position
                PacketWriter.WriteString(entity.ID);
                PacketWriter.WriteString(entity.Type);
                PacketWriter.WriteFloat(entity.entity.Position.X);
                PacketWriter.WriteFloat(entity.entity.Position.Y);
                PacketWriter.WriteFloat(entity.entity.Position.Z);
            }
            //Once the packet has all the information, close it and send it off to the client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells each in the list of clients about every entity in that list
        public static void SendListEntityUpdates(List<Client> ClientList, List<ServerEntity> EntityList)
        {
            //Define a network packet which lists the updated targets for each entity in the given list
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SendEntityUpdates);
            //The client will need to know how much entity updates are in the network packet
            PacketWriter.WriteInteger(EntityList.Count);
            //Now write in the required data for every entity in the list
            foreach(var Entity in EntityList)
            {
                //Entity ID
                PacketWriter.WriteString(Entity.ID);
                //New Entity Position
                PacketWriter.WriteFloat(Entity.entity.Position.X);
                PacketWriter.WriteFloat(Entity.entity.Position.Y);
                PacketWriter.WriteFloat(Entity.entity.Position.Z);
                //Their rotation values too
                PacketWriter.WriteFloat(Entity.entity.Orientation.X);
                PacketWriter.WriteFloat(Entity.entity.Orientation.Y);
                PacketWriter.WriteFloat(Entity.entity.Orientation.Z);
                PacketWriter.WriteFloat(Entity.entity.Orientation.W);
            }
            //The packet is ready, now send it to everyone in the list
            foreach (Client Client in ClientList)
                ClientManager.SendPacketTo(Client.ClientID, PacketWriter.ToArray());
            //Close up the packet and finish off
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
                PacketWriter.WriteFloat(Other.CharacterPosition.X);
                PacketWriter.WriteFloat(Other.CharacterPosition.Y);
                PacketWriter.WriteFloat(Other.CharacterPosition.Z);
            }
            
            //send the packet to the client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a client to spawn someone elses character into their game world
        public static void SendSpawnOther(int ClientID, string CharacterName, Vector3 Position)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SpawnOtherPlayer);  //packet type
            PacketWriter.WriteString(CharacterName);
            PacketWriter.WriteFloat(Position.X);
            PacketWriter.WriteFloat(Position.Y);
            PacketWriter.WriteFloat(Position.Z);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells everyone in a list of clients to spawn a specific player character into their game worlds
        public static void SendListSpawnOther(List<Client> ClientList, string CharacterName, Vector3 Position)
        {
            foreach(Client Client in ClientList)
                SendSpawnOther(Client.ClientID, CharacterName, Position);
        }

        //tells a client to update someone elses position info
        public static void SendPlayerUpdatePosition(int ClientID, string CharacterName, Vector3 NewPosition, Quaternion NewRotation)
        {
            //Create the packet to send through the network
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.PlayerUpdatePosition); //write the packet type
            PacketWriter.WriteString(CharacterName);//write the account name
            //write the position data
            PacketWriter.WriteFloat(NewPosition.X);
            PacketWriter.WriteFloat(NewPosition.Y);
            PacketWriter.WriteFloat(NewPosition.Z);
            //wrote the rotation data
            PacketWriter.WriteFloat(NewRotation.X);
            PacketWriter.WriteFloat(NewRotation.Y);
            PacketWriter.WriteFloat(NewRotation.Z);
            PacketWriter.WriteFloat(NewRotation.W);
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
