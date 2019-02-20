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
            Console.Title = "MMO" + " -- Players online: " + 0 + " / Max online: " + 0 + "";
            System.Windows.Forms.Application.Run(new Server());
        }
        
    }
}