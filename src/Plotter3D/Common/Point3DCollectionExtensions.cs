using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Plotter3D.Common
{
    public static class Point3DCollectionExtensions
    {
        public static void Add(this Point3DCollection pntColl, Point3D[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                pntColl.Add(points[i]);
            }
        }
    }
}
