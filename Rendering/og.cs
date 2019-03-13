// ================================================================================================================================
// File:        l.cs
// Description: Used to quickly and easily print messages to the window to be displayed during runtime for debug purposes
// ================================================================================================================================

using BEPUutilities;

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

        public static void og(Vector3 vector)
        {
            og(vector.X + ", " + vector.Y + ", " + vector.Z);
        }
    }
}
