// ================================================================================================================================
// File:        l.cs
// Description: Used to quickly and easily print messages to the window to be displayed during runtime for debug purposes
// ================================================================================================================================

using System;
using BEPUutilities;

namespace Server
{
    //Log
    public class l
    {
        //Prints m to the log
        public static void og(string Message)
        {
            Rendering.Window.DisplayMessage(Message);
        }

        //Edits the previously sent message
        public static void ogEdit(string Message)
        {
            Rendering.Window.EditPreviousMessage(Message);
        }

        //Converts an integer value to a string and prints that to the log
        public static void og(int Value)
        {
            og(Value.ToString());
        }

        //Converts a floating point value to a string and prints that to the log
        public static void og(float Value)
        {
            og(Value.ToString());
        }

        //Converts a boolean value to a string and prints that to the log
        public static void og(bool Value)
        {
            og(Value.ToString());
        }

        //Displays a vector3 to the log in a nicely formatted way
        public static void og(Vector3 Vector)
        {
            og(Vector.X + ", " + Vector.Y + ", " + Vector.Z);
        }

        //Displays a quaternion to the log in a nicely formatted way
        public static void og(Quaternion Quaternion)
        {
            og(Quaternion.X + ", " + Quaternion.Y + ", " + Quaternion.Z + ", " + Quaternion.W);
        }
    }
}
