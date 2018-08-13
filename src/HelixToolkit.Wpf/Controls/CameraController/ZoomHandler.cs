﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZoomHandler.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// Handles zooming.
    /// </summary>
    internal class ZoomHandler : MouseGestureHandler
    {
        /// <summary>
        /// The change field of view.
        /// </summary>
        private readonly bool changeFieldOfView;

        /// <summary>
        /// The zoom point.
        /// </summary>
        private Point zoomPoint;

        /// <summary>
        /// The zoom point 3 d.
        /// </summary>
        private Point3D zoomPoint3D;

        private Vector deltaMoved;
        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomHandler"/> class.
        /// </summary>
        /// <param name="controller">
        /// The controller.
        /// </param>
        /// <param name="changeFieldOfView">
        /// if set to <c>true</c> [change field of view].
        /// </param>
        public ZoomHandler(CameraController controller, bool changeFieldOfView = false)
            : base(controller)
        {
            this.changeFieldOfView = changeFieldOfView;
        }

        /// <summary>
        /// Changes the camera position.
        /// </summary>
        /// <param name="delta">
        /// The relative change (0 = no change).
        /// </param>
        /// <param name="zoomAround">
        /// The point to zoom around.
        /// </param>
        public void ZoomByChangingCameraPosition(double delta, Point3D zoomAround)
        {
            if (delta < -0.5)
            {
                delta = -0.5;
            }

            delta *= this.ZoomSensitivity;
            switch (this.CameraMode)
            {
                case CameraMode.Inspect:
                    this.ChangeCameraDistance(delta, zoomAround);
                    break;
                case CameraMode.WalkAround:
                    this.CameraPosition -= this.CameraLookDirection * delta;
                    break;
            }
        }

        public void MoveCameraPosition(Vector3D delta)
        {
            var z = this.CameraLookDirection;
            z.Normalize();
            var x = Vector3D.CrossProduct(this.CameraLookDirection, this.CameraUpDirection);
            var y = Vector3D.CrossProduct(x, z);
            y.Normalize();
            x = Vector3D.CrossProduct(z, y);

            // delta *= this.ZoomSensitivity;
            switch (this.CameraMode)
            {
                case CameraMode.Inspect:
                case CameraMode.WalkAround:
                    this.CameraPosition += x * delta.X + y * delta.Y + z * delta.Z;
                    break;
            }
        }

        /// <summary>
        /// The change camera width.
        /// </summary>
        /// <param name="delta">
        /// The delta.
        /// </param>
        /// <param name="zoomAround">
        /// The zoom around.
        /// </param>
        public void ZoomByChangingCameraWidth(double delta, Point3D zoomAround)
        {
            if (delta < -0.5)
            {
                delta = -0.5;
            }

            switch (this.CameraMode)
            {
                case CameraMode.WalkAround:
                case CameraMode.Inspect:
                case CameraMode.FixedPosition:
                    this.ChangeCameraDistance(delta, zoomAround);

                    // Modify the camera width
                    var ocamera = this.Camera as OrthographicCamera;
                    if (ocamera != null)
                    {
                        ocamera.Width *= 1 + delta;
                    }

                    break;
            }
        }

        /// <summary>
        /// Changes the camera distance.
        /// </summary>
        /// <param name="delta">The delta.</param>
        /// <param name="zoomAround">The zoom around point.</param>
        private void ChangeCameraDistance2(double delta, Point3D zoomAround)
        {
            // Handle the 'zoomAround' point
            var target = this.CameraPosition + this.CameraLookDirection;
            var relativeTarget = zoomAround - target;
            var relativePosition = zoomAround - this.CameraPosition;

            var newRelativePosition = relativePosition * (1 + delta);
            var newRelativeTarget = relativeTarget * (1 + delta);

            var newTarget = zoomAround - newRelativeTarget;
            var newPosition = zoomAround - newRelativePosition;
            var newLookDirection = newTarget - newPosition;

            this.CameraLookDirection = newLookDirection;
            this.CameraPosition = newPosition;
        }


        /// <summary>
        /// Changes the camera distance.
        /// </summary>
        /// <param name="delta">The delta.</param>
        /// <param name="zoomAround">The zoom around point.</param>
        private void ChangeCameraDistance(double delta, Point3D zoomAround)
        {
            // Handle the 'zoomAround' point
            var target = this.CameraPosition + this.CameraLookDirection;
            var relativeTarget = zoomAround - target;

            var newPosition = this.CameraPosition + relativeTarget;
            var relativePosition = zoomAround - newPosition;
            var newRelativePosition = relativePosition * (1 + delta);

            newPosition = zoomAround - newRelativePosition;
            var newLookDirection = zoomAround - newPosition;

            this.CameraLookDirection = newLookDirection;
            this.CameraPosition = newPosition;
        }

        /// <summary>
        /// Changes the field of view and tries to keep the scale fixed.
        /// </summary>
        /// <param name="delta">
        /// The relative change in fov.
        /// </param>
        public void ZoomByChangingFieldOfView(double delta)
        {
            var pcamera = this.Camera as PerspectiveCamera;
            if (pcamera == null)
            {
                return;
            }

            if (!this.Controller.IsChangeFieldOfViewEnabled)
            {
                return;
            }

            double fov = pcamera.FieldOfView;
            double d = this.CameraLookDirection.Length;
            double r = d * Math.Tan(0.5 * fov / 180 * Math.PI);

            fov *= 1 + (delta * 0.5);
            if (fov < this.Controller.MinimumFieldOfView)
            {
                fov = this.Controller.MinimumFieldOfView;
            }

            if (fov > this.Controller.MaximumFieldOfView)
            {
                fov = this.Controller.MaximumFieldOfView;
            }

            pcamera.FieldOfView = fov;
            double d2 = r / Math.Tan(0.5 * fov / 180 * Math.PI);
            Vector3D newLookDirection = this.CameraLookDirection;
            newLookDirection.Normalize();
            newLookDirection *= d2;
            Point3D target = this.CameraPosition + this.CameraLookDirection;
            this.CameraPosition = target - newLookDirection;
            this.CameraLookDirection = newLookDirection;
        }

        /// <summary>
        /// Occurs when the manipulation is completed.
        /// </summary>
        /// <param name="e">The <see cref="ManipulationEventArgs"/> instance containing the event data.</param>
        public override void Completed(ManipulationEventArgs e)
        {
            base.Completed(e);
            this.Controller.HideTargetAdorner();
        }

        /// <summary>
        /// Occurs when the position is changed during a manipulation.
        /// </summary>
        /// <param name="e">The <see cref="ManipulationEventArgs"/> instance containing the event data.</param>
        public override void Delta(ManipulationEventArgs e)
        {
            deltaMoved = e.CurrentPosition - this.LastPoint;
            this.LastPoint = e.CurrentPosition;
            this.Zoom(deltaMoved.Y * 0.01, this.zoomPoint3D);
        }

        protected override void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseUp(sender, e);
            var zoomFactor = 0d;
            if (deltaMoved.X == 0 && deltaMoved.Y == 0)
            {
                if (this.Controller.ZoomIn)
                {
                    zoomFactor = -0.2;
                }
                else
                {
                    zoomFactor = 0.2;
                }

                if (MouseDownNearestPoint3D != null)
                    this.zoomPoint3D = this.MouseDownNearestPoint3D.Value;
            }
            this.Zoom(zoomFactor, this.zoomPoint3D);
        }
        /// <summary>
        /// Occurs when the manipulation is started.
        /// </summary>
        /// <param name="e">The <see cref="ManipulationEventArgs"/> instance containing the event data.</param>
        public override void Started(ManipulationEventArgs e)
        {
            base.Started(e);
            this.zoomPoint = new Point(
                this.Controller.Viewport.ActualWidth / 2, this.Controller.Viewport.ActualHeight / 2);
            this.zoomPoint3D = this.Controller.CameraTarget;

            if (this.Controller.ZoomAroundMouseDownPoint && this.MouseDownNearestPoint3D != null)
            {
                this.zoomPoint = this.MouseDownPoint;
                this.zoomPoint3D = this.MouseDownNearestPoint3D.Value;
            }

            if (!this.changeFieldOfView)
            {
                this.Controller.ShowTargetAdorner(this.zoomPoint);
            }
        }

        /// <summary>
        /// Zooms the view.
        /// </summary>
        /// <param name="delta">
        /// The delta.
        /// </param>
        public void Zoom(double delta)
        {
            this.Zoom(delta, this.CameraTarget);
        }

        /// <summary>
        /// Zooms the view around the specified point.
        /// </summary>
        /// <param name="delta">
        /// The delta.
        /// </param>
        /// <param name="zoomAround">
        /// The zoom around.
        /// </param>
        public void Zoom(double delta, Point3D zoomAround)
        {
            if (!this.Controller.IsZoomEnabled)
            {
                return;
            }

            if (this.Camera is PerspectiveCamera)
            {
                if (this.CameraMode == CameraMode.FixedPosition || this.changeFieldOfView)
                {
                    this.ZoomByChangingFieldOfView(delta);
                }
                else
                {
                    this.ZoomByChangingCameraPosition(delta, zoomAround);
                }

                return;
            }

            if (this.Camera is OrthographicCamera)
            {
                this.ZoomByChangingCameraWidth(delta, zoomAround);
            }
        }

        /// <summary>
        /// Occurs when the command associated with this handler initiates a check to determine whether the command can be executed on the command target.
        /// </summary>
        /// <returns>
        /// True if the execution can continue.
        /// </returns>
        protected override bool CanExecute()
        {
            if (this.changeFieldOfView)
            {
                return this.Controller.IsChangeFieldOfViewEnabled && this.Controller.ActualCamera is PerspectiveCamera;
            }

            return this.Controller.IsZoomEnabled;
        }

        /// <summary>
        /// Gets the cursor for the gesture.
        /// </summary>
        /// <returns>
        /// A cursor.
        /// </returns>
        protected override Cursor GetCursor()
        {
            return this.Controller.ZoomCursor;
        }

    }
}