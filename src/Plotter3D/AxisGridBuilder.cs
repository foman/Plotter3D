using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows.Media;
using Thorlabs.WPF.Plotter3D.Common;

namespace Thorlabs.WPF.Plotter3D
{
    public class AxisGridBuilder
    {
        private double _length = 5;
        public double AxisLength
        {
            get { return _length; }
            set { _length = value; }
        }

        private Range<double> _xRange;
        public Range<double> XRange
        {
            set { _xRange = value; }
            get { return _xRange; }
        }

        private Range<double> _yRange;
        public Range<double> YRange
        {
            set { _yRange = value; }
            get { return _yRange; }
        }

        private Range<double> _zRange;
        public Range<double> ZRange
        {
            set { _zRange = value; }
            get { return _zRange; }
        }

        public ITickProvider TicksProvider
        {
            get;
            set;
        }

        private ModelVisual3D axisVisuals = new ModelVisual3D();
        private Model3DGroup mayorTickLabelGroup = new Model3DGroup();
        private Model3DGroup axisLabelGroup = new Model3DGroup();
        private TickInfo[] _xAxisMayorTicks = null;

        private Transform3D dataAxisTransform = null;
        public Transform3D DataToAxisTransform
        {
            get
            {
                if (dataAxisTransform == null)
                {
                    Transform3DGroup transformGroup = new Transform3DGroup();
                    TranslateTransform3D transform1 = new TranslateTransform3D()
                    {
                        OffsetX = 0 - _xRange.Min,
                        OffsetY = 0 - _yRange.Min,
                        OffsetZ = 0 - _zRange.Min
                    };
                    transformGroup.Children.Add(transform1);
                    ScaleTransform3D transform2 = new ScaleTransform3D();
                    transform2.ScaleX = _length / (_xRange.Max - _xRange.Min);
                    transform2.ScaleY = _length / (_yRange.Max - _yRange.Min);
                    transform2.ScaleZ = _length / (_zRange.Max - _zRange.Min);

                    transformGroup.Children.Add(transform2);
                    dataAxisTransform = transformGroup;
                }
                return dataAxisTransform;
            }
        }

        public ModelVisual3D DrawAxisGrid()
        {
            Point3DCollection points = new Point3DCollection();
            DrawXOZAxisGrid(points);
            Point3D[] sufacePoints = new Point3D[points.Count];
            points.CopyTo(sufacePoints, 0);//XOZ Point
            AddYOZFromXOZ(points, sufacePoints);
            //Now sufacePoints is YOZ points
            AddXOYFromYOZ(points, sufacePoints);
            LinesVisual3D axisLines = new LinesVisual3D()
            {
                Color = Colors.Blue,
                Thickness = 1
            };
            axisLines.Points = points;
            axisVisuals.Children.Add(axisLines);

            DrawXAxisLabels();
            DrawYAxisLabels();
            DrawZAxisLabels();

            ModelVisual3D tickLabels = new ModelVisual3D();
            tickLabels.Content = mayorTickLabelGroup;
            axisVisuals.Children.Add(tickLabels);

            ModelVisual3D axisLabels = new ModelVisual3D();
            axisLabels.Content = axisLabelGroup;
            axisVisuals.Children.Add(axisLabels);


            return axisVisuals;
        }

        private void DrawXOZAxisGrid(Point3DCollection points)
        {
            _xAxisMayorTicks = this.TicksProvider.CreateMayorTicks(_xRange, _length);

            foreach (var mayorTick in _xAxisMayorTicks)
            {
                //Add X Axis lines
                points.Add(new Point3D()
                {
                    X = 0,
                    Y = 0,
                    Z = mayorTick.AxisValue
                });
                points.Add(new Point3D()
                {
                    X = _length,
                    Y = 0,
                    Z = mayorTick.AxisValue
                });

                //Add +Z Axis lines
                points.Add(new Point3D()
                {
                    X = mayorTick.AxisValue,
                    Y = 0,
                    Z = 0
                });
                points.Add(new Point3D()
                {
                    X = mayorTick.AxisValue,
                    Y = 0,
                    Z = _length
                });
            }

            DrawXAxisMinorTicks(points);
        }

