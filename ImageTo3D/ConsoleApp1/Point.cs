using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Point
    {
        private float xCoord;
        private float yCoord;

        public Point(float xCoord, float yCoord)
        {
            this.xCoord = xCoord;
            this.yCoord = yCoord;
        }

        public float getX()
        {
            return xCoord;
        }

        public float getY()
        {
            return yCoord;
        }
    }
}
