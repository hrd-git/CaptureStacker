using CaptureStacker.Win32API;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CaptureStacker.Entity
{
    class MonitorInfo
    {
        private int width;

        private int height;

        private Point location;

        public int Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        public int Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public Point Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        public MonitorInfo(int width, int height, Point location)
        {
            this.width = width;
            this.height = height;
            this.location = location;
        }

    }
}
