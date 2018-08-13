﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindableTranslateManipulator.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace HelixToolkit.Wpf
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;

    /// <summary>
    ///   A visual element that contains a manipulator that can translate along an axis.
    /// </summary>
    public class BindableTranslateManipulator : Manipulator
    {
        /// <summary>
        /// Identifies the <see cref="Diameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register(
            "Diameter",
            typeof(double),
            typeof(BindableTranslateManipulator),
            new UIPropertyMetadata(0.2, GeometryChanged));

        /// <summary>
        /// Identifies the <see cref="Direction"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
            "Direction",
            typeof(Vector3D),
            typeof(BindableTranslateManipulator),
            new UIPropertyMetadata(new Vector3D(0, 0, 1), GeometryChanged));

        /// <summary>
        /// Identifies the <see cref="Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register(
            "Length", typeof(double), typeof(BindableTranslateManipulator), new UIPropertyMetadata(2.0, GeometryChanged));

        /// <summary>
        ///   The last point.
        /// </summary>
        private Point3D lastPoint;

        /// <summary>
        ///   Gets or sets the diameter of the manipulator arrow.
        /// </summary>
        /// <value> The diameter. </value>
        public double Diameter
        {
            get
            {
                return (double)this.GetValue(DiameterProperty);
            }

            set
            {
                this.SetValue(DiameterProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets the direction of the translation.
        /// </summary>
        /// <value> The direction. </value>
        public Vector3D Direction
        {
            get
            {
                return (Vector3D)this.GetValue(DirectionProperty);
            }

            set
            {
                this.SetValue(DirectionProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets the length of the manipulator arrow.
        /// </summary>
        /// <value> The length. </value>
        public double Length
        {
            get
            {
                return (double)this.GetValue(LengthProperty);
            }

            set
            {
                this.SetValue(LengthProperty, value);
            }
        }

        /// <summary>
        ///   Called when geometry has been changed.
        /// </summary>
        protected override void OnGeometryChanged()
        {
            var mb = new MeshBuilder(false, false);
            var p0 = new Point3D(0, 0, 0);
            var d = this.Direction;
            d.Normalize();
            var p1 = p0 + (d * this.Length);
            mb.AddArrow(p0, p1, this.Diameter);
            this.Model.Geometry = mb.ToMesh();
        }

        /// <summary>
        /// The on mouse down.
        /// </summary>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            var direction = this.ToWorld(this.Direction);

            var up = Vector3D.CrossProduct(this.Camera.LookDirection, direction);
            var hitPlaneOrigin = this.ToWorld(this.Position);
            this.HitPlaneNormal = Vector3D.CrossProduct(up, direction);
            var p = e.GetPosition(this.ParentViewport);

            var np = this.GetNearestPoint(p, hitPlaneOrigin, this.HitPlaneNormal);
            if (np == null)
            {
                return;
            }

            var lp = this.ToLocal(np.Value);

            this.lastPoint = lp;
            this.CaptureMouse();
        }

        /// <summary>
        /// The on mouse move.
        /// </summary>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.IsMouseCaptured)
            {
                var hitPlaneOrigin = this.ToWorld(this.Position);
                var p = e.GetPosition(this.ParentViewport);
                var nearestPoint = this.GetNearestPoint(p, hitPlaneOrigin, this.HitPlaneNormal);
                if (nearestPoint == null)
                {
                    return;
                }

                var delta = this.ToLocal(nearestPoint.Value) - this.lastPoint;
                this.Value += Vector3D.DotProduct(delta, this.Direction);

                nearestPoint = this.GetNearestPoint(p, hitPlaneOrigin, this.HitPlaneNormal);
                if (nearestPoint != null)
                {
                    this.lastPoint = this.ToLocal(nearestPoint.Value);
                }
            }
        }

        /// <summary>
        /// Updates the TargetTransform in addition to updating position.
        /// </summary>
        /// <param name="e">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnPositionChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPositionChanged(e);
            if (this.TargetTransform != null)
            {
                var delta = (Point3D)e.NewValue - (Point3D)e.OldValue;
                var translateTransform = new TranslateTransform3D(delta);
                this.TargetTransform = Transform3DHelper.CombineTransform(translateTransform, this.TargetTransform);
            }
        }

        /// <summary>
        /// Moves the Manipulator position  by the change in value along the direction vector.
        /// </summary>
        /// <param name="e">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (double)e.OldValue;
            var newValue = (double)e.NewValue;
            var delta = newValue - oldValue;
            var deltaVector = new Vector3D(this.Direction.X, this.Direction.Y, this.Direction.Z);
            deltaVector.Normalize();
            deltaVector = deltaVector * delta;
            this.Position = this.Position + deltaVector;
        }

        /// <summary>
        /// Gets the nearest point on the translation axis.
        /// </summary>
        /// <param name="position">
        /// The position (in screen coordinates).
        /// </param>
        /// <param name="hitPlaneOrigin">
        /// The hit plane origin (world coordinate system).
        /// </param>
        /// <param name="hitPlaneNormal">
        /// The hit plane normal (world coordinate system).
        /// </param>
        /// <returns>
        /// The nearest point (world coordinates) or null if no point could be found.
        /// </returns>
        private Point3D? GetNearestPoint(Point position, Point3D hitPlaneOrigin, Vector3D hitPlaneNormal)
        {
            var hpp = this.GetHitPlanePoint(position, hitPlaneOrigin, hitPlaneNormal);
            if (hpp == null)
            {
                return null;
            }

            var ray = new Ray3D(this.ToWorld(this.Position), this.ToWorld(this.Direction));
            return ray.GetNearest(hpp.Value);
        }
    }
}