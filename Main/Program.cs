// ================================================================================================================================
// File:        Program.cs
// Description: Application Main EntryPoint, opens the server app window which in itself does everything else
// Author:      Robert          
// Notes:       Whats robs surname lol
// ================================================================================================================================

using System;
using System.Text;
using System.Timers;
using System.Threading;
using System.Collections.Generic;

using System.Windows.Forms;
namespace Swaelo_Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            System.Windows.Forms.Application.Run(new Main());
        }
    }
}