using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptureStacker.Entity
{
    class MonitorTotalMaxSize
    {
        private int[] size = new int[2];

        public int Width
        {
            get
            {
                return this.size[0];
            }
            set
            {
                this.size[0] = value;
            }
        }

        public int Height
        {
            get
            {
                return this.size[1];
            }
            set
            {
                this.size[1] = value;
            }
        }
    }
}
