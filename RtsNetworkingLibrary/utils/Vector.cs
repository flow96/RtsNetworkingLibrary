using System;

namespace RtsNetworkingLibrary.utils
{
    
    [Serializable]
    public class Vector
    {
        public float x, y, z;

        public Vector(float x = 0, float y = 0, float z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}