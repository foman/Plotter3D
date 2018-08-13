﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraController.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// Provides a control that manipulates the camera by mouse and keyboard gestures.
    /// </summary>
    public class CameraController : Grid
    {
        /// <summary>
        /// Identifies the <see cref="CameraMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraModeProperty = DependencyProperty.Register(
            "CameraMode", typeof(CameraMode), typeof(CameraController), new UIPropertyMetadata(CameraMode.Inspect));

        /// <summary>
        /// Identifies the <see cref="Camera"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraProperty = DependencyProperty.Register(
            "Camera",
            typeof(ProjectionCamera),
            typeof(CameraController),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, CameraChanged));

        /// <summary>
        /// Identifies the <see cref="CameraRotationMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraRotationModeProperty =
            DependencyProperty.Register(
                "CameraRotationMode",
                typeof(CameraRotationMode),
                typeof(CameraController),
                new UIPropertyMetadata(CameraRotationMode.Turntable));

        /// <summary>
        /// Identifies the <see cref="ChangeFieldOfViewCursor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChangeFieldOfViewCursorProperty =
            DependencyProperty.Register(
                "ChangeFieldOfViewCursor",
                typeof(Cursor),
                typeof(CameraController),
                new UIPropertyMetadata(Cursors.ScrollNS));

        /// <summary>
        /// Identifies the <see cref="DefaultCamera"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultCameraProperty = DependencyProperty.Register(
            "DefaultCamera", typeof(ProjectionCamera), typeof(CameraController), new UIPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Enabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.Register(
            "Enabled", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="InertiaFactor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InertiaFactorProperty = DependencyProperty.Register(
            "InertiaFactor", typeof(double), typeof(CameraController), new UIPropertyMetadata(0.9));

        /// <summary>
        /// Identifies the <see cref="InfiniteSpin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InfiniteSpinProperty = DependencyProperty.Register(
            "InfiniteSpin", typeof(bool), typeof(CameraController), new UIPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="IsChangeFieldOfViewEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsChangeFieldOfViewEnabledProperty =
            DependencyProperty.Register(
                "IsChangeFieldOfViewEnabled", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsInertiaEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsInertiaEnabledProperty =
            DependencyProperty.Register(
                "IsInertiaEnabled", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsMoveEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsMoveEnabledProperty = DependencyProperty.Register(
            "IsMoveEnabled", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsPanEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsPanEnabledProperty = DependencyProperty.Register(
            "IsPanEnabled", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsRotationEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsRotationEnabledProperty =
            DependencyProperty.Register(
                "IsRotationEnabled", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsTouchZoomEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsTouchZoomEnabledProperty =
            DependencyProperty.Register(
                "IsTouchZoomEnabled", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsZoomEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsZoomEnabledProperty = DependencyProperty.Register(
            "IsZoomEnabled", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="LeftRightPanSensitivity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LeftRightPanSensitivityProperty =
            DependencyProperty.Register(
                "LeftRightPanSensitivity", typeof(double), typeof(CameraController), new UIPropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="LeftRightRotationSensitivity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LeftRightRotationSensitivityProperty =
            DependencyProperty.Register(
                "LeftRightRotationSensitivity", typeof(double), typeof(CameraController), new UIPropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="MaximumFieldOfView"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumFieldOfViewProperty =
            DependencyProperty.Register(
                "MaximumFieldOfView", typeof(double), typeof(CameraController), new UIPropertyMetadata(160.0));

        /// <summary>
        /// Identifies the <see cref="MinimumFieldOfView"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumFieldOfViewProperty =
            DependencyProperty.Register(
                "MinimumFieldOfView", typeof(double), typeof(CameraController), new UIPropertyMetadata(5.0));

        /// <summary>
        /// Identifies the <see cref="MoveSensitivity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MoveSensitivityProperty =
            DependencyProperty.Register(
                "MoveSensitivity", typeof(double), typeof(CameraController), new UIPropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="PageUpDownZoomSensitivity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PageUpDownZoomSensitivityProperty =
            DependencyProperty.Register(
                "PageUpDownZoomSensitivity", typeof(double), typeof(CameraController), new UIPropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="PanCursor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PanCursorProperty = DependencyProperty.Register(
            "PanCursor", typeof(Cursor), typeof(CameraController), new UIPropertyMetadata(Cursors.Hand));

        /// <summary>
        /// Identifies the <see cref="RotateAroundMouseDownPoint"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RotateAroundMouseDownPointProperty =
            DependencyProperty.Register(
                "RotateAroundMouseDownPoint", typeof(bool), typeof(CameraController), new UIPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="RotateCursor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RotateCursorProperty = DependencyProperty.Register(
            "RotateCursor", typeof(Cursor), typeof(CameraController), new UIPropertyMetadata(Cursors.SizeAll));

        /// <summary>
        /// Identifies the <see cref="RotationSensitivity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RotationSensitivityProperty =
            DependencyProperty.Register(
                "RotationSensitivity", typeof(double), typeof(CameraController), new UIPropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="ShowCameraTarget"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowCameraTargetProperty =
            DependencyProperty.Register(
                "ShowCameraTarget", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="SpinReleaseTime"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SpinReleaseTimeProperty =
            DependencyProperty.Register(
                "SpinReleaseTime", typeof(int), typeof(CameraController), new UIPropertyMetadata(200));

        /// <summary>
        /// Identifies the <see cref="TouchMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TouchModeProperty = DependencyProperty.Register(
            "TouchMode", typeof(TouchMode), typeof(CameraController), new UIPropertyMetadata(TouchMode.Panning));

        /// <summary>
        /// Identifies the <see cref="ModelUpDirection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModelUpDirectionProperty = DependencyProperty.Register(
            "ModelUpDirection",
            typeof(Vector3D),
            typeof(CameraController),
            new UIPropertyMetadata(new Vector3D(0, 0, 1)));

        /// <summary>
        /// Identifies the <see cref="UpDownPanSensitivity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UpDownPanSensitivityProperty =
            DependencyProperty.Register(
                "UpDownPanSensitivity", typeof(double), typeof(CameraController), new UIPropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="UpDownRotationSensitivity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UpDownRotationSensitivityProperty =
            DependencyProperty.Register(
                "UpDownRotationSensitivity", typeof(double), typeof(CameraController), new UIPropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="Viewport"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewportProperty = DependencyProperty.Register(
            "Viewport", typeof(Viewport3D), typeof(CameraController), new PropertyMetadata(null, ViewportChanged));

        /// <summary>
        /// Identifies the <see cref="ZoomAroundMouseDownPoint"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomAroundMouseDownPointProperty =
            DependencyProperty.Register(
                "ZoomAroundMouseDownPoint", typeof(bool), typeof(CameraController), new UIPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="ZoomCursor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomCursorProperty = DependencyProperty.Register(
            "ZoomCursor", typeof(Cursor), typeof(CameraController), new UIPropertyMetadata(Cursors.SizeNS));

        /// <summary>
        /// Identifies the <see cref="ZoomRectangleCursor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomRectangleCursorProperty =
            DependencyProperty.Register(
                "ZoomRectangleCursor",
                typeof(Cursor),
                typeof(CameraController),
                new UIPropertyMetadata(Cursors.ScrollSE));

        /// <summary>
        /// Identifies the <see cref="ZoomSensitivity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomSensitivityProperty =
            DependencyProperty.Register(
                "ZoomSensitivity", typeof(double), typeof(CameraController), new UIPropertyMetadata(1.0));

        /// <summary>
        /// The back view command.
        /// </summary>
        public static RoutedCommand BackViewCommand = new RoutedCommand();

        /// <summary>
        /// The bottom view command.
        /// </summary>
        public static RoutedCommand BottomViewCommand = new RoutedCommand();

        /// <summary>
        /// The change fov command.
        /// </summary>
        public static RoutedCommand ChangeFieldOfViewCommand = new RoutedCommand();

        /// <summary>
        /// The change look at command.
        /// </summary>
        public static RoutedCommand ChangeLookAtCommand = new RoutedCommand();

        /// <summary>
        /// The front view command.
        /// </summary>
        public static RoutedCommand FrontViewCommand = new RoutedCommand();

        /// <summary>
        /// The left view command.
        /// </summary>
        public static RoutedCommand LeftViewCommand = new RoutedCommand();

        /// <summary>
        /// The pan command.
        /// </summary>
        public static RoutedCommand PanCommand = new RoutedCommand();

        /// <summary>
        /// The reset camera command.
        /// </summary>
        public static RoutedCommand ResetCameraCommand = new RoutedCommand();

        /// <summary>
        /// The right view command.
        /// </summary>
        public static RoutedCommand RightViewCommand = new RoutedCommand();

        /// <summary>
        /// The rotate command.
        /// </summary>
        public static RoutedCommand RotateCommand = new RoutedCommand();

        /// <summary>
        /// The top view command.
        /// </summary>
        public static RoutedCommand TopViewCommand = new RoutedCommand();

        /// <summary>
        /// The zoom command.
        /// </summary>
        public static RoutedCommand ZoomCommand = new RoutedCommand();

        /// <summary>
        /// The zoom extents command.
        /// </summary>
        public static RoutedCommand ZoomExtentsCommand = new RoutedCommand();

        /// <summary>
        /// The zoom rectangle command.
        /// </summary>
        public static RoutedCommand ZoomRectangleCommand = new RoutedCommand();

        /// <summary>
        /// The camera history stack.
        /// </summary>
        /// <remarks>
        /// Implemented as a list since we want to remove items at the bottom of the stack.
        /// </remarks>
        private readonly List<CameraSetting> cameraHistory = new List<CameraSetting>();

        /// <summary>
        /// The rendering event listener.
        /// </summary>
        private readonly RenderingEventListener renderingEventListener;

        /// <summary>
        /// The change field of view event handler.
        /// </summary>
        private ZoomHandler changeFieldOfViewHandler;

        /// <summary>
        /// The change look at event handler.
        /// </summary>
        private RotateHandler changeLookAtHandler;

        /// <summary>
        /// The is spinning flag.
        /// </summary>
        private bool isSpinning;

        /// <summary>
        /// The last tick.
        /// </summary>
        private long lastTick;

        /// <summary>
        /// The move speed.
        /// </summary>
        private Vector3D moveSpeed;

        /// <summary>
        /// The pan event handler.
        /// </summary>
        private PanHandler panHandler;

        /// <summary>
        /// The pan speed.
        /// </summary>
        private Vector3D panSpeed;

        /// <summary>
        /// The rectangle adorner.
        /// </summary>
        private RectangleAdorner rectangleAdorner;

        /// <summary>
        /// The rotation event handler.
        /// </summary>
        private RotateHandler rotateHandler;

        /// <summary>
        /// The 3D rotation point.
        /// </summary>
        private Point3D rotationPoint3D;

        /// <summary>
        /// The rotation position.
        /// </summary>
        private Point rotationPosition;

        /// <summary>
        /// The rotation speed.
        /// </summary>
        private Vector rotationSpeed;

        /// <summary>
        /// The 3D point to spin around.
        /// </summary>
        private Point3D spinningPoint3D;

        /// <summary>
        /// The spinning position.
        /// </summary>
        private Point spinningPosition;

        /// <summary>
        /// The spinning speed.
        /// </summary>
        private Vector spinningSpeed;

        /// <summary>
        /// The target adorner.
        /// </summary>
        private Adorner targetAdorner;

        /// <summary>
        /// The touch down point.
        /// </summary>
        private Point touchDownPoint;

        /// <summary>
        /// The zoom event handler.
        /// </summary>
        private ZoomHandler zoomHandler;

        /// <summary>
        /// The point to zoom around.
        /// </summary>
        private Point3D zoomPoint3D;

        /// <summary>
        /// The zoom rectangle event handler.
        /// </summary>
        private ZoomRectangleHandler zoomRectangleHandler;

        /// <summary>
        /// The zoom speed.
        /// </summary>
        private double zoomSpeed;


        /// <summary>
        /// Initializes static members of the <see cref="CameraController" /> class.
        /// Initializes static members of the <see cref="CameraController" /> class. Initializes static members of the
        ///     <see
        ///         cref="CameraController" />
        /// class. Initializes static members of the <see cref="CameraController" /> class. Initializes static members of the
        ///     <see
        ///         cref="CameraController" />
        /// class.
        /// </summary>
        static CameraController()
        {
            BackgroundProperty.OverrideMetadata(
                typeof(CameraController), new FrameworkPropertyMetadata(Brushes.Transparent));
            FocusVisualStyleProperty.OverrideMetadata(typeof(CameraController), new FrameworkPropertyMetadata(null));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraController" /> class.
        /// </summary>
        public CameraController()
        {
            this.Loaded += this.CameraControllerLoaded;
            this.Unloaded += this.CameraControllerUnloaded;

            // Must be focusable to received key events
            this.Focusable = true;
            this.FocusVisualStyle = null;

            this.IsManipulationEnabled = true;

            this.InitializeBindings();
            this.renderingEventListener = new RenderingEventListener(this.OnCompositionTargetRendering);
        }

        /// <summary>
        /// Gets ActualCamera.
        /// </summary>
        public ProjectionCamera ActualCamera
        {
            get
            {
                if (this.Camera != null)
                {
                    return this.Camera;
                }

                if (this.Viewport != null)
                {
                    return this.Viewport.Camera as ProjectionCamera;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets Camera.
        /// </summary>
        public ProjectionCamera Camera
        {
            get
            {
                return (ProjectionCamera)this.GetValue(CameraProperty);
            }

            set
            {
                this.SetValue(CameraProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets CameraLookDirection.
        /// </summary>
        public Vector3D CameraLookDirection
        {
            get
            {
                return this.ActualCamera.LookDirection;
            }

            set
            {
                this.ActualCamera.LookDirection = value;
            }
        }

        /// <summary>
        /// Gets or sets CameraMode.
        /// </summary>
        public CameraMode CameraMode
        {
            get
            {
                return (CameraMode)this.GetValue(CameraModeProperty);
            }

            set
            {
                this.SetValue(CameraModeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets CameraPosition.
        /// </summary>
        public Point3D CameraPosition
        {
            get
            {
                return this.ActualCamera.Position;
            }

            set
            {
                this.ActualCamera.Position = value;
            }
        }

        /// <summary>
        /// Gets or sets CameraRotationMode.
        /// </summary>
        public CameraRotationMode CameraRotationMode
        {
            get
            {
                return (CameraRotationMode)this.GetValue(CameraRotationModeProperty);
            }

            set
            {
                this.SetValue(CameraRotationModeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets CameraTarget.
        /// </summary>
        public Point3D CameraTarget
        {
            get
            {
                return this.CameraPosition + this.CameraLookDirection;
            }

            set
            {
                this.CameraLookDirection = value - this.CameraPosition;
            }
        }

        /// <summary>
        /// Gets or sets CameraUpDirection.
        /// </summary>
        public Vector3D CameraUpDirection
        {
            get
            {
                return this.ActualCamera.UpDirection;
            }

            set
            {
                this.ActualCamera.UpDirection = value;
            }
        }

        /// <summary>
        /// Gets or sets the change field of view cursor.
        /// </summary>
        /// <value> The change field of view cursor. </value>
        public Cursor ChangeFieldOfViewCursor
        {
            get
            {
                return (Cursor)this.GetValue(ChangeFieldOfViewCursorProperty);
            }

            set
            {
                this.SetValue(ChangeFieldOfViewCursorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the default camera (used when resetting the view).
        /// </summary>
        /// <value> The default camera. </value>
        public ProjectionCamera DefaultCamera
        {
            get
            {
                return (ProjectionCamera)this.GetValue(DefaultCameraProperty);
            }

            set
            {
                this.SetValue(DefaultCameraProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return (bool)this.GetValue(EnabledProperty);
            }

            set
            {
                this.SetValue(EnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets InertiaFactor.
        /// </summary>
        public double InertiaFactor
        {
            get
            {
                return (double)this.GetValue(InertiaFactorProperty);
            }

            set
            {
                this.SetValue(InertiaFactorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether InfiniteSpin.
        /// </summary>
        public bool InfiniteSpin
        {
            get
            {
                return (bool)this.GetValue(InfiniteSpinProperty);
            }

            set
            {
                this.SetValue(InfiniteSpinProperty, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsActive.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this.Enabled && this.Viewport != null && this.ActualCamera != null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether field of view can be changed.
        /// </summary>
        public bool IsChangeFieldOfViewEnabled
        {
            get
            {
                return (bool)this.GetValue(IsChangeFieldOfViewEnabledProperty);
            }

            set
            {
                this.SetValue(IsChangeFieldOfViewEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether inertia is enabled for the camera manipulations.
        /// </summary>
        /// <value><c>true</c> if inertia is enabled; otherwise, <c>false</c>.</value>
        public bool IsInertiaEnabled
        {
            get
            {
                return (bool)this.GetValue(IsInertiaEnabledProperty);
            }
            set
            {
                this.SetValue(IsInertiaEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether move is enabled.
        /// </summary>
        /// <value> <c>true</c> if move is enabled; otherwise, <c>false</c> . </value>
        public bool IsMoveEnabled
        {
            get
            {
                return (bool)this.GetValue(IsMoveEnabledProperty);
            }

            set
            {
                this.SetValue(IsMoveEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether pan is enabled.
        /// </summary>
        public bool IsPanEnabled
        {
            get
            {
                return (bool)this.GetValue(IsPanEnabledProperty);
            }

            set
            {
                this.SetValue(IsPanEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether IsRotationEnabled.
        /// </summary>
        public bool IsRotationEnabled
        {
            get
            {
                return (bool)this.GetValue(IsRotationEnabledProperty);
            }

            set
            {
                this.SetValue(IsRotationEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether touch zoom (pinch gesture) is enabled.
        /// </summary>
        /// <value> <c>true</c> if touch zoom is enabled; otherwise, <c>false</c> . </value>
        public bool IsTouchZoomEnabled
        {
            get
            {
                return (bool)this.GetValue(IsTouchZoomEnabledProperty);
            }

            set
            {
                this.SetValue(IsTouchZoomEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether IsZoomEnabled.
        /// </summary>
        public bool IsZoomEnabled
        {
            get
            {
                return (bool)this.GetValue(IsZoomEnabledProperty);
            }

            set
            {
                this.SetValue(IsZoomEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the sensitivity for pan by the left and right keys.
        /// </summary>
        /// <value> The pan sensitivity. </value>
        /// <remarks>
        /// Use -1 to invert the pan direction.
        /// </remarks>
        public double LeftRightPanSensitivity
        {
            get
            {
                return (double)this.GetValue(LeftRightPanSensitivityProperty);
            }

            set
            {
                this.SetValue(LeftRightPanSensitivityProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the sensitivity for rotation by the left and right keys.
        /// </summary>
        /// <value> The rotation sensitivity. </value>
        /// <remarks>
        /// Use -1 to invert the rotation direction.
        /// </remarks>
        public double LeftRightRotationSensitivity
        {
            get
            {
                return (double)this.GetValue(LeftRightRotationSensitivityProperty);
            }

            set
            {
                this.SetValue(LeftRightRotationSensitivityProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum field of view.
        /// </summary>
        /// <value> The maximum field of view. </value>
        public double MaximumFieldOfView
        {
            get
            {
                return (double)this.GetValue(MaximumFieldOfViewProperty);
            }

            set
            {
                this.SetValue(MaximumFieldOfViewProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the minimum field of view.
        /// </summary>
        /// <value> The minimum field of view. </value>
        public double MinimumFieldOfView
        {
            get
            {
                return (double)this.GetValue(MinimumFieldOfViewProperty);
            }

            set
            {
                this.SetValue(MinimumFieldOfViewProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the model up direction.
        /// </summary>
        public Vector3D ModelUpDirection
        {
            get
            {
                return (Vector3D)this.GetValue(ModelUpDirectionProperty);
            }

            set
            {
                this.SetValue(ModelUpDirectionProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the move sensitivity.
        /// </summary>
        /// <value> The move sensitivity. </value>
        public double MoveSensitivity
        {
            get
            {
                return (double)this.GetValue(MoveSensitivityProperty);
            }

            set
            {
                this.SetValue(MoveSensitivityProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the sensitivity for zoom by the page up and page down keys.
        /// </summary>
        /// <value> The zoom sensitivity. </value>
        /// <remarks>
        /// Use -1 to invert the zoom direction.
        /// </remarks>
        public double PageUpDownZoomSensitivity
        {
            get
            {
                return (double)this.GetValue(PageUpDownZoomSensitivityProperty);
            }

            set
            {
                this.SetValue(PageUpDownZoomSensitivityProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the pan cursor.
        /// </summary>
        /// <value> The pan cursor. </value>
        public Cursor PanCursor
        {
            get
            {
                return (Cursor)this.GetValue(PanCursorProperty);
            }

            set
            {
                this.SetValue(PanCursorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to rotate around the mouse down point.
        /// </summary>
        /// <value> <c>true</c> if rotation around the mouse down point is enabled; otherwise, <c>false</c> . </value>
        public bool RotateAroundMouseDownPoint
        {
            get
            {
                return (bool)this.GetValue(RotateAroundMouseDownPointProperty);
            }

            set
            {
                this.SetValue(RotateAroundMouseDownPointProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the rotate cursor.
        /// </summary>
        /// <value> The rotate cursor. </value>
        public Cursor RotateCursor
        {
            get
            {
                return (Cursor)this.GetValue(RotateCursorProperty);
            }

            set
            {
                this.SetValue(RotateCursorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the rotation sensitivity (degrees/pixel).
        /// </summary>
        /// <value> The rotation sensitivity. </value>
        public double RotationSensitivity
        {
            get
            {
                return (double)this.GetValue(RotationSensitivityProperty);
            }

            set
            {
                this.SetValue(RotationSensitivityProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show a target adorner when manipulating the camera.
        /// </summary>
        public bool ShowCameraTarget
        {
            get
            {
                return (bool)this.GetValue(ShowCameraTargetProperty);
            }

            set
            {
                this.SetValue(ShowCameraTargetProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the max duration of mouse drag to activate spin.
        /// </summary>
        /// <remarks>
        /// If the time between mouse down and mouse up is less than this value, spin is activated.
        /// </remarks>
        public int SpinReleaseTime
        {
            get
            {
                return (int)this.GetValue(SpinReleaseTimeProperty);
            }

            set
            {
                this.SetValue(SpinReleaseTimeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the touch mode.
        /// </summary>
        /// <value> The touch mode. </value>
        public TouchMode TouchMode
        {
            get
            {
                return (TouchMode)this.GetValue(TouchModeProperty);
            }

            set
            {
                this.SetValue(TouchModeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the sensitivity for pan by the up and down keys.
        /// </summary>
        /// <value> The pan sensitivity. </value>
        /// <remarks>
        /// Use -1 to invert the pan direction.
        /// </remarks>
        public double UpDownPanSensitivity
        {
            get
            {
                return (double)this.GetValue(UpDownPanSensitivityProperty);
            }

            set
            {
                this.SetValue(UpDownPanSensitivityProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the sensitivity for rotation by the up and down keys.
        /// </summary>
        /// <value> The rotation sensitivity. </value>
        /// <remarks>
        /// Use -1 to invert the rotation direction.
        /// </remarks>
        public double UpDownRotationSensitivity
        {
            get
            {
                return (double)this.GetValue(UpDownRotationSensitivityProperty);
            }

            set
            {
                this.SetValue(UpDownRotationSensitivityProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets Viewport.
        /// </summary>
        public Viewport3D Viewport
        {
            get
            {
                return (Viewport3D)this.GetValue(ViewportProperty);
            }

            set
            {
                this.SetValue(ViewportProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to zoom around mouse down point.
        /// </summary>
        /// <value> <c>true</c> if zooming around the mouse down point is enabled; otherwise, <c>false</c> . </value>
        public bool ZoomAroundMouseDownPoint
        {
            get
            {
                return (bool)this.GetValue(ZoomAroundMouseDownPointProperty);
            }

            set
            {
                this.SetValue(ZoomAroundMouseDownPointProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the zoom cursor.
        /// </summary>
        /// <value> The zoom cursor. </value>
        public Cursor ZoomCursor
        {
            get
            {
                return (Cursor)this.GetValue(ZoomCursorProperty);
            }

            set
            {
                this.SetValue(ZoomCursorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the zoom rectangle cursor.
        /// </summary>
        /// <value> The zoom rectangle cursor. </value>
        public Cursor ZoomRectangleCursor
        {
            get
            {
                return (Cursor)this.GetValue(ZoomRectangleCursorProperty);
            }

            set
            {
                this.SetValue(ZoomRectangleCursorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets ZoomSensitivity.
        /// </summary>
        public double ZoomSensitivity
        {
            get
            {
                return (double)this.GetValue(ZoomSensitivityProperty);
            }

            set
            {
                this.SetValue(ZoomSensitivityProperty, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsOrthographicCamera.
        /// </summary>
        protected bool IsOrthographicCamera
        {
            get
            {
                return this.ActualCamera is OrthographicCamera;
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsPerspectiveCamera.
        /// </summary>
        protected bool IsPerspectiveCamera
        {
            get
            {
                return this.ActualCamera is PerspectiveCamera;
            }
        }

        /// <summary>
        /// Gets OrthographicCamera.
        /// </summary>
        protected OrthographicCamera OrthographicCamera
        {
            get
            {
                return this.ActualCamera as OrthographicCamera;
            }
        }

        /// <summary>
        /// Gets PerspectiveCamera.
        /// </summary>
        protected PerspectiveCamera PerspectiveCamera
        {
            get
            {
                return this.ActualCamera as PerspectiveCamera;
            }
        }

        /// <summary>
        /// Adds the specified move force.
        /// </summary>
        /// <param name="dx">
        /// The delta x.
        /// </param>
        /// <param name="dy">
        /// The delta y.
        /// </param>
        /// <param name="dz">
        /// The delta z.
        /// </param>
        public void AddMoveForce(double dx, double dy, double dz)
        {
            this.AddMoveForce(new Vector3D(dx, dy, dz));
        }

        /// <summary>
        /// Adds the specified move force.
        /// </summary>
        /// <param name="delta">
        /// The delta.
        /// </param>
        public void AddMoveForce(Vector3D delta)
        {
            if (!this.IsMoveEnabled)
            {
                return;
            }

            this.PushCameraSetting();
            this.moveSpeed += delta * 40;
        }

        /// <summary>
        /// Adds the specified pan force.
        /// </summary>
        /// <param name="dx">
        /// The delta x.
        /// </param>
        /// <param name="dy">
        /// The delta y.
        /// </param>
        public void AddPanForce(double dx, double dy)
        {
            this.AddPanForce(this.FindPanVector(dx, dy));
        }

        /// <summary>
        /// The add pan force.
        /// </summary>
        /// <param name="pan">
        /// The pan.
        /// </param>
        public void AddPanForce(Vector3D pan)
        {
            if (!this.IsPanEnabled)
            {
                return;
            }

            this.PushCameraSetting();
            if (this.IsInertiaEnabled)
            {
                this.panSpeed += pan * 40;
            }
            else
            {
                this.panHandler.Pan(pan);
            }
        }

        /// <summary>
        /// The add rotate force.
        /// </summary>
        /// <param name="dx">
        /// The delta x.
        /// </param>
        /// <param name="dy">
        /// The delta y.
        /// </param>
        public void AddRotateForce(double dx, double dy)
        {
            if (!this.IsRotationEnabled)
            {
                return;
            }

            this.PushCameraSetting();
            if (this.IsInertiaEnabled)
            {
                this.rotationPoint3D = this.CameraTarget;
                this.rotationPosition = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
                this.rotationSpeed.X += dx * 40;
                this.rotationSpeed.Y += dy * 40;
            }
            else
            {
                this.rotationPosition = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
                this.rotateHandler.Rotate(
                    this.rotationPosition, this.rotationPosition + new Vector(dx, dy), this.CameraTarget);
                // this.rotateHandler.Rotate(new Vector(dx,dy));
            }
        }

        /// <summary>
        /// Adds the zoom force.
        /// </summary>
        /// <param name="delta">
        /// The delta.
        /// </param>
        public void AddZoomForce(double delta)
        {
            this.AddZoomForce(delta, this.CameraTarget);
        }

        /// <summary>
        /// Adds the zoom force.
        /// </summary>
        /// <param name="delta">
        /// The delta.
        /// </param>
        /// <param name="zoomOrigin">
        /// The zoom origin.
        /// </param>
        public void AddZoomForce(double delta, Point3D zoomOrigin)
        {
            if (!this.IsZoomEnabled)
            {
                return;
            }

            this.PushCameraSetting();

            if (this.IsInertiaEnabled)
            {
                this.zoomPoint3D = zoomOrigin;
                this.zoomSpeed += delta * 8;
            }
            else
            {
                this.zoomHandler.Zoom(delta, zoomOrigin);
            }
        }

        /// <summary>
        /// Changes the direction of the camera.
        /// </summary>
        /// <param name="lookDir">
        /// The look direction.
        /// </param>
        /// <param name="upDir">
        /// The up direction.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public void ChangeDirection(Vector3D lookDir, Vector3D upDir, double animationTime = 500)
        {
            this.StopAnimations();
            this.PushCameraSetting();
            CameraHelper.ChangeDirection(this.ActualCamera, lookDir, upDir, animationTime);
        }

        /// <summary>
        /// Changes the direction of the camera.
        /// </summary>
        /// <param name="lookDir">
        /// The look direction.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public void ChangeDirection(Vector3D lookDir, double animationTime = 500)
        {
            this.StopAnimations();
            this.PushCameraSetting();
            CameraHelper.ChangeDirection(this.ActualCamera, lookDir, this.ActualCamera.UpDirection, animationTime);
        }

        /// <summary>
        /// Hides the rectangle.
        /// </summary>
        public void HideRectangle()
        {
            AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this.Viewport);
            if (this.rectangleAdorner != null)
            {
                myAdornerLayer.Remove(this.rectangleAdorner);
            }

            this.rectangleAdorner = null;

            this.Viewport.InvalidateVisual();
        }

        /// <summary>
        /// Hides the target adorner.
        /// </summary>
        public void HideTargetAdorner()
        {
            AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this.Viewport);
            if (this.targetAdorner != null)
            {
                myAdornerLayer.Remove(this.targetAdorner);
            }

            this.targetAdorner = null;

            // the adorner sometimes leaves some 'dust', so refresh the viewport
            this.RefreshViewport();
        }

        /// <summary>
        /// Change the "look-at" point.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        [Obsolete]
        public void LookAt(Point3D target, double animationTime)
        {
            if (!this.IsPanEnabled)
            {
                return;
            }

            this.PushCameraSetting();
            CameraHelper.LookAt(this.Camera, target, animationTime);
        }

        /// <summary>
        /// Push the current camera settings on an internal stack.
        /// </summary>
        public void PushCameraSetting()
        {
            this.cameraHistory.Add(new CameraSetting(this.ActualCamera));
            if (this.cameraHistory.Count > 100)
            {
                this.cameraHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Resets the camera.
        /// </summary>
        public void ResetCamera()
        {
            if (!this.IsZoomEnabled || !this.IsRotationEnabled || !this.IsPanEnabled)
            {
                return;
            }

            this.PushCameraSetting();
            if (this.DefaultCamera != null)
            {
                CameraHelper.Copy(this.DefaultCamera, this.ActualCamera);
            }
            else
            {
                CameraHelper.Reset(this.ActualCamera);
                CameraHelper.ZoomExtents(this.ActualCamera, this.Viewport);
            }
        }

        /// <summary>
        /// Resets the camera up direction.
        /// </summary>
        public void ResetCameraUpDirection()
        {
            this.CameraUpDirection = this.ModelUpDirection;
        }

        /// <summary>
        /// Restores the most recent camera setting from the internal stack.
        /// </summary>
        /// <returns> The restore camera setting. </returns>
        public bool RestoreCameraSetting()
        {
            if (this.cameraHistory.Count > 0)
            {
                CameraSetting cs = this.cameraHistory[this.cameraHistory.Count - 1];
                this.cameraHistory.RemoveAt(this.cameraHistory.Count - 1);
                cs.UpdateCamera(this.ActualCamera);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Shows the rectangle.
        /// </summary>
        /// <param name="rect">
        /// The rectangle.
        /// </param>
        /// <param name="color1">
        /// The color 1.
        /// </param>
        /// <param name="color2">
        /// The color 2.
        /// </param>
        public void ShowRectangle(Rect rect, Color color1, Color color2)
        {
            if (this.rectangleAdorner != null)
            {
                return;
            }

            var myAdornerLayer = AdornerLayer.GetAdornerLayer(this.Viewport);
            this.rectangleAdorner = new RectangleAdorner(
                this.Viewport, rect, color1, color2, 3, 1, 10, DashStyles.Solid);
            myAdornerLayer.Add(this.rectangleAdorner);
        }

        /// <summary>
        /// Shows the target adorner.
        /// </summary>
        /// <param name="position">
        /// The position.
        /// </param>
        public void ShowTargetAdorner(Point position)
        {
            if (!this.ShowCameraTarget)
            {
                return;
            }

            if (this.targetAdorner != null)
            {
                return;
            }

            AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this.Viewport);
            this.targetAdorner = new TargetSymbolAdorner(this.Viewport, position);
            myAdornerLayer.Add(this.targetAdorner);
        }

        /// <summary>
        /// Starts the spin.
        /// </summary>
        /// <param name="speed">
        /// The speed.
        /// </param>
        /// <param name="position">
        /// The position.
        /// </param>
        /// <param name="aroundPoint">
        /// The spin around point.
        /// </param>
        public void StartSpin(Vector speed, Point position, Point3D aroundPoint)
        {
            this.spinningSpeed = speed;
            this.spinningPosition = position;
            this.spinningPoint3D = aroundPoint;
            this.isSpinning = true;
        }

        /// <summary>
        /// Stops the spin.
        /// </summary>
        public void StopSpin()
        {
            this.isSpinning = false;
        }

        /// <summary>
        /// Updates the rectangle.
        /// </summary>
        /// <param name="rect">
        /// The rectangle.
        /// </param>
        public void UpdateRectangle(Rect rect)
        {
            if (this.rectangleAdorner == null)
            {
                return;
            }

            this.rectangleAdorner.Rectangle = rect;
            this.rectangleAdorner.InvalidateVisual();
        }

        /// <summary>
        /// Zooms by the specified delta value.
        /// </summary>
        /// <param name="delta">
        /// The delta value.
        /// </param>
        public void Zoom(double delta)
        {
            this.zoomHandler.Zoom(delta);
        }

        /// <summary>
        /// Zooms to the extents of the model.
        /// </summary>
        /// <param name="animationTime">
        /// The animation time (milliseconds).
        /// </param>
        public void ZoomExtents(double animationTime = 200)
        {
            if (!this.IsZoomEnabled)
            {
                return;
            }

            this.PushCameraSetting();
            CameraHelper.ZoomExtents(this.ActualCamera, this.Viewport, animationTime);
        }

        /// <summary>
        /// Called when the <see cref="E:System.Windows.UIElement.ManipulationCompleted" /> event occurs.
        /// </summary>
        /// <param name="e">
        /// The data for the event.
        /// </param>
        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            base.OnManipulationCompleted(e);
            var p = this.touchDownPoint + e.TotalManipulation.Translation;

            switch (this.TouchMode)
            {
                case TouchMode.Panning:
                    this.panHandler.Completed(new ManipulationEventArgs(p));
                    break;
                case TouchMode.Rotating:
                    this.rotateHandler.Completed(new ManipulationEventArgs(p));
                    break;
            }

            this.zoomHandler.Completed(new ManipulationEventArgs(p));

            // Tap
            double l = e.TotalManipulation.Translation.Length;
            if (l < 4 && this.TouchMode != TouchMode.None)
            {
                this.TouchMode = this.TouchMode == TouchMode.Panning ? TouchMode.Rotating : TouchMode.Panning;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Called when the <see cref="E:System.Windows.UIElement.ManipulationDelta" /> event occurs.
        /// </summary>
        /// <param name="e">
        /// The data for the event.
        /// </param>
        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            // http://msdn.microsoft.com/en-us/library/system.windows.uielement.manipulationdelta.aspx

            // Debug.WriteLine("OnManipulationDelta: T={0}, S={1}, R={2}, O={3}", e.DeltaManipulation.Translation, e.DeltaManipulation.Scale, e.DeltaManipulation.Rotation, e.ManipulationOrigin);
            Point position = this.touchDownPoint + e.CumulativeManipulation.Translation;

            switch (this.TouchMode)
            {
                case TouchMode.Panning:
                    this.panHandler.Delta(new ManipulationEventArgs(position));
                    break;
                case TouchMode.Rotating:
                    this.rotateHandler.Delta(new ManipulationEventArgs(position));
                    break;
            }

            if (this.IsTouchZoomEnabled)
            {
                var zoomAroundPoint = this.zoomHandler.UnProject(
                    e.ManipulationOrigin, this.zoomHandler.Origin, this.CameraLookDirection);
                if (zoomAroundPoint != null)
                {
                    this.zoomHandler.Zoom(1 - (e.DeltaManipulation.Scale.Length / Math.Sqrt(2)), zoomAroundPoint.Value);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Called when the <see cref="E:System.Windows.UIElement.ManipulationStarted" /> event occurs.
        /// </summary>
        /// <param name="e">
        /// The data for the event.
        /// </param>
        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            base.OnManipulationStarted(e);
            this.Focus();
            this.touchDownPoint = e.ManipulationOrigin;
            this.panHandler.Started(new ManipulationEventArgs(this.touchDownPoint));
            this.rotateHandler.Started(new ManipulationEventArgs(this.touchDownPoint));
            this.zoomHandler.Started(new ManipulationEventArgs(this.touchDownPoint));
            e.Handled = true;
        }

        /// <summary>
        /// Invoked when an unhandled MouseDown attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.
        /// </param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
            if (e.ChangedButton == MouseButton.XButton1)
            {
                this.RestoreCameraSetting();
            }
        }

        /// <summary>
        /// Invoked when an unhandled StylusSystemGesture attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:System.Windows.Input.StylusSystemGestureEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnStylusSystemGesture(StylusSystemGestureEventArgs e)
        {
            base.OnStylusSystemGesture(e);

            // Debug.WriteLine("OnStylusSystemGesture: " + e.SystemGesture);
            if (e.SystemGesture == SystemGesture.HoldEnter)
            {
                Point p = e.GetPosition(this);
                this.changeLookAtHandler.Started(new ManipulationEventArgs(p));
                this.changeLookAtHandler.Completed(new ManipulationEventArgs(p));
                e.Handled = true;
            }

            if (e.SystemGesture == SystemGesture.TwoFingerTap)
            {
                this.ZoomExtents();
                e.Handled = true;
            }
        }

        /// <summary>
        /// The camera changed.
        /// </summary>
        /// <param name="d">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private static void CameraChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CameraController)d).OnCameraChanged();
        }

        /// <summary>
        /// The viewport changed.
        /// </summary>
        /// <param name="d">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private static void ViewportChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CameraController)d).OnViewportChanged();
        }

        /// <summary>
        /// The back view event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void BackViewHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.ChangeDirection(new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));
        }

        /// <summary>
        /// The bottom view event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void BottomViewHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.ChangeDirection(new Vector3D(0, 0, 1), new Vector3D(0, -1, 0));
        }

        /// <summary>
        /// The camera controller_ loaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void CameraControllerLoaded(object sender, RoutedEventArgs e)
        {
            this.SubscribeEvents();
        }

        /// <summary>
        /// Called when the CameraController is unloaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void CameraControllerUnloaded(object sender, RoutedEventArgs e)
        {
            this.UnSubscribeEvents();
        }

        /// <summary>
        /// Clamps the specified value between the limits.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="min">
        /// The min.
        /// </param>
        /// <param name="max">
        /// The max.
        /// </param>
        /// <returns>
        /// The clamp.
        /// </returns>
        private double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        /// <summary>
        /// Finds the pan vector.
        /// </summary>
        /// <param name="dx">
        /// The delta x.
        /// </param>
        /// <param name="dy">
        /// The delta y.
        /// </param>
        /// <returns>
        /// The <see cref="Vector3D" /> .
        /// </returns>
        private Vector3D FindPanVector(double dx, double dy)
        {
            var axis1 = Vector3D.CrossProduct(this.CameraLookDirection, this.CameraUpDirection);
            var axis2 = Vector3D.CrossProduct(axis1, this.CameraLookDirection);
            axis1.Normalize();
            axis2.Normalize();
            double l = this.CameraLookDirection.Length;
            double f = l * 0.001;
            var move = (-axis1 * f * dx) + (axis2 * f * dy);

            // this should be dependent on distance to target?
            return move;
        }

        /// <summary>
        /// The front view event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void FrontViewHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.ChangeDirection(new Vector3D(-1, 0, 0), new Vector3D(0, 0, 1));
        }

        /// <summary>
        /// Initializes the input bindings.
        /// </summary>
        private void InitializeBindings()
        {
            this.changeLookAtHandler = new RotateHandler(this, true);
            this.rotateHandler = new RotateHandler(this);
            this.zoomRectangleHandler = new ZoomRectangleHandler(this);
            this.zoomHandler = new ZoomHandler(this);
            this.panHandler = new PanHandler(this);
            this.changeFieldOfViewHandler = new ZoomHandler(this, true);

            this.CommandBindings.Add(new CommandBinding(ZoomRectangleCommand, this.zoomRectangleHandler.Execute));
            this.CommandBindings.Add(new CommandBinding(ZoomExtentsCommand, this.ZoomExtentsHandler));
            this.CommandBindings.Add(new CommandBinding(RotateCommand, this.rotateHandler.Execute));
            this.CommandBindings.Add(new CommandBinding(ZoomCommand, this.zoomHandler.Execute));
            this.CommandBindings.Add(new CommandBinding(PanCommand, this.panHandler.Execute));
            this.CommandBindings.Add(new CommandBinding(ResetCameraCommand, this.ResetCameraHandler));
            this.CommandBindings.Add(new CommandBinding(ChangeLookAtCommand, this.changeLookAtHandler.Execute));
            this.CommandBindings.Add(
                new CommandBinding(ChangeFieldOfViewCommand, this.changeFieldOfViewHandler.Execute));

            this.CommandBindings.Add(new CommandBinding(TopViewCommand, this.TopViewHandler));
            this.CommandBindings.Add(new CommandBinding(BottomViewCommand, this.BottomViewHandler));
            this.CommandBindings.Add(new CommandBinding(LeftViewCommand, this.LeftViewHandler));
            this.CommandBindings.Add(new CommandBinding(RightViewCommand, this.RightViewHandler));
            this.CommandBindings.Add(new CommandBinding(FrontViewCommand, this.FrontViewHandler));
            this.CommandBindings.Add(new CommandBinding(BackViewCommand, this.BackViewHandler));
        }

        /// <summary>
        /// The left view event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void LeftViewHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.ChangeDirection(new Vector3D(0, 1, 0), new Vector3D(0, 0, 1));
        }

        /// <summary>
        /// The on camera changed.
        /// </summary>
        private void OnCameraChanged()
        {
            this.cameraHistory.Clear();
            this.PushCameraSetting();
        }

        /// <summary>
        /// The rendering event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void OnCompositionTargetRendering(object sender, RenderingEventArgs e)
        {
            var ticks = e.RenderingTime.Ticks;
            double time = 100e-9 * (ticks - this.lastTick);

            if (this.lastTick != 0)
            {
                this.OnTimeStep(time);
            }

            this.lastTick = ticks;
        }

        /// <summary>
        /// Called when a key is pressed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.KeyEventArgs" /> instance containing the event data.
        /// </param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            this.OnKeyDown(e);
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool control = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            double f = control ? 0.25 : 1;

            if (!shift)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        this.AddRotateForce(-1 * f * this.LeftRightRotationSensitivity, 0);
                        e.Handled = true;
                        break;
                    case Key.Right:
                        this.AddRotateForce(1 * f * this.LeftRightRotationSensitivity, 0);
                        e.Handled = true;
                        break;
                    case Key.Up:
                        this.AddRotateForce(0, -1 * f * this.UpDownRotationSensitivity);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        this.AddRotateForce(0, 1 * f * this.UpDownRotationSensitivity);
                        e.Handled = true;
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Left:
                        this.AddPanForce(-5 * f * this.LeftRightPanSensitivity, 0);
                        e.Handled = true;
                        break;
                    case Key.Right:
                        this.AddPanForce(5 * f * this.LeftRightPanSensitivity, 0);
                        e.Handled = true;
                        break;
                    case Key.Up:
                        this.AddPanForce(0, -5 * f * this.UpDownPanSensitivity);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        this.AddPanForce(0, 5 * f * this.UpDownPanSensitivity);
                        e.Handled = true;
                        break;
                }
            }

            switch (e.Key)
            {
                case Key.PageUp:
                    this.AddZoomForce(-0.1 * f * this.PageUpDownZoomSensitivity);
                    e.Handled = true;
                    break;
                case Key.PageDown:
                    this.AddZoomForce(0.1 * f * this.PageUpDownZoomSensitivity);
                    e.Handled = true;
                    break;
                case Key.Back:
                    if (this.RestoreCameraSetting())
                    {
                        e.Handled = true;
                    }

                    break;
            }

            switch (e.Key)
            {
                case Key.W:
                    this.AddMoveForce(0, 0, 0.1 * f * this.MoveSensitivity);
                    break;
                case Key.A:
                    this.AddMoveForce(-0.1 * f * this.LeftRightPanSensitivity, 0, 0);
                    break;
                case Key.S:
                    this.AddMoveForce(0, 0, -0.1 * f * this.MoveSensitivity);
                    break;
                case Key.D:
                    this.AddMoveForce(0.1 * f * this.LeftRightPanSensitivity, 0, 0);
                    break;
                case Key.Z:
                    this.AddMoveForce(0, -0.1 * f * this.LeftRightPanSensitivity, 0);
                    break;
                case Key.Q:
                    this.AddMoveForce(0, 0.1 * f * this.LeftRightPanSensitivity, 0);
                    break;
            }
        }

        /// <summary>
        /// Called when the mouse wheel is moved.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.MouseWheelEventArgs" /> instance containing the event data.
        /// </param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!this.IsZoomEnabled)
            {
                return;
            }

            if (this.ZoomAroundMouseDownPoint)
            {
                Point point = e.GetPosition(this);
                Point3D nearestPoint;
                Vector3D normal;
                DependencyObject visual;
                if (Viewport3DHelper.FindNearest(this.Viewport, point, out nearestPoint, out normal, out visual))
                {
                    this.AddZoomForce(-e.Delta * 0.001, nearestPoint);
                    e.Handled = true;
                    return;
                }
            }

            this.AddZoomForce(-e.Delta * 0.001);
            e.Handled = true;
        }

        /// <summary>
        /// The on time step.
        /// </summary>
        /// <param name="time">
        /// The time.
        /// </param>
        private void OnTimeStep(double time)
        {
            // should be independent of time
            double factor = this.IsInertiaEnabled ? Math.Pow(this.InertiaFactor, time / 0.012) : 0;
            factor = this.Clamp(factor, 0.2, 1);

            if (this.isSpinning && this.spinningSpeed.LengthSquared > 0)
            {
                this.rotateHandler.Rotate(
                    this.spinningPosition, this.spinningPosition + (this.spinningSpeed * time), this.spinningPoint3D);

                if (!this.InfiniteSpin)
                {
                    this.spinningSpeed *= factor;
                }
            }

            if (this.rotationSpeed.LengthSquared > 0.1)
            {
                this.rotateHandler.Rotate(
                    this.rotationPosition, this.rotationPosition + (this.rotationSpeed * time), this.rotationPoint3D);
                this.rotationSpeed *= factor;
            }

            if (Math.Abs(this.panSpeed.LengthSquared) > 0.0001)
            {
                this.panHandler.Pan(this.panSpeed * time);
                this.panSpeed *= factor;
            }

            if (Math.Abs(this.moveSpeed.LengthSquared) > 0.0001)
            {
                this.zoomHandler.MoveCameraPosition(this.moveSpeed * time);
                this.moveSpeed *= factor;
            }

            if (Math.Abs(this.zoomSpeed) > 0.1)
            {
                this.zoomHandler.Zoom(this.zoomSpeed * time, this.zoomPoint3D);
                this.zoomSpeed *= factor;
            }
        }

        /// <summary>
        /// The on viewport changed.
        /// </summary>
        private void OnViewportChanged()
        {
        }

        /// <summary>
        /// The refresh viewport.
        /// </summary>
        private void RefreshViewport()
        {
            // todo: this is a hack, should be improved

            // var mg = new ModelVisual3D { Content = new AmbientLight(Colors.White) };
            // Viewport.Children.Add(mg);
            // Viewport.Children.Remove(mg);
            Camera c = this.Viewport.Camera;
            this.Viewport.Camera = null;
            this.Viewport.Camera = c;

            // var w = Viewport.Width;
            // Viewport.Width = w-1;
            // Viewport.Width = w;

            // Viewport.InvalidateVisual();
        }

        /// <summary>
        /// The reset camera event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void ResetCameraHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.IsPanEnabled && this.IsZoomEnabled && this.CameraMode != CameraMode.FixedPosition)
            {
                this.StopAnimations();
                this.ResetCamera();
            }
        }

        /// <summary>
        /// The right view event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void RightViewHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.ChangeDirection(new Vector3D(0, -1, 0), new Vector3D(0, 0, 1));
        }

        /// <summary>
        /// The stop animations.
        /// </summary>
        private void StopAnimations()
        {
            this.rotationSpeed = new Vector();
            this.panSpeed = new Vector3D();
            this.zoomSpeed = 0;
        }

        /// <summary>
        /// The subscribe events.
        /// </summary>
        private void SubscribeEvents()
        {
            this.MouseWheel += this.OnMouseWheel;
            this.KeyDown += this.OnKeyDown;
            RenderingEventManager.AddListener(this.renderingEventListener);
        }

        /// <summary>
        /// The top view event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void TopViewHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.ChangeDirection(new Vector3D(0, 0, -1), new Vector3D(0, 1, 0));
        }

        /// <summary>
        /// The un subscribe events.
        /// </summary>
        private void UnSubscribeEvents()
        {
            this.MouseWheel -= this.OnMouseWheel;
            this.KeyDown -= this.OnKeyDown;
            RenderingEventManager.RemoveListener(this.renderingEventListener);
        }

        /// <summary>
        /// The Zoom extents event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void ZoomExtentsHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.StopAnimations();
            this.ZoomExtents();
        }


        /// <summary>
        /// The look at (target) point changed event
        /// </summary>
        public static readonly RoutedEvent LookAtChangedEvent = EventManager.RegisterRoutedEvent(
            "LookAtChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CameraController));

        /// <summary>
        /// Occurs when the look at/target point changed.
        /// </summary>
        public event RoutedEventHandler LookAtChanged
        {
            add
            {
                this.AddHandler(LookAtChangedEvent, value);
            }

            remove
            {
                this.RemoveHandler(LookAtChangedEvent, value);
            }
        }

        /// <summary>
        /// Raises the LookAtChanged event.
        /// </summary>
        internal protected virtual void OnLookAtChanged()
        {
            var args = new RoutedEventArgs(LookAtChangedEvent);
            this.RaiseEvent(args);

        }

        /// <summary>
        /// The zoomed by rectangle event
        /// </summary>
        public static readonly RoutedEvent ZoomedByRectangleEvent = EventManager.RegisterRoutedEvent(
        "ZoomedByRectangle", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CameraController));

        /// <summary>
        /// Occurs when the view is zoomed by rectangle.
        /// </summary>
        public event RoutedEventHandler ZoomedByRectangle
        {
            add
            {
                this.AddHandler(ZoomedByRectangleEvent, value);
            }

            remove
            {
                this.RemoveHandler(ZoomedByRectangleEvent, value);
            }
        }

        /// <summary>
        /// Raises the ZoomedByRectangle event.
        /// </summary>
        internal protected virtual void OnZoomedByRectangle()
        {
            var args = new RoutedEventArgs(ZoomedByRectangleEvent);
            this.RaiseEvent(args);

        }

        public static readonly RoutedEvent ZoomedEvent = EventManager.RegisterRoutedEvent("Zoomed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CameraController));

        public static readonly RoutedEvent RotatedEvent = EventManager.RegisterRoutedEvent("Rotated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CameraController));

        public event RoutedEventHandler Zoomed
        {
            add
            {
                base.AddHandler(CameraController.ZoomedEvent, value);
            }
            remove
            {
                base.RemoveHandler(CameraController.ZoomedEvent, value);
            }
        }

        public event RoutedEventHandler Rotated
        {
            add
            {
                base.AddHandler(CameraController.RotatedEvent, value);
            }
            remove
            {
                base.RemoveHandler(CameraController.RotatedEvent, value);
            }
        }

        private bool zoomIn = false;
        /// <summary>
        /// In Zooming mode
        /// </summary>
        public bool ZoomIn
        {
            get { return zoomIn; }
            set { zoomIn = value; }
        }

        protected internal virtual void OnZoomed()
        {
            RoutedEventArgs args = new RoutedEventArgs(ZoomedEvent);
            base.RaiseEvent(args);
        }

        protected internal virtual void OnRotated()
        {
            RoutedEventArgs args = new RoutedEventArgs(RotatedEvent);
            base.RaiseEvent(args);
        }

        public void GotoXYView()
        {
            ProjectionCamera camera = this.Viewport.Camera as ProjectionCamera;
            Point3D target = camera.Position + camera.LookDirection;
            camera.LookDirection = new Vector3D(0.0, 0.0, -1.0) * camera.LookDirection.Length;
            camera.UpDirection = new Vector3D(-1.0, 0.0, 0.0);
            camera.Position = target - camera.LookDirection;
            this.OnRotated();
        }

        public void GotoYZView()
        {
            ProjectionCamera camera = this.Viewport.Camera as ProjectionCamera;
            Point3D target = camera.Position + camera.LookDirection;
            camera.LookDirection = new Vector3D(-1.0, 0.0, 0.0) * camera.LookDirection.Length;
            camera.UpDirection = new Vector3D(0.0, 0.0, 1.0);
            camera.Position = target - camera.LookDirection;
            this.OnRotated();
        }

        public void GotoXZView()
        {
            ProjectionCamera camera = this.Viewport.Camera as ProjectionCamera;
            Point3D target = camera.Position + camera.LookDirection;
            camera.LookDirection = new Vector3D(0.0, -1.0, 0.0) * camera.LookDirection.Length;
            camera.UpDirection = new Vector3D(0.0, 0.0, 1.0);
            camera.Position = target - camera.LookDirection;
            this.OnRotated();
        }
    }
}