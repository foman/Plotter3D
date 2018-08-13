using HelixToolkit.Wpf;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Plotter3D.Axis
{
    public class TickLabelVisual3D : RenderingModelVisual3D
    {
        private Model3D _model3D;

        private Point3D _locationPt;

        private Location _location;

        private Point3D _center;

        private Vector3D _textDirection;

        private Vector3D _up;

        private bool _doubleSide = false;

        private bool _isRendering = false;

        private string _text = string.Empty;

        private double _screenHeight = 1.0;

        private Viewport3D _viewport;

        private Vector3D zeroVec = new Vector3D(0.0, 0.0, 0.0);

        private Point3D _leftUpPoint;

        private Point3D _LeftLowPoint;

        private DiffuseMaterial mat;

        private Matrix3D _screenToVisual;

        private Matrix3D _visualToScreen;

        public double ModelWidth
        {
            get;
            private set;
        }

        public double ModelHeight
        {
            get;
            private set;
        }

        public double ScreenHeight
        {
            get
            {
                return _screenHeight;
            }
            private set
            {
                _screenHeight = value;
            }
        }

        public double ScreenWidth
        {
            get;
            private set;
        }

        public TickLabelVisual3D(string text, Brush textColor, bool bDoubleSided, double modelHeight, Point3D locationPoint, Location location, Vector3D textDirection, Vector3D up)
        {
            _up = up;
            _textDirection = textDirection;
            _doubleSide = bDoubleSided;
            FormattedText ft = new FormattedText(text, new CultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, default(FontStretch)), 20.0, textColor, null, TextFormattingMode.Display);
            DrawingVisual ftVisual = new DrawingVisual();
            DrawingContext drawingContext = ftVisual.RenderOpen();
            drawingContext.DrawText(ft, new Point(1.0, 1.0));
            drawingContext.Close();
            _text = text;
            _screenHeight = 12.0;
            this.ScreenWidth = (double)text.Length * _screenHeight;
            double width = (double)text.Length * modelHeight;
            this.ModelHeight = modelHeight;
            this.ModelWidth = width;
            _locationPt = locationPoint;
            _location = location;
            _center = locationPoint;
            switch (location)
            {
                case Location.Start:
                    _center += textDirection * width / 2.0;
                    break;
                case Location.End:
                    _center -= textDirection * width / 2.0;
                    break;
            }
            Rect rect = ft.BuildHighlightGeometry(new Point(0.0, 0.0)).Bounds;
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)Math.Ceiling(rect.Width) + 1, (int)Math.Ceiling(rect.Height) + 1, 96.0, 96.0, PixelFormats.Pbgra32);
            bitmap.Render(ftVisual);
            bitmap.Freeze();
            this.mat = new DiffuseMaterial();
            ImageBrush imageBrush = new ImageBrush(bitmap);
            imageBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            this.mat.Brush = imageBrush;
            base.SubscribeToRenderingEvent();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            _isRendering = (parent != null);
        }

        protected override void OnCompositionTargetRendering(object sender, RenderingEventArgs eventArgs)
        {
            if (_isRendering)
            {
                if (_viewport == null)
                {
                    _viewport = Visual3DHelper.GetViewport3D(this);
                    if (_viewport == null)
                    {
                        return;
                    }
                }
                if (this.UpdateTransforms())
                {
                    this.UpdateMode3D();
                    this.AdjustTextDirection();
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

        private void UpdateMode3D()
        {
            ProjectionCamera camera = _viewport.Camera as ProjectionCamera;
            camera.UpDirection.Normalize();
            Point4D p = (Point4D)_center * _visualToScreen;
            p.Y += _screenHeight * p.W;
            p *= _screenToVisual;
            double height = (_center - new Point3D(p.X / p.W, p.Y / p.W, p.Z / p.W)).Length;
            double width = (double)_text.Length * height;
            this.ModelHeight = height;
            this.ModelWidth = width;
            Point3D p2 = _center - width / 2.0 * _textDirection - height / 2.0 * _up;
            Point3D p3 = p2 + _up * 1.0 * height;
            Point3D p4 = p2 + _textDirection * width;
            Point3D p5 = p2 + _up * 1.0 * height + _textDirection * width;
            MeshGeometry3D mg = new MeshGeometry3D();
            mg.Positions = new Point3DCollection();
            mg.Positions.Add(p2);
            mg.Positions.Add(p3);
            mg.Positions.Add(p4);
            mg.Positions.Add(p5);
            if (_doubleSide)
            {
                mg.Positions.Add(p2);
                mg.Positions.Add(p3);
                mg.Positions.Add(p4);
                mg.Positions.Add(p5);
            }
            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(3);
            mg.TriangleIndices.Add(1);
            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(2);
            mg.TriangleIndices.Add(3);
            if (_doubleSide)
            {
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(5);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(6);
            }
            mg.TextureCoordinates.Add(new Point(0.0, 1.0));
            mg.TextureCoordinates.Add(new Point(0.0, 0.0));
            mg.TextureCoordinates.Add(new Point(1.0, 1.0));
            mg.TextureCoordinates.Add(new Point(1.0, 0.0));
            if (_doubleSide)
            {
                mg.TextureCoordinates.Add(new Point(1.0, 1.0));
                mg.TextureCoordinates.Add(new Point(1.0, 0.0));
                mg.TextureCoordinates.Add(new Point(0.0, 1.0));
                mg.TextureCoordinates.Add(new Point(0.0, 0.0));
            }
            _model3D = new GeometryModel3D(mg, this.mat);
            base.Content = _model3D;
        }

        private void AdjustTextDirection()
        {
            ProjectionCamera camera = _viewport.Camera as ProjectionCamera;
            Vector3D upVec = camera.UpDirection;
            upVec.Normalize();
            Vector3D horVec = Vector3D.CrossProduct(upVec, camera.LookDirection);
            horVec.Normalize();
            Vector3D axis = Vector3D.CrossProduct(_up, upVec);
            Vector3D axis2 = Vector3D.CrossProduct(_textDirection, horVec);
            double degree = Vector3D.AngleBetween(_up, upVec);
            double degree2 = Vector3D.AngleBetween(_textDirection, horVec);
            QuaternionRotation3D qr = new QuaternionRotation3D();
            if (axis != this.zeroVec && axis2 != this.zeroVec)
            {
                Quaternion q = new Quaternion(axis, degree);
                Quaternion q2 = new Quaternion(axis2, degree2);
                qr.Quaternion = q * q2;
            }
            else if (axis == this.zeroVec && axis2 != this.zeroVec)
            {
                Quaternion q2 = new Quaternion(axis2, degree2);
                qr.Quaternion = q2;
            }
            else
            {
                if (!(axis != this.zeroVec) || !(axis2 == this.zeroVec))
                {
                    return;
                }
                Quaternion q = new Quaternion(axis, degree);
                qr.Quaternion = q;
            }
            Transform3D transform = new RotateTransform3D(qr, _center);
            _model3D.Transform = transform;
        }
    }
}
