using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class OrderedList
    {
        //Store all the objects of the list kept sorted in order of distance values
        public  Dictionary<float, ListObject> ListObjects = new Dictionary<float, ListObject>();
        //Track the number of objects that have been added to the list so far
        public  int ListSize = 0;

        //Adds a new object to this list
        public void AddObject(ListObject NewObject, float ListValue)
        {
            //Increase the size of the list
            ListSize++;
            //Place the new object into the dictionary
            ListObjects.Add(ListValue, NewObject);
        }
    }
}
