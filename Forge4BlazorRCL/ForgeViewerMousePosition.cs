using System;
using System.Collections.Generic;
using System.Text;

namespace Forge4BlazorRCL
{
    public class ForgeViewerMousePosition
    {
        public double CanvasX { get; set; }
        public double CanvasY { get; set; }
        public double WorldX { get; set; }
        public double WorldY { get; set; }
        public double SnapX { get; set; }
        public double SnapY { get; set; }
        public double SnapZ { get; set; }

        public ForgeViewerMousePosition()
        {

        }

        public ForgeViewerMousePosition(double cx, double cy, double wx, double wy, double sx, double sy, double sz)
        {
            CanvasX = cx;
            CanvasY = cy;
            WorldX = wx;
            WorldY = wy;
            SnapX = sx;
            SnapY = sy;
            SnapZ = sz;
        }
    }
}
