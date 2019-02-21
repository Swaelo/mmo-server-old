using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;

namespace Swaelo_Server
{

    public class Log
    {
        public static void Out(string Message)
        {
            ((Main)Globals.MainWindowForm).SetLogMessage(Message);
        }
    }
}
