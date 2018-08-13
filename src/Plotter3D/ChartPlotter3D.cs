using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Plotter3D.Common;
using Plotter3D.Surface;

namespace Plotter3D
{
    /// <summary>
    ///ChartPlotter3D
    /// </summary>
    [TemplatePart(Name = "PART_VIEWPORT", Type = typeof(HelixViewport3D))]
    [TemplatePart(Name = "PART_GRID", Type = typeof(Grid))]
    public class ChartPlotter3D : ItemsControl
    {
        private const string PartViewport = "PART_VIEWPORT";

        private const string PartMainGrid = "PART_GRID";

        private const string CurveVisualKey = "CurveVisual";

        private const string PartPointToolTip = "Part_PointToolTip";

        private static readonly Cursor cursorZoomIn;

        private static readonly Cursor cursorZoomOut;

        private HelixViewport3D _viewport3D;

        private AxisGrid _axisGrid;

        private AxisFrame _axisFrame;

        private SurfaceWireframe _wireFrame;

        private Surface3D _curveModel;

        private Surface3DPoints _pntsModel;

        private SurfaceMesh3D _mesh3D;

        private Grid _mainGrid;

        private ModelVisual3D _dataPointTip;

        private PointToolTip _ptToolTip;

        private ClickModes _clickMode = ClickModes.Normal;

        private Range<double> _roundZRange;

        private bool _showPointGraph = false;

        private bool _showMesh = false;

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data", typeof(double[,]), typeof(ChartPlotter3D), new UIPropertyMetadata(null, DataChanged));

        public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(
            "Points", typeof(Point3D[,]), typeof(ChartPlotter3D), new UIPropertyMetadata(null, GeometryChanged));

        public static readonly DependencyProperty XRangeProperty = DependencyProperty.Register(
            "XRange", typeof(Range<double>), typeof(ChartPlotter3D), new UIPropertyMetadata(null));

