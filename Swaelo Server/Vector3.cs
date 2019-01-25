using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    class Vector3
    {
        public float x = 0.0f;
        public float y = 0.0f;
        public float z = 0.0f;

        public Vector3()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
        }

        public Vector3(float value)
        {
            this.x = value;
            this.y = value;
            this.z = value;
        }

        public Vector3(float xValue, float yValue, float zValue)
        {
            this.x = xValue;
            this.y = yValue;
            this.z = zValue;
        }
    }
}
