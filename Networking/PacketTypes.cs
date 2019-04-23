// ================================================================================================================================
// File:        PacketTypes.cs
// Description: Defines ID numbers for each type of packet send across the network
// ================================================================================================================================


namespace Server.Networking
{
    public enum PacketTypes
    {
        //User Account Management
        UserAccountRegistrationRequest, //User is requesting registration of a brand new account
        UserAccountRegistrationReply,   //User is being told if their registration request was accepted
        UserAccountLoginRequest,        //User is requesting to log into an exiting account
        UserAccountLoginReply,          //User is being told if their login request was accepted
        //Player Character Management
        CharacterCreationRequest,       //User is requesting creation of a new player character
        CharacterCreationReply,         //User is being told if their new character creation was accepted
        CharacterDataRequest,           //User is requesting information about all the characters they own
        CharacterDataReply,             //User is being told about all the characters they own
        CharacterLoginRequest,          //User is requesting to enter the game world with one of their characters
        CharacterLoginReply,            //User is being told if their request to enter the game world was accepted
        //World State Queries
        ActiveEntitiesInfoRequest,      //User is requesting the current status of all entities currently in the game
        ActiveEntitiesInfoReply,        //User is being told the current status of all entities currently in the game
        ActiveItemsInfoRequest,         //User is requesting the current status of all item picks in the game world
        ActiveItemsInfoReply,           //User is being told the current status of all item picks up in the game world
        ActivePlayersInfoRequest,
        ActivePlayersInfoReply,
        //Gameplay Events
        PlayerMessageInfo,
        PlayerMessageDelivery
    }
}