        public static readonly DependencyProperty YRangeProperty = DependencyProperty.Register(
            "YRange", typeof(Range<double>), typeof(ChartPlotter3D), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ZRangeProperty = DependencyProperty.Register(
            "ZRange", typeof(Range<double>), typeof(ChartPlotter3D), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ShowAxisLabelProperty = DependencyProperty.Register(
            "ShowAxisLabel", typeof(bool), typeof(ChartPlotter3D), new UIPropertyMetadata(true, new PropertyChangedCallback(OnShowAxisLabelChanged)));


        public static RoutedCommand ZoomCommand = new RoutedCommand();
        public static RoutedCommand PanCommand = new RoutedCommand();
        public static RoutedCommand ViewXYZCommand = new RoutedCommand();
        public static RoutedCommand ResetViewCommand = new RoutedCommand();
        public static RoutedCommand DataCursorCommand = new RoutedCommand();
        public static RoutedCommand NormalCommand = new RoutedCommand();
        public static RoutedCommand MeshViewCommand = new RoutedCommand();
        public static RoutedCommand ShowHideLabelCommand = new RoutedCommand();



        private double _minimumDistance = double.MaxValue;

        private Point3D _nearestPt = default(Point3D);

        private Vector3D _nearestNormal = default(Vector3D);

        private RayMeshGeometry3DHitTestResult _rayhit = null;

        private Visibility _dataTipVsisible = Visibility.Hidden;

        public bool AutoScale
        {
            get;
            set;
        }

        public AxisGrid AxisGrid
        {
            get
            {
                return _axisGrid;
            }
        }

        public SurfaceWireframe SurfaceWireframe
        {
            get
            {
                return _wireFrame;
            }
        }

        public Surface3D Surface3D
        {
            get
            {
                return _curveModel;
            }
        }

        public Surface3DPoints Surface3DPoints
        {
            get
            {
                return _pntsModel;
            }
        }

        public Visual3DCollection Children
        {
            get
            {
                return _viewport3D.Children;
            }
        }

        /// <summary>
        /// Two dimensions array,length of first dimension is number of points on X Axis direction,
        ///  the lenth of sencond dimension is number of points on Y Axis direction, and the array value
        ///  represents value on Z Axis.
        /// </summary>

        public double[,] Data
        {
            get
            {
                return (double[,])base.GetValue(ChartPlotter3D.DataProperty);
            }
            set
            {
                base.SetValue(ChartPlotter3D.DataProperty, value);
            }
        }

        /// <summary>
        /// Two dimensions array,length of first dimension is number of points on X Axis direction,
        ///  the lenth of sencond dimension is number of points on Y Axis direction, and the array value
        ///  Point3D represents values on X,Y,Z Axis.
        /// </summary>

        public Point3D[,] Points
        {
            get
            {
                return (Point3D[,])base.GetValue(ChartPlotter3D.PointsProperty);
            }
            set
            {
                base.SetValue(ChartPlotter3D.PointsProperty, value);
            }
        }

        public Range<double> XRange
        {
            get
            {
                return (Range<double>)base.GetValue(ChartPlotter3D.XRangeProperty);
            }
            private set
            {
                base.SetValue(ChartPlotter3D.XRangeProperty, value);
            }
        }

        public Range<double> YRange
        {
            get
            {
                return (Range<double>)base.GetValue(ChartPlotter3D.YRangeProperty);
            }
            private set
            {
                base.SetValue(ChartPlotter3D.YRangeProperty, value);
            }
        }

        public Range<double> ZRange
        {
            get
            {
                return (Range<double>)base.GetValue(ChartPlotter3D.ZRangeProperty);
            }
            private set
            {
                base.SetValue(ChartPlotter3D.ZRangeProperty, value);
            }
        }

        public bool ShowAxisLabel
        {
            get
            {
                return (bool)base.GetValue(ChartPlotter3D.ShowAxisLabelProperty);
            }
            set
            {
                base.SetValue(ChartPlotter3D.ShowAxisLabelProperty, value);
            }
        }

        public HelixViewport3D Viewport
        {
            get
            {
                return _viewport3D;
            }
        }

        public ClickModes ClickMode
        {
            get
            {
                return _clickMode;
            }
            set
            {
                _clickMode = value;
                switch (value)
                {
                    case ClickModes.Normal:
                        this.SetNormalMode();
                        break;

                    case ClickModes.ZoomIn:
                        this.SetZoomInMode();
                        break;

                    case ClickModes.ZoomOut:
                        this.SetZoomOutMode();
                        break;

                    case ClickModes.DataCursor:
                        this.SetCusorMode();
                        break;
                }
            }
        }

        static ChartPlotter3D()
        {
            ChartPlotter3D.cursorZoomIn = null;
            ChartPlotter3D.cursorZoomOut = null;

            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartPlotter3D), new FrameworkPropertyMetadata(typeof(ChartPlotter3D)));
            ChartPlotter3D.cursorZoomIn = ChartPlotter3D.LoadCursor("ZoomIn");
            ChartPlotter3D.cursorZoomOut = ChartPlotter3D.LoadCursor("ZoomOut");
        }

        private static Cursor LoadCursor(string name)
        {
            string imagePath = string.Format("pack://application:,,,/Plotter3D;Component/Resources/{0}.ico", name);
            Stream stream = Application.GetResourceStream(new Uri(imagePath)).Stream;
            return new Cursor(stream);
        }

        public ChartPlotter3D()
        {
            _dataPointTip = new ModelVisual3D();
            _axisGrid = new AxisGrid();
            NumericTicksProvider numericProvider = new NumericTicksProvider(6, 4, 0.15, 0.1);
            numericProvider.LabelStringFormat = "{0:0.00}";
            _axisGrid.TicksProvider = numericProvider;
            _axisFrame = new AxisFrame();
            _axisFrame.AxisLength = _axisGrid.AxisLength;
            _curveModel = new Surface3D();
            _curveModel.SetValue(FrameworkElement.NameProperty, "CurveVisual");
            _wireFrame = new SurfaceWireframe();
            _wireFrame.Thickness = 1.2;
            _wireFrame.DepthOffset = 1E-05;
            _pntsModel = new Surface3DPoints();
            _mesh3D = new SurfaceMesh3D();
            _mesh3D.DepthOffset = 1E-05;
            base.CommandBindings.Add(new CommandBinding(ChartPlotter3D.ZoomCommand, new ExecutedRoutedEventHandler(this.OnMenuZoomCommand)));
            base.CommandBindings.Add(new CommandBinding(ChartPlotter3D.NormalCommand, new ExecutedRoutedEventHandler(this.OnMenuNormalCommand)));
            base.CommandBindings.Add(new CommandBinding(ChartPlotter3D.DataCursorCommand, new ExecutedRoutedEventHandler(this.OnMenuDataCursorCommand)));
            base.CommandBindings.Add(new CommandBinding(ChartPlotter3D.PanCommand, new ExecutedRoutedEventHandler(this.OnPanCommand)));
            base.CommandBindings.Add(new CommandBinding(ChartPlotter3D.ViewXYZCommand, new ExecutedRoutedEventHandler(this.OnViewXYZCommand)));
            base.CommandBindings.Add(new CommandBinding(ChartPlotter3D.ResetViewCommand, new ExecutedRoutedEventHandler(this.OnResetViewCommand)));
            base.CommandBindings.Add(new CommandBinding(ChartPlotter3D.MeshViewCommand, new ExecutedRoutedEventHandler(this.OnMeshViewCommand)));
            base.Loaded += new RoutedEventHandler(this.ChartPlotter3D_Loaded);
        }

