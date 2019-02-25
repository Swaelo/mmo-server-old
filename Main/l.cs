// ================================================================================================================================
// File:        l.cs
// Description: Used to quickly and easily print messages to the window to be displayed during runtime for debug purposes
// Author:      Harley Laurie          
// Notes:       Typing l.o("Hello World"); as a line of code will display "Hello World" to the message window
// ================================================================================================================================

//Very quickly and easily prints a message to the window

namespace Swaelo_Server
{
    //Log
    public class l
    {
        //Out(Message)
        public static void o(string m)
        {
            ((Main)Globals.MainWindowForm).SetLogMessage(m);
        }

        //Out(Vector3)
        public static void o(Vector3 v)
        {
            ((Main)Globals.MainWindowForm).SetLogMessage(v.X + ", " + v.Y + ", " + v.Z);
        }
    }
}
