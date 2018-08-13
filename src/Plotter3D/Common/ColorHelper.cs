using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Plotter3D.Common
{
    public static class ColorHelper
    {

        public static Color GetGradientColor(Color start, Color end, double propertion)
        {
            var r = (int)((end.R - start.R) * propertion) + start.R;
            var g = (int)((end.G - start.G) * propertion) + start.G;
            var b = (int)((end.B - start.B) * propertion) + start.B;
            var a = (int)((end.A - start.A) * propertion) + start.A;

            return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);

        }

        public static Color WavelengthToColor(int wavelength)
        {

            double R,
                G,
                B,
                alpha,
                wl = wavelength;

            if (wl >= 380 && wl < 440)
            {
                R = -1 * (wl - 440) / (440 - 380);
                G = 0;
                B = 1;
            }
            else if (wl >= 440 && wl < 490)
            {
                R = 0;
                G = (wl - 440) / (490 - 440);
                B = 1;
            }
            else if (wl >= 490 && wl < 510)
            {
                R = 0;
                G = 1;
                B = -1 * (wl - 510) / (510 - 490);
            }
            else if (wl >= 510 && wl < 580)
            {
                R = (wl - 510) / (580 - 510);
                G = 1;
                B = 0;
            }
            else if (wl >= 580 && wl < 645)
            {
                R = 1;
                G = -1 * (wl - 645) / (645 - 580);
                B = 0.0;
            }
            else if (wl >= 645 && wl <= 780)
            {
                R = 1;
                G = 0;
                B = 0;
            }
            else
            {
                R = 0;
                G = 0;
                B = 0;
            }


            //// intensty is lower at the edges of the visible spectrum.

            if (wl > 780 || wl < 380)
            {
                alpha = 1;
            }
            else if (wl > 700)
            {
                alpha = 0.3 + 0.7 * (780 - wl) / (780 - 700);
            }
            //else if (wl < 420)
            //{
            //    alpha = 0.3 + 0.7 * (wl - 380) / (420 - 380);
            //}
            else
            {
                alpha = 1;
            }

            var color = Color.FromArgb((byte)Convert.ToInt32(alpha * 255),
                (byte)Convert.ToInt32(R * 255),
                (byte)Convert.ToInt32(G * 255),
                (byte)Convert.ToInt32(B * 255));

            return color;
        }

    }
}
