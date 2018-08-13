using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using Plotter3D.Common;

namespace Plotter3D
{
    public class VisualSpectrumTextureBuilder : SurfaceTextureBuilder
    {
        public DiffuseMaterial m_material;

        public WriteableBitmap bitMap = null;

        public Range<int> waveLengRange = new Range<int>(380, 720);

        private int colorCount = 128;

        private Material _vsTexture = null;

        public override Material CreateTexture()
        {
            if (_vsTexture == null)
            {
                _vsTexture = this.CreateVisibleSpectrumMaterial();
            }
            return _vsTexture;
        }

        public override Point GetTextureMapping(double value)
        {
            Point pt = new Point(Math.Abs(Math.Round((value - base.DataRange.Min) / (base.DataRange.Max - base.DataRange.Min) * (double)this.colorCount) - 0.5) / (double)this.colorCount, 1.0);
            return pt;
        }

        private unsafe Material CreateVisibleSpectrumMaterial()
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(this.colorCount, 1, 96.0, 96.0, PixelFormats.Bgra32, null);
            writeableBitmap.Lock();
            unsafe
            {
                // Get a pointer to the back buffer.
                byte* pStart = (byte*)(void*)writeableBitmap.BackBuffer;
                //int nL = writeableBitmap.BackBufferStride;
                double step = (waveLengRange.Max - waveLengRange.Min) / (double)colorCount;

                for (int ix = 0; ix < colorCount; ix += 1)
                {
                    Color color = ColorHelper.WavelengthToColor(waveLengRange.Min + Convert.ToInt32(ix * step));

                    *(pStart + ix * 4 + 0) = (byte)(color.B);
                    *(pStart + ix * 4 + 1) = (byte)(color.G);
                    *(pStart + ix * 4 + 2) = (byte)(color.R);
                    *(pStart + ix * 4 + 3) = (byte)(color.A);
                }
            }

            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, colorCount, 1));

            // Release the back buffer and make it available for display.
            writeableBitmap.Unlock();

            MaterialGroup materials = new MaterialGroup();

            ImageBrush imageBrush = new ImageBrush(writeableBitmap);
            imageBrush.ViewportUnits = BrushMappingMode.Absolute;

            var material = new DiffuseMaterial();
            material.Brush = imageBrush;
            //material.Brush = Brushes.Blue;
            materials.Children.Add(material);

            var spectMaterial = new SpecularMaterial(
                imageBrush, 90);
            //materials.Children.Add(spectMaterial);


            bitMap = writeableBitmap;
            return materials;// material;
        }
    }
}