        private void ChartPlotter3D_Loaded(object sender, RoutedEventArgs e)
        {
            _axisGrid.Viewport = _viewport3D;
            _axisFrame.Viewport = _viewport3D.Viewport;
            _viewport3D.Children.Add(_axisFrame);
        }

        public override void OnApplyTemplate()
        {
            _viewport3D = (base.Template.FindName("PART_VIEWPORT", this) as HelixViewport3D);
            _viewport3D.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnMOuseLeftBtnDown);
            _viewport3D.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseLeftButtonUp);
            _mainGrid = (base.Template.FindName("PART_GRID", this) as Grid);
            _ptToolTip = (base.Template.FindName("Part_PointToolTip", this) as PointToolTip);
            _viewport3D.Children.Add(_curveModel);
            _viewport3D.Children.Add(_wireFrame);
            _viewport3D.Children.Add(_axisGrid);
            _viewport3D.Children.Add(_dataPointTip);
            base.OnApplyTemplate();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected static void DataChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ChartPlotter3D plotter = (ChartPlotter3D)sender;
            plotter.Points = plotter.CreatePoints();
            plotter.UpdateGeometry();
        }

        protected static void GeometryChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((ChartPlotter3D)sender).UpdateGeometry();
        }
        private static void OnShowAxisLabelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var plotter = ((ChartPlotter3D)sender);
            plotter._axisGrid.ShowAxisLabel = plotter.ShowAxisLabel;
        }

        public void UpdateGeometry()
        {
            //Very wired,If you do not remove and add curveModel here,
            //program will be crashed after updating curvemode's Points property.
            _viewport3D.Children.Remove(_curveModel);
            _viewport3D.Children.Remove(_pntsModel);
            _viewport3D.Children.Remove(_mesh3D);
            _viewport3D.Children.Remove(_wireFrame);
            this.AutoAdjustCoordinate();
            _curveModel.TextureBuilder = new VisualSpectrumTextureBuilder
            {
                DataRange = this.ZRange
            };

            _curveModel.Points = this.Points;
            _curveModel.Transform = _axisGrid.DataToAxisTransform;
            _pntsModel.TextureBuilder = _curveModel.TextureBuilder;
            _pntsModel.Points = this.Points;
            _pntsModel.Transform = _axisGrid.DataToAxisTransform;
            _mesh3D.TextureBuilder = _curveModel.TextureBuilder;
            _mesh3D.Points = this.Points;
            _mesh3D.Transform = _axisGrid.DataToAxisTransform;
            _wireFrame.Points = this.Points;
            _wireFrame.Transform = _axisGrid.DataToAxisTransform;
            _dataPointTip.Transform = _axisGrid.DataToAxisTransform;
            if (!_showPointGraph)
            {
                if (_showMesh)
                {
                    _viewport3D.Children.Add(_mesh3D);
                }
                else
                {
                    _viewport3D.Children.Add(_curveModel);
                    _viewport3D.Children.Add(_wireFrame);
                }
            }
            else
            {
                _viewport3D.Children.Add(_pntsModel);
            }
            _dataPointTip.Children.Clear();
            _ptToolTip.Visibility = Visibility.Hidden;
        }

        private Point3D[,] CreatePoints()
        {
            Point3D[,] pnts = new Point3D[this.Data.GetLength(0), this.Data.GetLength(1)];
            for (int i = 0; i < pnts.GetLength(0); i++)
            {
                Point3D validP = new Point3D((double)i, 0.0, this.Data[i, 0]);
                for (int j = 0; j < pnts.GetLength(1); j++)
                {
                    if (!double.IsNaN(this.Data[i, j]))
                    {
                        validP = new Point3D((double)i, (double)j, this.Data[i, j]);
                        break;
                    }
                }
                for (int k = 0; k < pnts.GetLength(1); k++)
                {
                    if (double.IsNaN(this.Data[i, k]))
                    {
                        pnts[i, k] = validP;
                    }
                    else
                    {
                        pnts[i, k] = new Point3D((double)i, (double)k, this.Data[i, k]);
                        validP = pnts[i, k];
                    }
                }
            }
            return pnts;
        }

        private void AutoAdjustCoordinate()
        {
            if (this.Points.Length != 0)
            {
                this.CalcXYZRange();
                _roundZRange = RoundHelper.CreateRoundedRange(this.ZRange.Min, this.ZRange.Max);
                _axisGrid.XRange = RoundHelper.CreateRoundedRange(this.XRange.Min, this.XRange.Max);
                _axisGrid.YRange = RoundHelper.CreateRoundedRange(this.YRange.Min, this.YRange.Max);
                if (_roundZRange.Min == _roundZRange.Max)
                {
                    _axisGrid.ZRange = new Range<double>(_roundZRange.Max / 2.0, _roundZRange.Max);
                }
                else
                {
                    _axisGrid.ZRange = _roundZRange;
                }
            }
        }

        private void CalcXYZRange()
        {
            double minx = this.Points[0, 0].X;
            double maxX = this.Points[0, 0].X;
            double minY = this.Points[0, 0].Y;
            double maxY = this.Points[0, 0].Y;
            double minZ = this.Points[0, 0].Z;
            double maxZ = this.Points[0, 0].Z;
            for (int i = 0; i <= this.Points.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= this.Points.GetUpperBound(1); j++)
                {
                    if (!double.IsNaN(this.Points[i, j].X))
                    {
                        if (double.IsNaN(minx))
                        {
                            minx = this.Points[i, j].X;
                        }
                        if (double.IsNaN(maxX))
                        {
                            maxX = this.Points[i, j].X;
                        }
                        if (this.Points[i, j].X < minx)
                        {
                            minx = this.Points[i, j].X;
                        }
                        else if (this.Points[i, j].X > maxX)
                        {
                            maxX = this.Points[i, j].X;
                        }
                    }
                    if (!double.IsNaN(this.Points[i, j].Y))
                    {
                        if (double.IsNaN(minY))
                        {
                            minY = this.Points[i, j].Y;
                        }
                        if (double.IsNaN(maxY))
                        {
                            maxY = this.Points[i, j].Y;
                        }
                        if (this.Points[i, j].Y < minY)
                        {
                            minY = this.Points[i, j].Y;
                        }
                        else if (this.Points[i, j].Y > maxY)
                        {
                            maxY = this.Points[i, j].Y;
                        }
                    }
                    if (!double.IsNaN(this.Points[i, j].Z))
                    {
                        if (double.IsNaN(minZ))
                        {
                            minZ = this.Points[i, j].Z;
                        }
                        if (double.IsNaN(maxZ))
                        {
                            maxZ = this.Points[i, j].Z;
                        }
                        if (this.Points[i, j].Z < minZ)
                        {
                            minZ = this.Points[i, j].Z;
                        }
                        else if (this.Points[i, j].Z > maxZ)
                        {
                            maxZ = this.Points[i, j].Z;
                        }
                    }
                }
            }
            this.XRange = new Range<double>(minx, maxX);
            this.YRange = new Range<double>(minY, maxY);
            this.ZRange = new Range<double>(minZ, maxZ);
        }

        private void FindNearPoint(MouseButtonEventArgs e)
        {
            Point location = e.GetPosition(_viewport3D.Viewport);
            _minimumDistance = double.MaxValue;
            ////Perform visual hit testing
            VisualTreeHelper.HitTest(_viewport3D.Viewport, null, new HitTestResultCallback(this.HitTestCallback), new PointHitTestParameters(location));
            if (_minimumDistance < double.MaxValue)
            {
                _dataPointTip.Children.Clear();
                Point3D pt = _curveModel.GetNearestPoint(_nearestPt, _rayhit.VertexIndex1);
                this.DrawDataTip(pt, _nearestNormal);
            }
        }

        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            RayMeshGeometry3DHitTestResult rayHit = result as RayMeshGeometry3DHitTestResult;
            ProjectionCamera camera = _viewport3D.Viewport.Camera as ProjectionCamera;
            HitTestResultBehavior result2;
            if (rayHit != null && rayHit.ModelHit != null)
            {
                Model3D model = rayHit.ModelHit;
                Visual3D visual3D = rayHit.VisualHit;
                if (visual3D != null)
                {
                    if (string.Compare(Convert.ToString(visual3D.GetValue(FrameworkElement.NameProperty)), "CurveVisual") != 0)
                    {
                        result2 = HitTestResultBehavior.Continue;
                        return result2;
                    }
                }
                MeshGeometry3D mesh = rayHit.MeshHit;
                if (mesh != null)
                {
                    Point3D p = mesh.Positions[rayHit.VertexIndex1];
                    Point3D p2 = mesh.Positions[rayHit.VertexIndex2];
                    Point3D p3 = mesh.Positions[rayHit.VertexIndex3];
                    double x = p.X * rayHit.VertexWeight1 + p2.X * rayHit.VertexWeight2 + p3.X * rayHit.VertexWeight3;
                    double y = p.Y * rayHit.VertexWeight1 + p2.Y * rayHit.VertexWeight2 + p3.Y * rayHit.VertexWeight3;
                    double z = p.Z * rayHit.VertexWeight1 + p2.Z * rayHit.VertexWeight2 + p3.Z * rayHit.VertexWeight3;

                    // point in local coordinates
                    Point3D localPoint = new Point3D(x, y, z);
                    Point3D p4 = localPoint;

                    // transform to global coordinates

                    // first transform the Model3D hierarchy
                    GeneralTransform3D t2 = Viewport3DHelper.GetTransform(rayHit.VisualHit, rayHit.ModelHit);
                    if (t2 != null)
                    {
                        p4 = t2.Transform(p4);
                    }

                    // then transform the Visual3D hierarchy up to the Viewport3D ancestor
                    GeneralTransform3D t3 = Viewport3DHelper.GetTransform(_viewport3D.Viewport, rayHit.VisualHit);
                    if (t3 != null)
                    {
                        p4 = t3.Transform(p4);
                    }
                    double distance = (camera.Position - p4).LengthSquared;
                    if (distance < _minimumDistance)
                    {
                        _minimumDistance = distance;
                        _nearestPt = localPoint;
                        _nearestNormal = Vector3D.CrossProduct(p2 - p, p3 - p);
                        _rayhit = rayHit;
                    }
                }
            }
            result2 = HitTestResultBehavior.Continue;
            return result2;
        }

        private void DrawDataTip(Point3D nearPnt, Vector3D normal)
        {
            PointsVisual3D pntVisual = new PointsVisual3D();
            pntVisual.Points.Add(nearPnt);
            pntVisual.Color = Colors.Black;
            pntVisual.Size = 10.0;
            pntVisual.DepthOffset = 0.0001;
            _dataPointTip.Children.Add(pntVisual);
            _ptToolTip.Visibility = Visibility.Visible;
            _ptToolTip.Point = nearPnt;
            _ptToolTip.AttachedVisual = _curveModel;
        }

        private void OnMOuseLeftBtnDown(object sender, MouseButtonEventArgs e)
        {
            ClickModes clickMode = _clickMode;
            if (clickMode != ClickModes.Normal)
            {
                if (clickMode == ClickModes.DataCursor)
                {
                    this.FindNearPoint(e);
                }
            }
            else if (!_showPointGraph)
            {
                _viewport3D.Children.Remove(_axisGrid);
                _viewport3D.Children.Remove(_curveModel);
                _viewport3D.Children.Remove(_wireFrame);
                _viewport3D.Children.Remove(_mesh3D);
                _viewport3D.Children.Add(_pntsModel);
                _showPointGraph = true;
                _dataTipVsisible = _ptToolTip.Visibility;
                _ptToolTip.Visibility = Visibility.Hidden;
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_clickMode == ClickModes.Normal)
            {
                if (_showPointGraph)
                {
                    if (_showMesh)
                    {
                        _viewport3D.Children.Add(_mesh3D);
                    }
                    else
                    {
                        _viewport3D.Children.Add(_curveModel);
                        _viewport3D.Children.Add(_wireFrame);
                    }
                    _viewport3D.Children.Add(_axisGrid);
                    _viewport3D.Children.Remove(_pntsModel);
                    _ptToolTip.Visibility = _dataTipVsisible;
                    _showPointGraph = false;
                }
            }
        }

        private void OnMenuZoomCommand(object sender, ExecutedRoutedEventArgs args)
        {
            if (Convert.ToString(args.Parameter) == "1")
            {
                this.SetZoomInMode();
            }
            else
            {
                this.SetZoomOutMode();
            }
        }

        private void OnMenuNormalCommand(object sender, ExecutedRoutedEventArgs args)
        {
            this.SetNormalMode();
        }

        private void OnMenuDataCursorCommand(object sender, ExecutedRoutedEventArgs args)
        {
            this.SetCusorMode();
        }

        private void OnPanCommand(object sender, ExecutedRoutedEventArgs args)
        {
            this.SetPanMode();
        }

        private void OnResetViewCommand(object sender, ExecutedRoutedEventArgs args)
        {
            _clickMode = ClickModes.Normal;
            base.Cursor = Cursors.Arrow;
            this.ResetGestureToNone();
            _viewport3D.RotateGesture = new MouseGesture(MouseAction.LeftClick);
            _viewport3D.ResetCamera();
            _dataPointTip.Children.Clear();
            _ptToolTip.Visibility = Visibility.Hidden;
            _showMesh = false;
        }

        private void OnMeshViewCommand(object sender, ExecutedRoutedEventArgs args)
        {
            this.ShowMesh();
        }

        private void OnViewXYZCommand(object sender, ExecutedRoutedEventArgs args)
        {
            string cmdPar = Convert.ToString(args.Parameter);
            if (cmdPar == "1")
            {
                _viewport3D.CameraController.GotoXYView();
            }
            else if (cmdPar == "2")
            {
                _viewport3D.CameraController.GotoYZView();
            }
            else if (cmdPar == "3")
            {
                _viewport3D.CameraController.GotoXZView();
            }
        }



        private void ResetGestureToNone()
        {
            _viewport3D.RotateGesture = new MouseGesture(MouseAction.None);
            _viewport3D.ZoomGesture = new MouseGesture(MouseAction.None);
            _viewport3D.PanGesture = new MouseGesture(MouseAction.None);
        }

        private void SetNormalMode()
        {
            _clickMode = ClickModes.Normal;
            base.Cursor = Cursors.Arrow;
            this.ResetGestureToNone();
            _viewport3D.RotateGesture = new MouseGesture(MouseAction.LeftClick);
        }

        private void SetZoomInMode()
        {
            this.ResetGestureToNone();
            _viewport3D.ZoomGesture = new MouseGesture(MouseAction.LeftClick);
            _clickMode = ClickModes.ZoomIn;
            base.Cursor = ChartPlotter3D.cursorZoomIn;
            _viewport3D.CameraController.ZoomIn = true;
        }

        private void SetZoomOutMode()
        {
            this.ResetGestureToNone();
            _viewport3D.ZoomGesture = new MouseGesture(MouseAction.LeftClick);
            _clickMode = ClickModes.ZoomOut;
            base.Cursor = ChartPlotter3D.cursorZoomOut;
            _viewport3D.CameraController.ZoomIn = false;
        }

        private void SetCusorMode()
        {
            _clickMode = ClickModes.DataCursor;
            base.Cursor = Cursors.Cross;
            this.ResetGestureToNone();
        }

        private void SetPanMode()
        {
            base.Cursor = Cursors.Hand;
            this.ResetGestureToNone();
            _viewport3D.PanGesture = new MouseGesture(MouseAction.LeftClick);
        }

        public void ShowMesh()
        {
            _showMesh = true;
            if (this.Points != null)
            {
                _dataPointTip.Children.Clear();
                _ptToolTip.Visibility = Visibility.Hidden;
                _viewport3D.Children.Remove(_curveModel);
                _viewport3D.Children.Remove(_wireFrame);
                _viewport3D.Children.Remove(_mesh3D);
                _viewport3D.Children.Add(_mesh3D);
            }
        }

        public void ShowSurface()
        {
            if (_showMesh)
            {
                if (this.Points != null)
                {
                    _viewport3D.Children.Remove(_curveModel);
                    _viewport3D.Children.Remove(_wireFrame);
                    _viewport3D.Children.Remove(_mesh3D);
                    _viewport3D.Children.Add(_curveModel);
                    _viewport3D.Children.Add(_wireFrame);
                }
                _showMesh = false;
            }
        }
    }
}