        private void AddYOZFromXOZ(Point3DCollection points, Point3D[] xozPnts)
        {
            //Add YOZ Grid
            RotateTransform3D transform = new RotateTransform3D()
            {
                Rotation = new AxisAngleRotation3D() { Axis = new Vector3D(0, 0, 1), Angle = 90 }
            };
            AddTransformedPoints(points, transform, xozPnts);
        }

        private void AddXOYFromYOZ(Point3DCollection points, Point3D[] xozPnts)
        {
            //Add XOY Grid
            RotateTransform3D transform = new RotateTransform3D()
            {
                Rotation = new AxisAngleRotation3D() { Axis = new Vector3D(0, 1, 0), Angle = 90 }
            };
            AddTransformedPoints(points, transform, xozPnts);
        }

        private void DrawXAxisMinorTicks(Point3DCollection points)
        {
            Point3DCollection pointsColl = new Point3DCollection();

            var minorTicks = this.TicksProvider.CreateMinorTicks(_xRange, _length);
            foreach (var tick in minorTicks)
            {
                Point3D start = new Point3D()
                {
                    X = tick.AxisValue,
                    Y = 0,
                    Z = _length
                };

                Point3D end = new Point3D()
                {
                    X = tick.AxisValue,
                    Y = 0,
                    Z = _length - this.TicksProvider.MinorTickSize
                };

                pointsColl.Add(start);
                pointsColl.Add(end);
            }

            //Add (1,0,0) side minor ticks
            Point3D[] oneSidesPoints = new Point3D[pointsColl.Count];
            pointsColl.CopyTo(oneSidesPoints, 0);
            Transform3D transform = new TranslateTransform3D()
            {
                OffsetX = 0,
                OffsetY = 0,
                OffsetZ = this.TicksProvider.MinorTickSize - _length
            };
            AddTransformedPoints(pointsColl, transform, oneSidesPoints);

            //Add the other two sides points on XOZ
            Point3D[] towSidesPoints = new Point3D[pointsColl.Count];
            pointsColl.CopyTo(towSidesPoints, 0);

            transform = new RotateTransform3D()
            {
                CenterX = _length / 2,
                CenterY = 0,
                CenterZ = _length / 2,
                Rotation = new AxisAngleRotation3D() { Axis = new Vector3D(0, 1, 0), Angle = 90 }
            };
            AddTransformedPoints(pointsColl, transform, towSidesPoints);

            foreach (var point in pointsColl)
            {
                points.Add(point);
            }
        }

        private void DrawXAxisLabels()
        {

            /// Prepare transformation for transfroming label
            /// from one side to the other side.
            Transform3D transform = new RotateTransform3D()
            {
                Rotation = new AxisAngleRotation3D() { Axis = new Vector3D(1, 0, 0), Angle = 90 }
            };

            var transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(transform);

            transform = new TranslateTransform3D()
            {
                OffsetX = 0,
                OffsetY = _length * 2 + 1,
                OffsetZ = 0
            };
            transformGroup.Children.Add(transform);

            foreach (var tick in _xAxisMayorTicks)
            {
                var mayorTickLabel =
                    Text3D.CreateTextLabel3D(this.TicksProvider.GetLabelText(tick),
                                            Brushes.Black, true, 0.2,
                                            new Point3D(tick.AxisValue, 0, _length + 0.5),
                    new Vector3D(0, 0, -1), new Vector3D(-1, 0, 0));

                var ticklabelCln = mayorTickLabel.Clone();
                ticklabelCln.Transform = transformGroup;
                mayorTickLabelGroup.Children.Add(mayorTickLabel);
                mayorTickLabelGroup.Children.Add(ticklabelCln);
            }

            var xTextModel =
                Text3D.CreateTextLabel3D("X Axis", Brushes.Black, true, 0.2,
                    new Point3D(_length / 2, 0, _length + 1.5),
                    new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));

            var xTextModel2 =
                Text3D.CreateTextLabel3D("X Axis", Brushes.Black, true, 0.2,
                    new Point3D(_length / 2, _length + 1.5, 0),
                    new Vector3D(-1, 0, 0), new Vector3D(0, -1, 0));

            axisLabelGroup.Children.Add(xTextModel);
            axisLabelGroup.Children.Add(xTextModel2);
        }

