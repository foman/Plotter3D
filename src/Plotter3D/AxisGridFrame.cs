using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows.Media;

namespace Thorlabs.WPF.Plotter3D
{
    internal class AxisGridFrame : ModelVisual3D
    {
        private double _length = 5;

        public AxisGridFrame()
        {
            CreateGridFrame();
        }

        public double AxisLength
        {
            get { return _length; }
            set { _length = value; }
        }

        private void CreateGridFrame()
        {
            Point3DCollection points = new Point3DCollection();

            #region Add X Axis lines

            points.Add(new Point3D()
            {
                X = 0,
                Y = 0,
                Z = 0
            });
            points.Add(new Point3D()
            {
                X = _length,
                Y = 0,
                Z = 0
            });

            points.Add(new Point3D()
            {
                X = 0,
                Y = 0,
                Z = _length
            });
            points.Add(new Point3D()
            {
                X = _length,
                Y = 0,
                Z = _length
            });

            points.Add(new Point3D()
            {
                X = 0,
                Y = _length,
                Z = 0
            });
            points.Add(new Point3D()
            {
                X = _length,
                Y = _length,
                Z = 0
            });

            #endregion

            #region Add Y Axis lines

            points.Add(new Point3D()
            {
                X = 0,
                Y = 0,
                Z = 0
            });
            points.Add(new Point3D()
            {
                X = 0,
                Y = _length,
                Z = 0
            });

            points.Add(new Point3D()
            {
                X = 0,
                Y = 0,
                Z = _length
            });
            points.Add(new Point3D()
            {
                X = 0,
                Y = _length,
                Z = _length
            });

            points.Add(new Point3D()
            {
                X = _length,
                Y = 0,
                Z = 0
            });
            points.Add(new Point3D()
            {
                X = _length,
                Y = _length,
                Z = 0
            });

            #endregion

            #region Add Z Axis lines

            points.Add(new Point3D()
            {
                X = 0,
                Y = 0,
                Z = 0
            });
            points.Add(new Point3D()
            {
                X = 0,
                Y = 0,
                Z = _length
            });

            points.Add(new Point3D()
            {
                X = 0,
                Y = _length,
                Z = 0
            });
            points.Add(new Point3D()
            {
                X = 0,
                Y = _length,
                Z = _length
            });

            points.Add(new Point3D()
            {
                X = _length,
                Y = 0,
                Z = 0
            });
            points.Add(new Point3D()
            {
                X = _length,
                Y = 0,
                Z = _length
            });

            #endregion

            LinesVisual3D axisLines = new LinesVisual3D()
            {
                Color = Colors.Blue,
                Thickness = 1
            };

            axisLines.Points = points;
            this.Children.Clear();
            this.Children.Add(axisLines);
        }
    }
}
