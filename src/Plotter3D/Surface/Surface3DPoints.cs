using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media;
using Plotter3D.Common;
using HelixToolkit.Wpf;

namespace Plotter3D
{
    /// <summary>
    /// The points graph
    /// </summary>
    public class Surface3DPoints : ModelVisual3D
    {
        public static readonly DependencyProperty TextureBuilderProperty = DependencyProperty.Register("TextureBuilder", typeof(SurfaceTextureBuilder), typeof(Surface3DPoints), new UIPropertyMetadata(null, new PropertyChangedCallback(Surface3DPoints.GeometryChanged)));

        public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(Point3D[,]), typeof(Surface3DPoints), new UIPropertyMetadata(null, new PropertyChangedCallback(Surface3DPoints.GeometryChanged)));

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(Surface3DPoints), new UIPropertyMetadata(2.0, new PropertyChangedCallback(Surface3DPoints.GeometryChanged)));

        private GeometryModel3D _model3D;

        private MeshGeometry3D _mesh;

        private Matrix3D _screenToVisual;

        private Matrix3D _visualToScreen;

        private SurfaceTextureBuilder _textureBuilder;

        private bool _textureAdded = false;

        private bool _isRendering = false;

        public Point3D[,] Points
        {
            get
            {
                return (Point3D[,])base.GetValue(Surface3DPoints.PointsProperty);
            }
            set
            {
                base.SetValue(Surface3DPoints.PointsProperty, value);
            }
        }

        public SurfaceTextureBuilder TextureBuilder
        {
            get
            {
                return (SurfaceTextureBuilder)base.GetValue(Surface3DPoints.TextureBuilderProperty);
            }
            set
            {
                base.SetValue(Surface3DPoints.TextureBuilderProperty, value);
            }
        }

        public double Size
        {
            get
            {
                return (double)base.GetValue(Surface3DPoints.SizeProperty);
            }
            set
            {
                base.SetValue(Surface3DPoints.SizeProperty, value);
            }
        }

        public Surface3DPoints()
        {
            _mesh = new MeshGeometry3D();
            _model3D = new GeometryModel3D();
            _model3D.Geometry = _mesh;
            base.Content = _model3D;
            CompositionTarget.Rendering += new EventHandler(this.OnCompositionTargetRendering);
        }

        protected static void GeometryChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((Surface3DPoints)sender).UpdateGeometry();
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
                this.AddTexture();
                this.CreateIndices();
                this.CreatePositions(0.0);
                this.CreateTextCoordinates();
            }
        }

        protected void CreateIndices()
        {
            Int32Collection indices = new Int32Collection(this.Points.Length * 6);
            for (int i = 0; i < this.Points.Length; i++)
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

        protected void CreatePositions(double depthOffset = 0.0)
        {
            double halfSize = this.Size / 2.0;
            int numPoints = this.Points.Length;
            Vector[] outline = new Vector[]
            {
                new Vector(-halfSize, halfSize),
                new Vector(-halfSize, -halfSize),
                new Vector(halfSize, halfSize),
                new Vector(halfSize, -halfSize)
            };
            Point3DCollection positions = new Point3DCollection(numPoints * 4);
            for (int x = 0; x < this.Points.GetLength(0); x++)
            {
                for (int y = 0; y < this.Points.GetLength(1); y++)
                {
                    Point4D screenPoint = (Point4D)this.Points[x, y] * _visualToScreen;
                    double spx = screenPoint.X;
                    double spy = screenPoint.Y;
                    double spz = screenPoint.Z;
                    double spw = screenPoint.W;
                    if (!depthOffset.Equals(0.0))
                    {
                        spz -= depthOffset * spw;
                    }
                    double pwinverse = 1.0 / (new Point4D(spx, spy, spz, spw) * _screenToVisual).W;
                    Vector[] array = outline;
                    for (int i = 0; i < array.Length; i++)
                    {
                        Vector v = array[i];
                        Point4D p = new Point4D(spx + v.X * spw, spy + v.Y * spw, spz, spw) * _screenToVisual;
                        positions.Add(new Point3D(p.X * pwinverse, p.Y * pwinverse, p.Z * pwinverse));
                    }
                }
            }
            positions.Freeze();
            _mesh.Positions = positions;
        }

        protected void CreateTextCoordinates()
        {
            PointCollection coordsCol = new PointCollection(this.Points.Length);
            for (int x = 0; x < this.Points.GetLength(0); x++)
            {
                for (int y = 0; y < this.Points.GetLength(1); y++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        coordsCol.Add(this.TextureBuilder.GetTextureMapping(this.Points[x, y].Z));
                    }
                }
            }
            coordsCol.Freeze();
            _mesh.TextureCoordinates = coordsCol;
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
