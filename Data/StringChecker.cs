// ================================================================================================================================
// File:        StringChecker.cs
// Description: Used to make sure strings do not contain any banned characters before they are used with the SQL database to prevent
//              users from trying to perform any SQL code injection
// ================================================================================================================================

using System;

namespace Server.Data
{
    public static class StringChecker
    {
        //Checks if a desired username contains any banned characters
        public static bool IsValidUsername(string Username)
        {
            for(int i = 0; i < Username.Length; i++)
            {
                //letters and numbers are allowed
                if (Char.IsLetter(Username[i]) || Char.IsNumber(Username[i]))
                    continue;
                //Dashes, Periods and Underscores are allowed
                if (Username[i] == '-' || Username[i] == '.' || Username[i] == '_')
                    continue;

                //Absolutely anything else is banned
                return false;
            }
            return true;
        }
    }
}
