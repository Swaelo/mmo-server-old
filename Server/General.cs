using System;

namespace Server
{
    class General
    {
        //Starts up the game server
        public void InitServer()
        {
            //Connect to the MySQL database and start the server
            Globals.mysql.MySQLInit();
            InitGameData();
            Globals.network.InitTCP();
        }

        //Sets up all the arrays which store information about any clients that connect and their characters inside the game world
        public void InitGameData()
        {
            for (int i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                Globals.Clients[i] = new Client();
                Globals.LivePlayers[i] = new LivePlayer();
            }
        }

        //Checks if a client is still connected to the server
        public bool IsClientConnected(int ClientIndex)
        {
            //Console.WriteLine("Checking if client index " + ClientIndex + " is connected to the server");
            if(Globals.Clients[ClientIndex] != null)
            {
                if(Globals.LivePlayers[ClientIndex].IsPlaying)
                {
                    //Console.WriteLine("They are still connected");
                    return true;
                }
                else
                {
                    //Console.WriteLine("They are no longer connected");
                    return false;
                }
            }
           // Console.WriteLine("Client wasnt found");
            return false;
        }

        //Gets a players position in the game world
        public Vector3 GetPlayerPosition(int PlayerIndex)
        {
            if (PlayerIndex <= 0 || PlayerIndex > Constants.MAX_PLAYERS)
                return new Vector3();
            return Globals.LivePlayers[PlayerIndex].PlayerPosition;
        }

        //Sets a players position in the game world
        public void SetPlayerPosition(int PlayerIndex, Vector3 TargetPosition)
        {
            if (PlayerIndex <= 0 || PlayerIndex > Constants.MAX_PLAYERS)
                return;
            Globals.LivePlayers[PlayerIndex].PlayerPosition = TargetPosition;
        }

        //Gets a players rotation in the game world
        public Vector4 GetPlayerRotation(int PlayerIndex)
        {
            if (PlayerIndex <= 0 || PlayerIndex > Constants.MAX_PLAYERS)
                return new Vector4();
            return Globals.LivePlayers[PlayerIndex].PlayerRotation;
        }

        //Sets a players rotation in the game world
        public void SetPlayerRotation(int PlayerIndex, Vector4 TargetRotation)
        {
            if (PlayerIndex <= 0 || PlayerIndex > Constants.MAX_PLAYERS)
                return;
            Globals.LivePlayers[PlayerIndex].PlayerRotation = TargetRotation;
        }
    }
}
