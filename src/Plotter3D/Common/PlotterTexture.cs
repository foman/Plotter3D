using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;


namespace Thorlabs.WPF.Plotter3D.Common
{
    public class VisualSpectrumTextureBuilder : PlotterTextureBuilder
    {
        public DiffuseMaterial m_material;
        public WriteableBitmap bitMap = null;

        private int colorCount = 64;

        private Material _vsTexture = null;

        public override Material CreateTexture(Point3D[,] points)
        {
            if (_vsTexture == null)
            {
                this._vsTexture = CreateVisibleSpectrumMaterial(points);
            }

            return _vsTexture;
        }

        /// <summary>
        /// Get corresponding texture coordinate according the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override Point GetTextureMapping(double value)
        {
            //Beacause the color is just one pixel, and the color become darker from the middle,X value should be 0.5 at the center
            //the positon of the correct color.

            if (value == DataRange.Min)
                return new Point(0.5 / colorCount, 1);

            return new Point((Math.Round((value - DataRange.Min) / (DataRange.Max - DataRange.Min) * colorCount) - 0.5d) / colorCount, 1);
        }

        private Material CreateVisibleSpectrumMaterial(Point3D[,] points)
        {

            int xWidth = points.GetLength(0);
            int yWidth = points.GetLength(1);

            colorCount = xWidth * yWidth;

            WriteableBitmap writeableBitmap = new WriteableBitmap(colorCount, 1, 96, 96, PixelFormats.Bgr24, null);
            writeableBitmap.Lock();

            unsafe
            {
                // Get a pointer to the back buffer.
                byte* pStart = (byte*)(void*)writeableBitmap.BackBuffer;
                int nL = writeableBitmap.BackBufferStride;

                for (int ix = 0; ix < colorCount; ix += 1)
                {
                    Color color = ColorHelper.GetGradientColor(Colors.Blue, Colors.Red, (double)ix / colorCount);

                    *(pStart + ix * 3 + 0) = (byte)(color.B);
                    *(pStart + ix * 3 + 1) = (byte)(color.G);
                    *(pStart + ix * 3 + 2) = (byte)(color.R);

                }
            }

            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, colorCount, 1));

            // Release the back buffer and make it available for display.
            writeableBitmap.Unlock();

            ImageBrush imageBrush = new ImageBrush(writeableBitmap);
            imageBrush.ViewportUnits = BrushMappingMode.Absolute;

            SpecularMaterial material = new SpecularMaterial();
            material.Brush = imageBrush;

            bitMap = writeableBitmap;
            return material;
        }
    }

    public abstract class PlotterTextureBuilder
    {
        public Range<double> DataRange
        {
            get;
            set;
        }
        public abstract Material CreateTexture(Point3D[,] points);
        public abstract Point GetTextureMapping(double zData);
    }
}
