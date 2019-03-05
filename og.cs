// ================================================================================================================================
// File:        l.cs
// Description: Used to quickly and easily print messages to the window to be displayed during runtime for debug purposes
// Author:      Harley Laurie          
// Notes:       Typing l.og("Hello World"); as a line of code will display "Hello World" to the message window
// ================================================================================================================================

using System;

namespace Server
{
    //Log
    public class l
    {
        //Prints m to the log
        public static void og(string m)
        {
            Rendering.Window.DisplayMessage(m);
        }

        //Edits the previously sent message
        public static void ogEdit(string m)
        {
            Rendering.Window.EditPreviousMessage(m);
        }

        public static void og(int value)
        {
            og(value.ToString());
        }

        public static void og(float value)
        {
            og(value.ToString());
        }

        public static void og(bool value)
        {
            og(value.ToString());
        }

        //public static void og(Vector3 Vector)
        //{
        //    og("Vector3: " + Vector.X + ", " + Vector.Y + ", " + Vector.Z);
        //}
    }
}
