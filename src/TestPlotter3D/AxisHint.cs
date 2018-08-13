using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelixToolkit.Wpf;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Plotter3D;

namespace Plotter3DDemo
{

    /// <summary>
    /// Drawing the frame of Plotter without ticks 
    /// </summary>
    public class AxisHint : RenderingModelVisual3D
    {
        private double _length = 5;
        private LinesVisual3D _xozLine;
        private LinesVisual3D _yozLine;
        private LinesVisual3D _xoyLine;
        private bool _isRendering = false;
        private Color _lineColor = Colors.Gray;
        private Model3D _model3D = new GeometryModel3D();
        LinesVisual3D uPline;
        LinesVisual3D lookline;
        private Vector3D _up;
        private Vector3D _horDirection;
        private Vector3D zeroVec = new Vector3D(0, 0, 0);
        private Point3D _center = new Point3D(0, 0, 0);

        public AxisHint()
        {
            CreateFrame();
            SubscribeToRenderingEvent();
            //RenderOptions.SetEdgeMode((DependencyObject)this, EdgeMode.Aliased);
        }

        public Viewport3D Viewport
        {
            get;
            set;
        }

        public double AxisLength
        {
            get { return _length; }
            set { _length = value; }
        }

        private void CreateFrame()
        {
            _up = new Vector3D(0, 0, 1);
            _horDirection = new Vector3D(1, 0, 0);

            //_model3D =
            //        Text3D.CreateTextLabel3D(
            //            "000",
            //            Brushes.Black, true, 0.2,
            //            new Point3D(0, 0, 0), Location.Center,
            //            _horDirection, _up);
            //this.Content = _model3D;

            uPline = new LinesVisual3D();
            uPline.Points.Add(new Point3D(0, 0, 0));
            uPline.Points.Add(new Point3D(0, 0, 1));
            uPline.Thickness = 2;
            uPline.Color = Colors.Blue;

            lookline = new LinesVisual3D();
            lookline.Points.Add(new Point3D(0, 0, 0));
            lookline.Points.Add(new Point3D(0, 0, 0) + _horDirection);
            lookline.Thickness = 2;
            lookline.Color = Colors.Blue;

            this.Children.Add(uPline);
            this.Children.Add(lookline);
        }

        protected override void OnVisualParentChanged(System.Windows.DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            var parent = VisualTreeHelper.GetParent(this);
            _isRendering = parent != null;
        }
        private Vector3D _cUp = new Vector3D(0, 0, 0);
        private Vector3D _cLook = new Vector3D(0, 0, 0);
        protected override void OnCompositionTargetRendering(object sender, RenderingEventArgs eventArgs)
        {
            if (!_isRendering)
                return;
            if (Viewport == null)
                return;
            if (CamereChanged)
            {
                AdjustTextDirection();
            }

        }

        public bool CamereChanged
        {
            get
            {
                var camera = Viewport.Camera as ProjectionCamera;
                if (_cUp == camera.UpDirection &&
                    _cLook == camera.LookDirection)
                    return false;
                _cUp = camera.UpDirection;
                _cLook = camera.LookDirection;
                return true;
            }
        }


        private void AdjustTextDirection()
        {
            var camera = Viewport.Camera as ProjectionCamera;

            var upVec = camera.UpDirection;
            upVec.Normalize();
            var horVec = Vector3D.CrossProduct(_up, camera.LookDirection);
            horVec.Normalize();

            ////Axis for rotate to up direction
            var axis1 = Vector3D.CrossProduct(_up, upVec);
            //Axis for rotate from textDirection to horVec.
            var axis2 = Vector3D.CrossProduct(_horDirection, horVec);

            var degree1 = Vector3D.AngleBetween(_up, upVec);
            var degree2 = Vector3D.AngleBetween(_horDirection, horVec);

            AxisAngleRotation3D aar1 = new AxisAngleRotation3D(axis1, degree1);
            AxisAngleRotation3D aar2 = new AxisAngleRotation3D(axis2, degree2);

            QuaternionRotation3D qr = new QuaternionRotation3D();

            if (axis1 != zeroVec && axis2 != zeroVec)
            {
                Quaternion q1 = new Quaternion(axis1, degree1);
                Quaternion q2 = new Quaternion(axis2, degree2);
                qr.Quaternion = q1 * q2;
            }
            else if (axis1 == zeroVec && axis2 != zeroVec)
            {
                Quaternion q2 = new Quaternion(axis2, degree2);
                qr.Quaternion = q2;
            }
            else if (axis1 != zeroVec && axis2 == zeroVec)
            {
                Quaternion q1 = new Quaternion(axis1, degree1);
                qr.Quaternion = q1;
            }
            else
            {
                return;
            }

            Transform3D transform = new RotateTransform3D(qr, _center);

            _model3D.Transform = transform;



            //Axis for rotate to up direction
            axis1 = Vector3D.CrossProduct(_up, upVec);
            //Axis for rotate from textDirection to horVec.
            axis2 = Vector3D.CrossProduct(_horDirection, horVec);
            degree1 = Vector3D.AngleBetween(_up, upVec);
            degree2 = Vector3D.AngleBetween(_horDirection, horVec);

            Transform3DGroup tg = new Transform3DGroup();
            if (axis1 != zeroVec)
            {
                Quaternion q1 = new Quaternion(axis1, degree1);
                QuaternionRotation3D qr1 = new QuaternionRotation3D(q1);
                Transform3D transform1 = new RotateTransform3D(qr1, _center);
                tg.Children.Add(transform1);
                uPline.Transform = transform1;
            }

            if (axis2 != zeroVec)
            {
                Quaternion q2 = new Quaternion(axis2, degree2);
                QuaternionRotation3D qr1 = new QuaternionRotation3D(q2);
                Transform3D transform1 = new RotateTransform3D(qr1, _center);
                tg.Children.Add(transform1);
                lookline.Transform = transform1;
            }
            //this.Transform = tg;
        }
    }
}