        private void DrawYAxisLabels()
        {
            Transform3D transform = new RotateTransform3D()
            {
                Rotation = new AxisAngleRotation3D() { Axis = new Vector3D(0, 1, 0), Angle = 90 }
            };

            var transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(transform);

            transform = new TranslateTransform3D()
            {
                OffsetX = 0,
                OffsetY = 0,
                OffsetZ = _length * 2 + 1
            };
            transformGroup.Children.Add(transform);

            var yAxisMayTicks = this.TicksProvider.CreateMayorTicks(_yRange, _length);

            for (int i = 1; i < yAxisMayTicks.Length; i++)
            {
                var mayorTickLabel =
                    Text3D.CreateTextLabel3D(this.TicksProvider.GetLabelText(yAxisMayTicks[i]),
                                            Brushes.Black, true, 0.2,
                                            new Point3D(_length + 0.5, yAxisMayTicks[i].AxisValue, 0),
                    new Vector3D(-1, 0, 0), new Vector3D(0, -1, 0));

                var ticklabelCln = mayorTickLabel.Clone();
                ticklabelCln.Transform = transformGroup;
                mayorTickLabelGroup.Children.Add(mayorTickLabel);
                mayorTickLabelGroup.Children.Add(ticklabelCln);
            }

            if (yAxisMayTicks.Length > 0)
            {
                var cornerLable =
                        Text3D.CreateTextLabel3D(this.TicksProvider.GetLabelText(yAxisMayTicks[0]),
                                  Brushes.Black, true, 0.2,
                                  new Point3D(_length + 0.5, yAxisMayTicks[0].AxisValue, 0),
                                  new Vector3D(-1, 0, 0), new Vector3D(0, -1, 0));
                mayorTickLabelGroup.Children.Add(cornerLable);
            }

            var yTextModel =
                Text3D.CreateTextLabel3D("Y Axis", Brushes.Black, true, 0.2,
                    new Point3D(_length + 1.5, _length / 2, 0),
                    new Vector3D(0, 1, 0), new Vector3D(-1, 0, 0));

            var yTextModel2 =
                Text3D.CreateTextLabel3D("Y Axis", Brushes.Black, true, 0.2,
                    new Point3D(0, _length / 2, _length + 1.5),
                    new Vector3D(0, 1, 0), new Vector3D(0, 0, 1));

            axisLabelGroup.Children.Add(yTextModel);
            axisLabelGroup.Children.Add(yTextModel2);
        }

        private void DrawZAxisLabels()
        {
            Transform3D transform = new RotateTransform3D()
            {
                Rotation = new AxisAngleRotation3D() { Axis = new Vector3D(0, 0, 1), Angle = 90 }
            };

            var transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(transform);

            transform = new TranslateTransform3D()
            {
                OffsetX = _length * 2 + 1,
                OffsetY = 0,
                OffsetZ = 0
            };
            transformGroup.Children.Add(transform);

            var zAxisMayTicks = this.TicksProvider.CreateMayorTicks(_zRange, _length);
            for (int i = 1; i < zAxisMayTicks.Length; i++)
            {
                var mayorTickLabel =
                    Text3D.CreateTextLabel3D(this.TicksProvider.GetLabelText(zAxisMayTicks[i]),
                                            Brushes.Black, true, 0.2,
                                            new Point3D(0, _length + 0.5, zAxisMayTicks[i].AxisValue),
                    new Vector3D(0, 1, 0), new Vector3D(0, 0, 1));

                var ticklabelCln = mayorTickLabel.Clone();
                ticklabelCln.Transform = transformGroup;
                mayorTickLabelGroup.Children.Add(mayorTickLabel);
                mayorTickLabelGroup.Children.Add(ticklabelCln);
            }

            var zTextModel =
                Text3D.CreateTextLabel3D("Z Axis", Brushes.Black, true, 0.2,
                    new Point3D(0, _length + 1.5, _length / 2),
                    new Vector3D(0, 0, 1), new Vector3D(0, -1, 0));

            var zTextModel2 =
                Text3D.CreateTextLabel3D("Z Axis", Brushes.Black, true, 0.2,
                    new Point3D(_length + 1.5, 0, _length / 2),
                    new Vector3D(0, 0, 1), new Vector3D(1, 0, 0));

            axisLabelGroup.Children.Add(zTextModel);
            axisLabelGroup.Children.Add(zTextModel2);
        }

        private void AddTransformedPoints(Point3DCollection pntColl, Transform3D transform, Point3D[] points)
        {
            transform.Transform(points);
            pntColl.Add(points);
        }
    }
}
