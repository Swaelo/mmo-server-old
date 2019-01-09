using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ServerLoop
    {
        Timer BackupTimer;

        public void Loop()
        {
            //back up every connected players information into the database automatically every 5 minutes
            BackupTimer = new Timer(SavePlayers, null, 0, 300000);

        }

        public void SavePlayers(Object o)
        {
            Console.WriteLine("backing up all players data");
            //Loop through the list of players
            for (int PlayerIterator = 0; PlayerIterator < Constants.MAX_PLAYERS; PlayerIterator++)
            {
                //Save the new position info of any connected players
                if(Globals.general.IsClientConnected(PlayerIterator))
                {
                    Globals.database.SavePlayerData(PlayerIterator);
                }
            }
        }
    }
}
