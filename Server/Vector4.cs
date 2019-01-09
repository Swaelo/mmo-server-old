using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Vector4
    {
        public float x = 0.0f;
        public float y = 0.0f;
        public float z = 0.0f;
        public float w = 0.0f;

        public Vector4()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
            this.w = 0.0f;
        }

        public Vector4(float value)
        {
            this.x = value;
            this.y = value;
            this.z = value;
            this.w = value;
        }

        public Vector4(float xValue, float yValue, float zValue, float wValue)
        {
            this.x = xValue;
            this.y = yValue;
            this.z = zValue;
            this.w = wValue;
        }
    }
}
