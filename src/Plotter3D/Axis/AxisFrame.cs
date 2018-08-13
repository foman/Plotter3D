using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace Plotter3D
{

    /// <summary>
    /// Drawing the frame of Plotter without ticks 
    /// </summary>
    public class AxisFrame : RenderingModelVisual3D
    {
        private double _length = 5.0;

        private LinesVisual3D _xozLine;

        private LinesVisual3D _yozLine;

        private LinesVisual3D _xoyLine;

        private bool _isRendering = false;

        private Color _lineColor = Colors.Black;

        public Viewport3D Viewport
        {
            get;
            set;
        }

        public double AxisLength
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
            }
        }

        public AxisFrame()
        {
            this.CreateFrame();
            base.SubscribeToRenderingEvent();
        }

        private void CreateFrame()
        {
            _xozLine = new LinesVisual3D
            {
                Color = _lineColor,
                Thickness = 1.0
            };
            _xozLine.Points = new Point3DCollection();
            Vector3D[] vecs = new Vector3D[]
            {
                new Vector3D(_length, 0.0, 0.0),
                new Vector3D(0.0, 0.0, _length),
                new Vector3D(-_length, 0.0, 0.0),
                new Vector3D(0.0, 0.0, -_length)
            };
            Point3D p = new Point3D(0.0, 0.0, 0.0);
            Vector3D[] array = vecs;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3D v = array[i];
                _xozLine.Points.Add(p);
                p += v;
                _xozLine.Points.Add(p);
            }
            _yozLine = new LinesVisual3D
            {
                Color = _lineColor,
                Thickness = 1.0
            };
            _yozLine.Points = new Point3DCollection();
            vecs = new Vector3D[]
            {
                new Vector3D(0.0, _length, 0.0),
                new Vector3D(0.0, 0.0, _length),
                new Vector3D(0.0, -_length, 0.0),
                new Vector3D(0.0, 0.0, -_length)
            };
            p = new Point3D(0.0, 0.0, 0.0);
            array = vecs;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3D v = array[i];
                _yozLine.Points.Add(p);
                p += v;
                _yozLine.Points.Add(p);
            }
            _xoyLine = new LinesVisual3D
            {
                Color = _lineColor,
                Thickness = 1.0
            };
            _xoyLine.Points = new Point3DCollection();
            vecs = new Vector3D[]
            {
                new Vector3D(0.0, _length, 0.0),
                new Vector3D(_length, 0.0, 0.0),
                new Vector3D(0.0, -_length, 0.0),
                new Vector3D(-_length, 0.0, 0.0)
            };
            p = new Point3D(0.0, 0.0, 0.0);
            array = vecs;
            for (int i = 0; i < array.Length; i++)
            {
                Vector3D v = array[i];
                _xoyLine.Points.Add(p);
                p += v;
                _xoyLine.Points.Add(p);
            }
            base.Children.Add(_xozLine);
            base.Children.Add(_yozLine);
            base.Children.Add(_xoyLine);
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
                if (this.Viewport != null)
                {
                    ProjectionCamera camera = this.Viewport.Camera as ProjectionCamera;
                    Vector3D xAixs = new Vector3D(1.0, 0.0, 0.0);
                    if ((camera.LookDirection - xAixs).Y > 0.0)
                    {
                        //The XOZ should be moved in YAxis direction by offset (0,Axis.Length,0).
                        _xozLine.Transform = new TranslateTransform3D(new Vector3D(0.0, _length, 0.0));
                    }
                    else
                    {
                        _xozLine.Transform = Transform3D.Identity;
                    }
                    Vector3D yAxis = new Vector3D(0.0, 1.0, 0.0);
                    Vector3D vec = camera.LookDirection - yAxis;
                    if (vec.X > 0.0)
                    {
                        _yozLine.Transform = new TranslateTransform3D(new Vector3D(_length, 0.0, 0.0));
                    }
                    else
                    {
                        _yozLine.Transform = Transform3D.Identity;
                    }
                    if (vec.Z > 0.0)
                    {
                        _xoyLine.Transform = new TranslateTransform3D(new Vector3D(0.0, 0.0, _length));
                    }
                    else
                    {
                        _xoyLine.Transform = Transform3D.Identity;
                    }
                }
            }
        }
    }
}
