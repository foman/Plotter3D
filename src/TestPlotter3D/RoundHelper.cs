using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plotter3D;

namespace Plotter3DDemo
{
    internal static class RoundHelper
    {
        internal static int GetDifferenceLog(double min, double max)
        {
            return (int)Math.Log(Math.Abs(max - min));
        }

        internal static double Round(double number, int rem, RoudingMode roundMode)
        {
            var unit = 1d;

            if (rem <= 0)
            {
                rem = MathHelper.Clamp(-rem, 0, 15);
                unit = Math.Pow(10, -rem - 1);

            }
            else
            {
                unit = Math.Pow(10, rem - 1);
            }

            if (roundMode == RoudingMode.Celling)
            {
                number = Math.Round((number + unit / 2) / unit, 0) * unit;

            }
            else
            {
                number = Math.Round((number - unit / 2) / unit, 0) * unit;
            }
            return number;
            //    if (roundMode == RoudingMode.Floor)
            //    {
            //        return Math.Round(number - unit / 2, rem + 1);
            //    }
            //    else
            //    {
            //        return Math.Round(number + unit / 2, rem + 1);
            //    }
            //}
            //else
            //{
            //    if (roundMode == RoudingMode.Celling)
            //    {
            //        var unit = Math.Pow(10, rem - 1);
            //        number = Math.Round((number + unit / 2) / unit, rem - 1) * unit;

            //        //upperBound = Math.Ceiling(number);
            //        //upperBound = upperBound + 5 - upperBound % 5;
            //    }
            //    else
            //    {
            //        var unit = Math.Pow(10, rem - 1);
            //        number = Math.Round((number - unit / 2) / unit, rem - 1) * unit;

            //        //upperBound = Math.Floor(number);
            //        //upperBound = upperBound - upperBound % 5;
            //    }

            //    return number;
            //}
        }

        internal static Range<double> CreateRoundedRange(double min, double max)
        {

            //return new Range<double>(Math.Floor(min), Math.Ceiling(max));

            double delta = max - min;

            if (delta == 0)
                return new Range<double>(min, max);

            int log = (int)Math.Floor(Math.Log10(Math.Abs(delta)));

            double newMin = Round(min, log, RoudingMode.Floor);
            double newMax = Round(max, log, RoudingMode.Celling);
            if (newMin == newMax)
            {
                log--;
                newMin = Round(min, log, RoudingMode.Floor);
                newMax = Round(max, log, RoudingMode.Celling);
            }

            return new Range<double>(newMin, newMax);

            //return new Math
        }

        internal enum RoudingMode
        {
            Celling = 0,
            Floor = 1
        }
    }
}
