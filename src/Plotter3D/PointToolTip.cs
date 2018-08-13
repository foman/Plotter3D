using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Plotter3D
{
    /// <summary>
    ///Showing the point data info
    /// </summary>
    public class PointToolTip : Control
    {
        private Point3D _point;
        private TextBlock _tb;

        static PointToolTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PointToolTip), new FrameworkPropertyMetadata(typeof(PointToolTip)));
        }

        public PointToolTip()
        {
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        public override void OnApplyTemplate()
        {
            _tb = this.Template.FindName("Part_TB", this) as TextBlock;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Update();
        }

        public Point3D Point
        {
            get { return _point; }
            set
            {
                _point = value;
                _tb.Text = string.Format("X: {0}\nY: {1}\nZ: {2}",
                    _point.X.ToString(), _point.Y.ToString(), _point.Z.ToString());
            }
        }

        /// <summary>
        /// The visual which point Attached to
        /// </summary>
        public Visual3D AttachedVisual
        {
            get;
            set;
        }

        private void Update()
        {
            if (this.Visibility != System.Windows.Visibility.Visible)
                return;

            if (AttachedVisual == null)
                return;

            var transform = Visual3DHelper.GetViewportTransform(AttachedVisual);
            if (!transform.HasValue)
                return;

            var m = transform.Value;
            var screenPt = m.Transform(Point);
            this.Margin = new Thickness(screenPt.X + 1, screenPt.Y - 1 - this.ActualHeight, 0, 0);
        }
    }
}
