using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class IDPair
    {
        public string FirstID = "-1";
        public string SecondID = "-2";

        public IDPair(string ID, string ID2)
        {
            FirstID = ID;
            SecondID = ID2;
        }
    }
}
