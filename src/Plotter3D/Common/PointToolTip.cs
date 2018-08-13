using HelixToolkit.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Plotter3D.Common
{
    internal class PointToolTip : Control
    {
        private Point3D _point;

        private TextBlock _tb;

        public Point3D Point
        {
            get
            {
                return this._point;
            }
            set
            {
                this._point = value;
                this._tb.Text = string.Format("X: {0}\nY: {1}\nZ: {2}", this._point.X.ToString(), this._point.Y.ToString(), this._point.Z.ToString());
            }
        }

        public Visual3D AttachedVisual
        {
            get;
            set;
        }

        static PointToolTip()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PointToolTip), new FrameworkPropertyMetadata(typeof(PointToolTip)));
        }

        public PointToolTip()
        {
            base.HorizontalAlignment = HorizontalAlignment.Left;
            base.VerticalAlignment = VerticalAlignment.Top;
            CompositionTarget.Rendering += new EventHandler(this.CompositionTarget_Rendering);
        }

        public override void OnApplyTemplate()
        {
            this._tb = (base.Template.FindName("Part_TB", this) as TextBlock);
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            this.Update();
        }

        private void Update()
        {
            if (base.Visibility == Visibility.Visible)
            {
                if (this.AttachedVisual != null)
                {
                    Matrix3D? transform = Visual3DHelper.GetViewportTransform(this.AttachedVisual);
                    if (transform.HasValue)
                    {
                        Point3D screenPt = transform.Value.Transform(this.Point);
                        base.Margin = new Thickness(screenPt.X + 1.0, screenPt.Y - 1.0 - base.ActualHeight, 0.0, 0.0);
                    }
                }
            }
        }
    }
}
