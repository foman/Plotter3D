using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;

namespace Plotter3D
{
    /// <summary>
    /// The Wireframe formed by connecting adjacent points
    /// </summary>
	public class SurfaceWireframe : LinesVisual3D
    {
        public new Point3D[,] Points
        {
            set
            {
                this.AddWirefreamPoints(value);
                base.UpdateGeometry();
            }
        }

        private void AddWirefreamPoints(Point3D[,] pts)
        {
            Point3DCollection points = new Point3DCollection(pts.Length);
            int xLength = pts.GetLength(0);
            int yLength = pts.GetLength(1);
            for (int x = 0; x < xLength - 1; x++)
            {
                if (!double.IsNaN(pts[x, 0].Z))
                {
                    if (double.IsNaN(pts[x + 1, 0].Z))
                    {
                        for (int y = 0; y < yLength - 1; y++)
                        {
                            points.Add(pts[x, y]);
                            points.Add(pts[x, y + 1]);
                        }
                    }
                    else
                    {
                        for (int y = 0; y < yLength - 1; y++)
                        {
                            points.Add(pts[x, y]);
                            points.Add(pts[x, y + 1]);
                            points.Add(pts[x, y]);
                            points.Add(pts[x + 1, y]);
                        }
                        points.Add(pts[x, yLength - 1]);
                        points.Add(pts[x + 1, yLength - 1]);
                    }
                }
            }
            if (!double.IsNaN(pts[xLength - 1, 0].Z))
            {
                for (int y = 0; y < yLength - 1; y++)
                {
                    points.Add(pts[xLength - 1, y]);
                    points.Add(pts[xLength - 1, y + 1]);
                }
            }
            points.Freeze();
            base.Points = points;
        }
    }
}
