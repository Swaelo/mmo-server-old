using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class ListObject
    {
        public Vector3 ObjectPosition = new Vector3(0, 0, 0);
        public float ListValue = -1f;

        public ListObject(Vector3 Position)
        {
            ObjectPosition = Position;
        }
    }
}
