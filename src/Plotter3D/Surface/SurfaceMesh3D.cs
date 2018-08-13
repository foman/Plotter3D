using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Plotter3D.Surface
{
    public class SurfaceMesh3D : ModelVisual3D
    {
        public static readonly DependencyProperty TextureBuilderProperty = DependencyProperty.Register("TextureBuilder", typeof(SurfaceTextureBuilder), typeof(SurfaceMesh3D), new UIPropertyMetadata(null, new PropertyChangedCallback(SurfaceMesh3D.GeometryChanged)));

        public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(Point3D[,]), typeof(SurfaceMesh3D), new UIPropertyMetadata(null, new PropertyChangedCallback(SurfaceMesh3D.GeometryChanged)));

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register("Thickness", typeof(double), typeof(SurfaceMesh3D), new UIPropertyMetadata(1.0, new PropertyChangedCallback(SurfaceMesh3D.GeometryChanged)));

        public static readonly DependencyProperty DepthOffsetProperty = DependencyProperty.Register("DepthOffset", typeof(double), typeof(SurfaceMesh3D), new UIPropertyMetadata(0.0, new PropertyChangedCallback(SurfaceMesh3D.GeometryChanged)));

        private GeometryModel3D _model3D;

        private MeshGeometry3D _mesh;

        private Matrix3D _screenToVisual;

        private Matrix3D _visualToScreen;

        private bool _textureAdded = false;

        private bool _isRendering = false;

        private Point3DCollection _linePoints;

        public Point3D[,] Points
        {
            get
            {
                return (Point3D[,])base.GetValue(SurfaceMesh3D.PointsProperty);
            }
            set
            {
                base.SetValue(SurfaceMesh3D.PointsProperty, value);
            }
        }

        public SurfaceTextureBuilder TextureBuilder
        {
            get
            {
                return (SurfaceTextureBuilder)base.GetValue(SurfaceMesh3D.TextureBuilderProperty);
            }
            set
            {
                base.SetValue(SurfaceMesh3D.TextureBuilderProperty, value);
            }
        }

        public double Thickness
        {
            get
            {
                return (double)base.GetValue(SurfaceMesh3D.ThicknessProperty);
            }
            set
            {
                base.SetValue(SurfaceMesh3D.ThicknessProperty, value);
            }
        }

        public double DepthOffset
        {
            get
            {
                return (double)base.GetValue(SurfaceMesh3D.DepthOffsetProperty);
            }
            set
            {
                base.SetValue(SurfaceMesh3D.DepthOffsetProperty, value);
            }
        }

        public SurfaceMesh3D()
        {
            _mesh = new MeshGeometry3D();
            _model3D = new GeometryModel3D();
            _model3D.Geometry = _mesh;
            base.Content = _model3D;
            CompositionTarget.Rendering += new EventHandler(this.OnCompositionTargetRendering);
        }

        protected static void GeometryChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((SurfaceMesh3D)sender).UpdateGeometry();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            _isRendering = (parent != null);
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            if (_isRendering)
            {
                if (this.Points != null && (this.Points.Length != 0 || _mesh.Positions.Count != 0))
                {
                    if (this.UpdateTransforms())
                    {
                        this.UpdateGeometry();
                    }
                }
            }
        }

        private bool UpdateTransforms()
        {
            Matrix3D? transform = Visual3DHelper.GetViewportTransform(this);
            bool result;
            if (!transform.HasValue)
            {
                result = false;
            }
            else
            {
                Matrix3D newTransform = transform.Value;
                if (double.IsNaN(newTransform.M11))
                {
                    result = false;
                }
                else if (!newTransform.HasInverse)
                {
                    result = false;
                }
                else if (newTransform == _visualToScreen)
                {
                    result = false;
                }
                else
                {
                    _visualToScreen = (_screenToVisual = newTransform);
                    _screenToVisual.Invert();
                    result = true;
                }
            }
            return result;
        }

        protected void UpdateGeometry()
        {
            if (this.Points != null && this.Points.Length > 0 && this.TextureBuilder != null)
            {
                this.CreateWirefreamPoints();
                this.AddTexture();
                this.CreateIndices();
                this.CreatePositions(2.0, 0.0);
                this.CreateTextCoordinates();
            }
        }

        protected void CreateWirefreamPoints()
        {
            _linePoints = new Point3DCollection(this.Points.Length);
            int xLength = this.Points.GetLength(0);
            int yLength = this.Points.GetLength(1);
            for (int x = 0; x < xLength; x++)
            {
                for (int y = 0; y < yLength; y++)
                {
                    if (y < yLength - 1)
                    {
                        _linePoints.Add(this.Points[x, y]);
                        _linePoints.Add(this.Points[x, y + 1]);
                    }
                    if (x < xLength - 1)
                    {
                        _linePoints.Add(this.Points[x, y]);
                        _linePoints.Add(this.Points[x + 1, y]);
                    }
                }
            }
            _linePoints.Freeze();
        }

        protected void CreateIndices()
        {
            Int32Collection indices = new Int32Collection(_linePoints.Count * 3);
            for (int i = 0; i < _linePoints.Count / 2; i++)
            {
                indices.Add(i * 4 + 2);
                indices.Add(i * 4 + 1);
                indices.Add(i * 4);
                indices.Add(i * 4 + 2);
                indices.Add(i * 4 + 3);
                indices.Add(i * 4 + 1);
            }
            indices.Freeze();
            _mesh.TriangleIndices = indices;
        }

        protected void CreatePositions(double thickness = 2.0, double depthOffset = 0.0)
        {
            double halfThickness = thickness * 0.5;
            int segmentCount = _linePoints.Count / 2;
            Point3DCollection positions = new Point3DCollection(segmentCount * 4);
            for (int i = 0; i < segmentCount; i++)
            {
                int startIndex = i * 2;
                Point3D startPoint = _linePoints[startIndex];
                Point3D endPoint = _linePoints[startIndex + 1];
                Point4D s0 = (Point4D)startPoint * _visualToScreen;
                Point4D s1 = (Point4D)endPoint * _visualToScreen;
                double lx = s1.X / s1.W - s0.X / s0.W;
                double ly = s1.Y / s1.W - s0.Y / s0.W;
                double j = halfThickness / Math.Sqrt(lx * lx + ly * ly);
                double dx = -ly * j;
                double dy = lx * j;
                Point4D p0 = s0;
                Point4D p1 = s0;
                Point4D p2 = s1;
                Point4D p3 = s1;
                p0.X += dx * p0.W;
                p0.Y += dy * p0.W;
                p1.X -= dx * p1.W;
                p1.Y -= dy * p1.W;
                p2.X += dx * p2.W;
                p2.Y += dy * p2.W;
                p3.X -= dx * p3.W;
                p3.Y -= dy * p3.W;
                if (depthOffset != 0.0)
                {
                    p0.Z -= depthOffset;
                    p1.Z -= depthOffset;
                    p2.Z -= depthOffset;
                    p3.Z -= depthOffset;
                    p0 *= _screenToVisual;
                    p1 *= _screenToVisual;
                    p2 *= _screenToVisual;
                    p3 *= _screenToVisual;
                    positions.Add(new Point3D(p0.X / p0.W, p0.Y / p0.W, p0.Z / p0.W));
                    positions.Add(new Point3D(p1.X / p0.W, p1.Y / p1.W, p1.Z / p1.W));
                    positions.Add(new Point3D(p2.X / p0.W, p2.Y / p2.W, p2.Z / p2.W));
                    positions.Add(new Point3D(p3.X / p0.W, p3.Y / p3.W, p3.Z / p3.W));
                }
                else
                {
                    p0 *= _screenToVisual;
                    p1 *= _screenToVisual;
                    p2 *= _screenToVisual;
                    p3 *= _screenToVisual;
                    positions.Add(new Point3D(p0.X, p0.Y, p0.Z));
                    positions.Add(new Point3D(p1.X, p1.Y, p1.Z));
                    positions.Add(new Point3D(p3.X, p3.Y, p3.Z));
                    positions.Add(new Point3D(p2.X, p2.Y, p2.Z));
                }
            }
            positions.Freeze();
            _mesh.Positions = positions;
        }

        protected void CreateTextCoordinates()
        {
            if (_linePoints != null && this.TextureBuilder != null)
            {
                PointCollection coordsCol = new PointCollection(_linePoints.Count * 2);
                for (int i = 0; i < _linePoints.Count / 2; i++)
                {
                    double j = _linePoints[i * 2].Z;
                    j += _linePoints[i * 2 + 1].Z;
                    j /= 2.0;
                    Point pt = this.TextureBuilder.GetTextureMapping(j);
                    for (int k = 0; k < 4; k++)
                    {
                        coordsCol.Add(pt);
                    }
                }
                coordsCol.Freeze();
                _mesh.TextureCoordinates = coordsCol;
            }
        }

        private void AddTexture()
        {
            if (this.Points.Length > 0 && !_textureAdded && this.TextureBuilder != null)
            {
                _model3D.Material = this.TextureBuilder.CreateTexture();
                _model3D.BackMaterial = _model3D.Material;
                _textureAdded = true;
            }
        }
    }
}
