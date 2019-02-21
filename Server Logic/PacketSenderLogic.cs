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
    UpdateEntityHealth = 11,    //server is telling clients the updated health value of one of the entities

    SpawnOtherPlayer = 12,   //server telling us to spawn another clients character into our world
    RemoveOtherPlayer = 13  //server telling us to remove a disconnected clients character from the world
}

namespace Swaelo_Server
{
    static class PacketSenderLogic
    {
        //displays a message in the clients console log window
        public static void SendConsoleMessage(int ClientID, string Message)
        {
            Log.Out("send console message");
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
            Log.Out("send player message");
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
            Log.Out("send players message");
            foreach (Client OtherClient in TargetClients)
                SendPlayerMessage(OtherClient.ClientID, Sender, Message);
        }

        //tells a client if their account was registered
        public static void SendRegisterReply(int ClientID, bool Success, string Message)
        {
            Log.Out("send register reply");
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
            Log.Out("send login reply");
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
            Log.Out("send create character reply");
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
            Log.Out("send character data");
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SendCharacterData);
            //First we need to look up in the database how many characters this user has created so far
            int CharacterCount = Globals.database.GetCharacterCount(AccountName);
            PacketWriter.WriteInteger(CharacterCount);
            //Now loop through and add all the information for each character that has already been created
            for (int i = 0; i < CharacterCount; i++)
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
            Log.Out("send active entities");
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SpawnActiveEntityList);
            int EntityCount = EntityManager.ActiveEntities.Count;
            PacketWriter.WriteInteger(EntityCount);
            List<BaseEntity> EntityList = EntityManager.ActiveEntities;
            foreach (BaseEntity entity in EntityList)
            {
                PacketWriter.WriteString(entity.ID);
                PacketWriter.WriteFloat(entity.Position.X);
                PacketWriter.WriteFloat(entity.Position.Y);
                PacketWriter.WriteFloat(entity.Position.Z);
            }
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells each in the list of clients about every entity in that list
        public static void SendListEntityUpdates(List<Client> ClientList, List<BaseEntity> EntityList)
        {
            //Log.Out("send entity updates");
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.SendEntityUpdates);
            PacketWriter.WriteInteger(EntityList.Count);
            foreach (var BaseEntity in EntityList)
            {
                //ID
                PacketWriter.WriteString(BaseEntity.ID);
                //Position
                PacketWriter.WriteFloat(BaseEntity.Position.X);
                PacketWriter.WriteFloat(BaseEntity.Position.Y);
                PacketWriter.WriteFloat(BaseEntity.Position.Z);
                //Rotation
                PacketWriter.WriteFloat(BaseEntity.Rotation.X);
                PacketWriter.WriteFloat(BaseEntity.Rotation.Y);
                PacketWriter.WriteFloat(BaseEntity.Rotation.Z);
                PacketWriter.WriteFloat(BaseEntity.Rotation.W);
            }
            foreach (Client Client in ClientList)
                ClientManager.SendPacketTo(Client.ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        public static void UpdateEntityHealth(string EntityID, int EntityHealth)
        {
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.UpdateEntityHealth);
            PacketWriter.WriteString(EntityID);
            PacketWriter.WriteInteger(EntityHealth);
            List<Client> ActiveClients = ClientManager.GetAllActiveClients();
            foreach(Client client in ActiveClients)
                ClientManager.SendPacketTo(client.ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a client to enter into the game world
        public static void SendPlayerEnterWorld(int ClientID)
        {
            Log.Out("sending player ingame");
            Client Client = ClientManager.Clients[ClientID];
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.PlayerEnterWorld); //write the packet type

            //we also want to pass information about all of the other clients already playing the game right now
            List<Client> OtherPlayers = ClientManager.GetActiveClientsExceptFor(ClientID);
            PacketWriter.WriteInteger(OtherPlayers.Count);  //how many other players are in the game
            foreach (Client Other in OtherPlayers)
            {
                //Provide the information for each other player who is in the game
                PacketWriter.WriteString(Other.CurrentCharacterName); //characters name 
                //characters position
                PacketWriter.WriteFloat(Other.CharacterPosition.X);
                PacketWriter.WriteFloat(Other.CharacterPosition.Y);
                PacketWriter.WriteFloat(Other.CharacterPosition.Z);
            }

            //and also, pass information about all of the active entities in the server right now
            List<BaseEntity> ActiveEntities = EntityManager.GetInteractiveEntities();
            //Count
            PacketWriter.WriteInteger(ActiveEntities.Count);
            foreach (BaseEntity Entity in ActiveEntities)
            {
                //Clients should not be told about static entities
                if (Entity.Type == "Static")
                    continue;
                
                //ID
                PacketWriter.WriteString(Entity.ID);
                //Pos
                PacketWriter.WriteFloat(Entity.Position.X);
                PacketWriter.WriteFloat(Entity.Position.Y);
                PacketWriter.WriteFloat(Entity.Position.Z);
            }

            //send the packet to the client
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a client to spawn someone elses character into their game world
        public static void SendSpawnOther(int ClientID, string CharacterName, Vector3 Position)
        {
            Log.Out("send spawn other player");
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
            Log.Out("send list spawn other");
            foreach (Client Client in ClientList)
                SendSpawnOther(Client.ClientID, CharacterName, Position);
        }

        //tells a client to update someone elses position info
        public static void SendPlayerUpdatePosition(int ClientID, string CharacterName, Vector3 NewPosition, Quaternion NewRotation)
        {
            Log.Out("send player update position");
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
            Log.Out("send remove other player");
            ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
            PacketWriter.WriteInteger((int)ServerPacketType.RemoveOtherPlayer);  //packet type
            PacketWriter.WriteString(CharacterName);
            ClientManager.SendPacketTo(ClientID, PacketWriter.ToArray());
            PacketWriter.Dispose();
        }

        //tells a list of clients to remove someones character from their world
        public static void SendListRemoveOtherPlayer(List<Client> ClientList, string CharacterName)
        {
            Log.Out("send list remove other player");
            foreach (Client C in ClientList)
                SendRemoveOtherPlayer(C.ClientID, CharacterName);
        }
    }
}
