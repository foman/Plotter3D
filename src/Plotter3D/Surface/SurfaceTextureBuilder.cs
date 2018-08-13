using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;


namespace Plotter3D
{
    public abstract class SurfaceTextureBuilder
    {
        public Range<double> DataRange
        {
            get;
            set;
        }

        public abstract Material CreateTexture();

        public abstract Point GetTextureMapping(double zData);
    }
}
