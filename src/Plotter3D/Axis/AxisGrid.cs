using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows.Media;
using Plotter3D.Common;
using System.Windows;
using System.Windows.Controls;
using Plotter3D.Axis;
using System.Diagnostics;

namespace Plotter3D
{

    /// <summary>
    /// In charge of drawing Axises,Grid lines,ticks and tick labels
    /// </summary>
	public class AxisGrid : RenderingModelVisual3D
    {
        internal class AxisDash
        {
            public Point3D VP0;

            public Point3D VP1;

            public Point3D HP0;

            public Point3D HP1;

            public Vector3D HVector;

            public Vector3D VVector;

            public int VDashCount;

            public int HDashCount;

            public PointsVisual3D Points3D;
        }

        internal enum TickCountChange
        {
            Decrease,
            Increase,
            OK
        }

        private const int MaxDashCount = 1000;

        private double _length = 5.0;

        private int _dashCount = 50;

        private double _dashGap = 8.0;

        private bool _isRendering = false;

        private Color _lineColor = Colors.Black;

        private double _axisLabelDistance = 1.5;

        private double _ticklabelHeight = 0.2;

        private double _axisTitleHeight = 0.3;

        private bool updateForcely = false;

        private int _xAxisTickCnt = 5;

        private int _yAxisTickCnt = 5;

        private int _zAxisTickCnt = 5;

        private double _tickScreenHeight = 12.0;

        private HelixViewport3D _viewport = null;

        private ModelVisual3D _axisTickLabelVisuals = new ModelVisual3D();

        private Model3DGroup _mayorTickLabelGroup = new Model3DGroup();

        private ModelVisual3D _axisTitleGroup = new ModelVisual3D();

        private List<TickInfo> _xAxisMayorTicks = new List<TickInfo>(25);

        private List<TickInfo> _yAxisMayorTicks = new List<TickInfo>(25);

        private List<TickInfo> _zAxisMayorTicks = new List<TickInfo>(25);

        private List<TickInfo> _minorTickBuf = new List<TickInfo>(100);

        private Transform3D _dataAxisTransform = Transform3D.Identity;

        private ModelVisual3D _xozGrid = new ModelVisual3D();

        private ModelVisual3D _yozGrid = new ModelVisual3D();

        private ModelVisual3D _xoYGrid = new ModelVisual3D();

        private ModelVisual3D _yozZAxisTicks = new ModelVisual3D();

        private ModelVisual3D _xozZAxisTicks = new ModelVisual3D();

        private ModelVisual3D _xoyXAxisTicks = new ModelVisual3D();

        private ModelVisual3D _xozXAxisTicks = new ModelVisual3D();

        private ModelVisual3D _yozYAxisTicks = new ModelVisual3D();

        private ModelVisual3D _xoyYAxisTicks = new ModelVisual3D();

        private AxisGrid.AxisDash _xoyDash = new AxisGrid.AxisDash();

        private AxisGrid.AxisDash _xozDash = new AxisGrid.AxisDash();

        private AxisGrid.AxisDash _yozDash = new AxisGrid.AxisDash();

        public static readonly DependencyProperty TicksProviderProperty = DependencyProperty.Register("TicksProvider", typeof(ITickProvider), typeof(AxisGrid), new UIPropertyMetadata(new NumericTicksProvider(6, 4, 0.2, 0.1), new PropertyChangedCallback(AxisGrid.PropertyChanged)));

        public static readonly DependencyProperty XRangeProperty = DependencyProperty.Register("XRange", typeof(Range<double>), typeof(AxisGrid), new UIPropertyMetadata(new Range<double>(0.0, 5.0), new PropertyChangedCallback(AxisGrid.PropertyChanged)));

        public static readonly DependencyProperty YRangeProperty = DependencyProperty.Register("YRange", typeof(Range<double>), typeof(AxisGrid), new UIPropertyMetadata(new Range<double>(0.0, 5.0), new PropertyChangedCallback(AxisGrid.PropertyChanged)));

        public static readonly DependencyProperty ZRangeProperty = DependencyProperty.Register("ZRange", typeof(Range<double>), typeof(AxisGrid), new UIPropertyMetadata(new Range<double>(0.0, 5.0), new PropertyChangedCallback(AxisGrid.PropertyChanged)));

        public static readonly DependencyProperty ShowAxisLabelProperty = DependencyProperty.Register("ShowAxisLabel", typeof(bool), typeof(AxisGrid), new UIPropertyMetadata(true, new PropertyChangedCallback(AxisGrid.PropertyChanged)));

        private bool _eventSubscribed = false;

        private ModelVisual3D _xozXAxisTickLabels = new ModelVisual3D();

        private ModelVisual3D _xoyXAxisTickLabels = new ModelVisual3D();

        private ModelVisual3D _xozXAxisTitle;

        private ModelVisual3D _xoyXAxisTitle;

        private double _maxXAxisTickWidth = 0.0;

        private ModelVisual3D _yozYAxisTickLabels = new ModelVisual3D();

        private ModelVisual3D _xoyYAxisTickLabels = new ModelVisual3D();

        private ModelVisual3D _yozYAxisTitle;

        private ModelVisual3D _xoyYAxisTitle;

        private double _maxYAxisTickWidth = 0.0;

        private ModelVisual3D _yozZAxisTickLabels = new ModelVisual3D();

        private ModelVisual3D _xozZAxisTickLabels = new ModelVisual3D();

        private ModelVisual3D _xozZAxisTitle;

        private ModelVisual3D _yozZAxisTitle;

        private double _maxZAxisTickWidth = 0.0;

        private int count = 0;

        private Matrix3D _visualToScreen = Matrix3D.Identity;

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

        public Range<double> XRange
        {
            get
            {
                return (Range<double>)base.GetValue(AxisGrid.XRangeProperty);
            }
            set
            {
                base.SetValue(AxisGrid.XRangeProperty, value);
            }
        }

        public Range<double> YRange
        {
            get
            {
                return (Range<double>)base.GetValue(AxisGrid.YRangeProperty);
            }
            set
            {
                base.SetValue(AxisGrid.YRangeProperty, value);
            }
        }

        public Range<double> ZRange
        {
            get
            {
                return (Range<double>)base.GetValue(AxisGrid.ZRangeProperty);
            }
            set
            {
                base.SetValue(AxisGrid.ZRangeProperty, value);
            }
        }

        public bool ShowAxisLabel
        {
            get
            {
                return (bool)base.GetValue(AxisGrid.ShowAxisLabelProperty);
            }
            set
            {
                base.SetValue(AxisGrid.ShowAxisLabelProperty, value);
            }
        }

        public ITickProvider TicksProvider
        {
            get
            {
                return (ITickProvider)base.GetValue(AxisGrid.TicksProviderProperty);
            }
            set
            {
                base.SetValue(AxisGrid.TicksProviderProperty, value);
            }
        }

        public Transform3D DataToAxisTransform
        {
            get
            {
                if (_dataAxisTransform == Transform3D.Identity)
                {
                    Transform3DGroup transformGroup = new Transform3DGroup();
                    TranslateTransform3D transform = new TranslateTransform3D
                    {
                        OffsetX = 0.0 - this.XRange.Min,
                        OffsetY = 0.0 - this.YRange.Min,
                        OffsetZ = 0.0 - this.ZRange.Min
                    };
                    transformGroup.Children.Add(transform);
                    ScaleTransform3D transform2 = new ScaleTransform3D();
                    transform2.ScaleX = _length / (this.XRange.Max - this.XRange.Min);
                    transform2.ScaleY = _length / (this.YRange.Max - this.YRange.Min);
                    transform2.ScaleZ = _length / (this.ZRange.Max - this.ZRange.Min);
                    transformGroup.Children.Add(transform2);
                    _dataAxisTransform = transformGroup;
                }
                return _dataAxisTransform;
            }
        }

        public HelixViewport3D Viewport
        {
            get
            {
                return _viewport;
            }
            set
            {
                _viewport = value;
                this.SubscribeViewportEvent();
            }
        }

        public AxisGrid()
        {
            base.Content = _mayorTickLabelGroup;
            base.SubscribeToRenderingEvent();
            this.InitAxisDash();
        }

        private void InitAxisDash()
        {
            _xoyDash.VP0 = new Point3D(0.0, 0.0, 0.0);
            _xoyDash.VP1 = new Point3D(_length, 0.0, 0.0);
            _xoyDash.HP0 = new Point3D(0.0, 0.0, 0.0);
            _xoyDash.HP1 = new Point3D(0.0, 0.0, _length);
            _xoyDash.VVector = new Vector3D(1.0, 0.0, 0.0);
            _xoyDash.HVector = new Vector3D(0.0, 1.0, 0.0);
            _xoyDash.VDashCount = _dashCount;
            _xoyDash.HDashCount = _dashCount;
            _xoyDash.Points3D = new PointsVisual3D();
            _xozDash.VP0 = new Point3D(0.0, 0.0, 0.0);
            _xozDash.VP1 = new Point3D(0.0, 0.0, _length);
            _xozDash.HP0 = new Point3D(0.0, 0.0, 0.0);
            _xozDash.HP1 = new Point3D(_length, 0.0, 0.0);
            _xozDash.VVector = new Vector3D(0.0, 0.0, 1.0);
            _xozDash.HVector = new Vector3D(1.0, 0.0, 0.0);
            _xozDash.VDashCount = _dashCount;
            _xozDash.HDashCount = _dashCount;
            _xozDash.Points3D = new PointsVisual3D();
            _yozDash.VP0 = new Point3D(0.0, 0.0, 0.0);
            _yozDash.VP1 = new Point3D(0.0, 0.0, _length);
            _yozDash.HP0 = new Point3D(0.0, 0.0, 0.0);
            _yozDash.HP1 = new Point3D(0.0, _length, 0.0);
            _yozDash.VVector = new Vector3D(0.0, 0.0, 1.0);
            _yozDash.HVector = new Vector3D(0.0, 1.0, 0.0);
            _yozDash.VDashCount = _dashCount;
            _yozDash.HDashCount = _dashCount;
            _yozDash.Points3D = new PointsVisual3D();
        }

        private void SubscribeViewportEvent()
        {
            if (!_eventSubscribed)
            {
                _viewport.CameraController.Zoomed += new RoutedEventHandler(this.CameraController_Zoomed);
                _viewport.CameraController.Rotated += new RoutedEventHandler(this.CameraController_Rotated);
            }
        }

        private void CameraController_Rotated(object sender, RoutedEventArgs e)
        {
            this.ChecklabelArragement();
        }

        private void CameraController_Zoomed(object sender, RoutedEventArgs e)
        {
            this.ChecklabelArragement();
        }

        protected static void PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AxisGrid axisGrid = (AxisGrid)sender;
            axisGrid.Reset();
        }

        private void Reset()
        {
            _xAxisTickCnt = this.TicksProvider.MayorTickCount;
            _yAxisTickCnt = this.TicksProvider.MayorTickCount;
            _zAxisTickCnt = this.TicksProvider.MayorTickCount;
            this.ResetTransform();
            this.ClearContent();
            this.DrawAxisGrid();
            this.updateForcely = true;
        }

        private void ResetTransform()
        {
            _dataAxisTransform = Transform3D.Identity;
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            _isRendering = (parent != null);
        }

        private void ClearContent()
        {
            base.Children.Clear();
            _axisTitleGroup.Children.Clear();
            _mayorTickLabelGroup.Children.Clear();
            _mayorTickLabelGroup = new Model3DGroup();
            _xozXAxisTickLabels.Children.Clear();
            _xoyXAxisTickLabels.Children.Clear();
            _yozYAxisTickLabels.Children.Clear();
            _xoyYAxisTickLabels.Children.Clear();
            _yozZAxisTickLabels.Children.Clear();
            _xozZAxisTickLabels.Children.Clear();
            _xoYGrid.Children.Clear();
            _xozGrid.Children.Clear();
            _yozGrid.Children.Clear();
            _axisTickLabelVisuals.Children.Clear();
        }

        private void DrawAxisGrid()
        {
            this.CreateAxisTicks();
            this.CreateDashLine();
            _xoYGrid.Children.Add(_xoyDash.Points3D);
            _xozGrid.Children.Add(_xozDash.Points3D);
            _yozGrid.Children.Add(_yozDash.Points3D);
            base.Children.Add(_xoYGrid);
            base.Children.Add(_xozGrid);
            base.Children.Add(_yozGrid);

            if (this.ShowAxisLabel)
            {
                this.CreateXAxisLabels();
                this.CreateYAxisLabels();
                this.CreateZAxisLabels();
                base.Children.Add(_axisTitleGroup);
                base.Children.Add(_axisTickLabelVisuals);
            }
        }

        private void CreateAxisTicks()
        {
            this.TicksProvider.MayorTickCount = _xAxisTickCnt;
            this.TicksProvider.CreateMinorTicks(_minorTickBuf, this.XRange, _length);
            this.TicksProvider.CreateMayorTicks(_xAxisMayorTicks, this.XRange, _length);
            _xozXAxisTicks = this.CreateAxisTicks(_minorTickBuf, _xAxisMayorTicks, new Point3D(0.0, 0.0, _length), new Vector3D(0.0, 0.0, 1.0), new Vector3D(1.0, 0.0, 0.0));
            _axisTickLabelVisuals.Children.Add(_xozXAxisTicks);
            _xoyXAxisTicks = this.CreateAxisTicks(_minorTickBuf, _xAxisMayorTicks, new Point3D(0.0, _length, 0.0), new Vector3D(0.0, 1.0, 0.0), new Vector3D(1.0, 0.0, 0.0));
            _axisTickLabelVisuals.Children.Add(_xoyXAxisTicks);
            this.TicksProvider.MayorTickCount = _yAxisTickCnt;
            this.TicksProvider.CreateMinorTicks(_minorTickBuf, this.YRange, _length);
            this.TicksProvider.CreateMayorTicks(_yAxisMayorTicks, this.YRange, _length);
            _xoyYAxisTicks = this.CreateAxisTicks(_minorTickBuf, _yAxisMayorTicks, new Point3D(_length, 0.0, 0.0), new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, 1.0, 0.0));
            _axisTickLabelVisuals.Children.Add(_xoyYAxisTicks);
            _yozYAxisTicks = this.CreateAxisTicks(_minorTickBuf, _yAxisMayorTicks, new Point3D(0.0, 0.0, _length), new Vector3D(0.0, 0.0, 1.0), new Vector3D(0.0, 1.0, 0.0));
            _axisTickLabelVisuals.Children.Add(_yozYAxisTicks);
            this.TicksProvider.MayorTickCount = _zAxisTickCnt;
            this.TicksProvider.CreateMinorTicks(_minorTickBuf, this.ZRange, _length);
            this.TicksProvider.CreateMayorTicks(_zAxisMayorTicks, this.ZRange, _length);
            _xozZAxisTicks = this.CreateAxisTicks(_minorTickBuf, _zAxisMayorTicks, new Point3D(_length, 0.0, 0.0), new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            _axisTickLabelVisuals.Children.Add(_xozZAxisTicks);
            _yozZAxisTicks = this.CreateAxisTicks(_minorTickBuf, _zAxisMayorTicks, new Point3D(0.0, _length, 0.0), new Vector3D(0.0, 1.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            _axisTickLabelVisuals.Children.Add(_yozZAxisTicks);
        }

        private LinesVisual3D CreateAxisTicks(List<TickInfo> minorTicks, List<TickInfo> mayorTicks, Point3D startPt, Vector3D lineDirection, Vector3D tickGrowVec)
        {
            Point3D[] pntsArray = new Point3D[minorTicks.Count * 2 + mayorTicks.Count * 2];
            int i = 0;
            while (i < minorTicks.Count && minorTicks.Count > 1)
            {
                Point3D p0 = startPt + tickGrowVec * minorTicks[i].AxisValue;
                Point3D p = p0 + lineDirection * this.TicksProvider.MinorTickSize;
                pntsArray[i * 2] = p0;
                pntsArray[i * 2 + 1] = p;
                i++;
            }
            i = 0;
            while (i < mayorTicks.Count && mayorTicks.Count > 1)
            {
                Point3D p0 = startPt + tickGrowVec * mayorTicks[i].AxisValue;
                Point3D p = p0 + lineDirection * this.TicksProvider.MayorTickSize;
                pntsArray[(minorTicks.Count + i) * 2] = p0;
                pntsArray[(minorTicks.Count + i) * 2 + 1] = p;
                i++;
            }
            return new LinesVisual3D
            {
                Color = _lineColor,
                Thickness = 1.0,
                Points = pntsArray
            };
        }

        private void CreateDashLine()
        {
            this.CreateDashLine(_xoyDash, _xAxisMayorTicks, _yAxisMayorTicks);
            this.CreateDashLine(_xozDash, _zAxisMayorTicks, _xAxisMayorTicks);
            this.CreateDashLine(_yozDash, _zAxisMayorTicks, _yAxisMayorTicks);
        }

        private void CreateDashLine(AxisGrid.AxisDash axisdash, List<TickInfo> horTicks, List<TickInfo> verTicks)
        {
            Point3DCollection dashPnts = new Point3DCollection();
            double hGap = _length / (double)axisdash.HDashCount;
            double vGap = _length / (double)axisdash.VDashCount;
            Vector3D hV = hGap * axisdash.HVector;
            Vector3D vV = vGap * axisdash.VVector;
            Point3D originalPnt = new Point3D(0.0, 0.0, 0.0);
            for (int i = 1; i < horTicks.Count - 1; i++)
            {
                for (int j = 0; j <= axisdash.HDashCount; j++)
                {
                    dashPnts.Add(originalPnt + hV * (double)j + horTicks[i].AxisValue * axisdash.VVector);
                }
            }
            for (int i = 1; i < verTicks.Count - 1; i++)
            {
                for (int j = 0; j <= axisdash.VDashCount; j++)
                {
                    dashPnts.Add(originalPnt + vV * (double)j + verTicks[i].AxisValue * axisdash.HVector);
                }
            }
            axisdash.Points3D.Color = Colors.Blue;
            axisdash.Points3D.Points = dashPnts;
        }

        private void CreateXAxisLabels()
        {
            foreach (TickInfo tick in _xAxisMayorTicks)
            {
                TickLabelVisual3D xozMayorTickLabel = new TickLabelVisual3D(this.TicksProvider.GetLabelText(tick), Brushes.Black, true, _ticklabelHeight, new Point3D(tick.AxisValue, -0.2, _length + 0.2), Location.End, new Vector3D(0.0, 1.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                TickLabelVisual3D xoyMayorTickLabel = new TickLabelVisual3D(this.TicksProvider.GetLabelText(tick), Brushes.Black, true, _ticklabelHeight, new Point3D(tick.AxisValue, _length + 0.2, -0.1), Location.Start, new Vector3D(0.0, 1.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                _xozXAxisTickLabels.Children.Add(xozMayorTickLabel);
                _xoyXAxisTickLabels.Children.Add(xoyMayorTickLabel);
                if (_maxXAxisTickWidth < xozMayorTickLabel.ScreenWidth)
                {
                    _maxXAxisTickWidth = xozMayorTickLabel.ScreenWidth;
                }
            }
            if (_xAxisMayorTicks.Count > 1)
            {
                _xozXAxisTitle = new TickLabelVisual3D("X Axis", Brushes.Black, true, _axisTitleHeight, new Point3D(_length / 2.0, -_axisLabelDistance, _length + 0.5), Location.Center, new Vector3D(0.0, 1.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                _xoyXAxisTitle = new TickLabelVisual3D("X Axis", Brushes.Black, true, _axisTitleHeight, new Point3D(_length / 2.0, _length + _axisLabelDistance, -0.5), Location.Center, new Vector3D(0.0, 1.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                _axisTitleGroup.Children.Add(_xozXAxisTitle);
                _axisTitleGroup.Children.Add(_xoyXAxisTitle);
            }
            _axisTickLabelVisuals.Children.Add(_xozXAxisTickLabels);
            _axisTickLabelVisuals.Children.Add(_xoyXAxisTickLabels);
        }

        private void CreateYAxisLabels()
        {
            for (int i = 0; i < _yAxisMayorTicks.Count; i++)
            {
                TickLabelVisual3D xoyMayorTickLabel = new TickLabelVisual3D(this.TicksProvider.GetLabelText(_yAxisMayorTicks[i]), Brushes.Black, true, _ticklabelHeight, new Point3D(_length + 0.2, _yAxisMayorTicks[i].AxisValue, -0.1), Location.End, new Vector3D(-1.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                TickLabelVisual3D yozMayorTickLabel = new TickLabelVisual3D(this.TicksProvider.GetLabelText(_yAxisMayorTicks[i]), Brushes.Black, true, _ticklabelHeight, new Point3D(0.0, _yAxisMayorTicks[i].AxisValue, _length + 0.2), Location.Start, new Vector3D(-1.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                _yozYAxisTickLabels.Children.Add(yozMayorTickLabel);
                _xoyYAxisTickLabels.Children.Add(xoyMayorTickLabel);
                if (_maxYAxisTickWidth < xoyMayorTickLabel.ScreenWidth)
                {
                    _maxYAxisTickWidth = xoyMayorTickLabel.ScreenWidth;
                }
            }
            if (_yAxisMayorTicks.Count > 1)
            {
                _xoyYAxisTitle = new TickLabelVisual3D("Y Axis", Brushes.Black, true, _axisTitleHeight, new Point3D(_length + _axisLabelDistance, _length / 2.0, -0.5), Location.Center, new Vector3D(0.0, 1.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                _yozYAxisTitle = new TickLabelVisual3D("Y Axis", Brushes.Black, true, _axisTitleHeight, new Point3D(-_axisLabelDistance, _length / 2.0, _length + 0.5), Location.Center, new Vector3D(0.0, 1.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                _axisTitleGroup.Children.Add(_xoyYAxisTitle);
                _axisTitleGroup.Children.Add(_yozYAxisTitle);
            }
            _axisTickLabelVisuals.Children.Add(_yozYAxisTickLabels);
            _axisTickLabelVisuals.Children.Add(_xoyYAxisTickLabels);
        }

        private void CreateZAxisLabels()
        {
            for (int i = 0; i < _zAxisMayorTicks.Count; i++)
            {
                TickLabelVisual3D yozMayorTickLabel = new TickLabelVisual3D(this.TicksProvider.GetLabelText(_zAxisMayorTicks[i]), Brushes.Black, true, _ticklabelHeight, new Point3D(-0.3, _length + 0.1, _zAxisMayorTicks[i].AxisValue), Location.Start, new Vector3D(0.0, 1.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                TickLabelVisual3D xozMayorTickLabel = new TickLabelVisual3D(this.TicksProvider.GetLabelText(_zAxisMayorTicks[i]), Brushes.Black, true, _ticklabelHeight, new Point3D(_length + 0.1, -0.3, _zAxisMayorTicks[i].AxisValue), Location.End, new Vector3D(-1.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                _yozZAxisTickLabels.Children.Add(yozMayorTickLabel);
                _xozZAxisTickLabels.Children.Add(xozMayorTickLabel);
                if (_maxZAxisTickWidth < yozMayorTickLabel.ScreenWidth)
                {
                    _maxZAxisTickWidth = yozMayorTickLabel.ScreenWidth;
                }
            }
            if (_zAxisMayorTicks.Count > 1)
            {
                _yozZAxisTitle = new TickLabelVisual3D("Z Axis", Brushes.Black, true, _axisTitleHeight, new Point3D(-0.1, _length + _axisLabelDistance, _length / 2.0), Location.Center, new Vector3D(0.0, 1.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                _xozZAxisTitle = new TickLabelVisual3D("Z Axis", Brushes.Black, true, _axisTitleHeight, new Point3D(_length + _axisLabelDistance, -0.1, _length / 2.0), Location.Center, new Vector3D(-1.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
                _axisTitleGroup.Children.Add(_xozZAxisTitle);
                _axisTitleGroup.Children.Add(_yozZAxisTitle);
            }
            _axisTickLabelVisuals.Children.Add(_yozZAxisTickLabels);
            _axisTickLabelVisuals.Children.Add(_xozZAxisTickLabels);
        }

        private void AddTransformedPoints(Point3DCollection pntColl, Transform3D transform, Point3D[] points)
        {
            transform.Transform(points);
            pntColl.Add(points);
        }

        private void ChecklabelArragement()
        {
            Debug.WriteLine("ChecklabelArragement" + this.count++);
            this.UpdateTransform();
            bool axisCntChanged = false;
            Point3D p0 = new Point3D(0.0, 0.0, 0.0);
            Point3D px = new Point3D(_length, 0.0, 0.0);
            Point3D py = new Point3D(0.0, _length, 0.0);
            Point3D pz = new Point3D(0.0, 0.0, _length);
            Point3D p = _visualToScreen.Transform(p0);
            Point3D px2 = _visualToScreen.Transform(px);
            Point3D py2 = _visualToScreen.Transform(py);
            Point3D pz2 = _visualToScreen.Transform(pz);
            Vector3D vecx = new Vector3D(Math.Abs(px2.X - p.X), Math.Abs(px2.Y - p.Y), 0.0);
            int fitCnt = this.FittestTicklabelCount(vecx, _xAxisTickCnt, _maxXAxisTickWidth, _tickScreenHeight);
            if (fitCnt != _xAxisTickCnt)
            {
                _xAxisTickCnt = fitCnt;
                axisCntChanged = true;
                Debug.WriteLine("XXX Axis count changed");
            }
            Vector3D vecy = new Vector3D(Math.Abs(py2.X - p.X), Math.Abs(py2.Y - p.Y), 0.0);
            fitCnt = this.FittestTicklabelCount(vecy, _yAxisTickCnt, _maxYAxisTickWidth, _tickScreenHeight);
            if (fitCnt != _yAxisTickCnt)
            {
                _yAxisTickCnt = fitCnt;
                axisCntChanged = true;
                Debug.WriteLine("YYY Axis count changed");
            }
            Vector3D vecz = new Vector3D(Math.Abs(pz2.X - p.X), Math.Abs(pz2.Y - p.Y), 0.0);
            fitCnt = this.FittestTicklabelCount(vecz, _zAxisTickCnt, _maxZAxisTickWidth, _tickScreenHeight);
            if (fitCnt != _zAxisTickCnt)
            {
                _zAxisTickCnt = fitCnt;
                axisCntChanged = true;
                Debug.WriteLine("ZZZ Axis count changed");
            }
            if (axisCntChanged)
            {
                this.UpdateDashlines();
                this.ResetTransform();
                this.ClearContent();
                this.DrawAxisGrid();
                this.Update();
                Debug.WriteLine("ChecklabelArragement Axis count changed");
            }
        }

        private int FittestTicklabelCount(Vector3D vecx, int axisTickCnt, double maxAxisTickWidth, double tickScreenHeight)
        {
            AxisGrid.TickCountChange change = AxisGrid.TickCountChange.OK;
            AxisGrid.TickCountChange preChange = AxisGrid.TickCountChange.OK;
            int preCnt = axisTickCnt;
            int curCnt = preCnt;
            while (preChange != AxisGrid.TickCountChange.Decrease || change != AxisGrid.TickCountChange.Increase)
            {
                if (preChange != AxisGrid.TickCountChange.Increase || change != AxisGrid.TickCountChange.Decrease)
                {
                    preChange = change;
                    preCnt = curCnt;
                    double totalHeigth = tickScreenHeight * (double)curCnt * 3.0;
                    double totalWidth = maxAxisTickWidth * (double)(curCnt - 1);
                    if (totalHeigth < vecx.Y || totalWidth < vecx.X)
                    {
                        curCnt = this.TicksProvider.IncreaseMayroTickCnt(curCnt);
                        change = AxisGrid.TickCountChange.Increase;
                    }
                    else
                    {
                        curCnt = this.TicksProvider.DecreaseMayorTickCnt(curCnt);
                        change = AxisGrid.TickCountChange.Decrease;
                    }
                    if (curCnt == preCnt || curCnt == 0)
                    {
                        curCnt = preCnt;
                    }
                    else if (change != AxisGrid.TickCountChange.OK)
                    {
                        continue;
                    }
                }
                return curCnt;
            }
            curCnt = preCnt;
            return curCnt;
        }

        protected override void OnCompositionTargetRendering(object sender, RenderingEventArgs e)
        {
            if (_isRendering)
            {
                if (this.UpdateTransform() || this.updateForcely)
                {
                    this.Update();
                    this.updateForcely = false;
                }
            }
        }

        private bool UpdateTransform()
        {
            bool result;
            if (this.Viewport == null)
            {
                result = false;
            }
            else
            {
                Matrix3D? transform = Visual3DHelper.GetViewportTransform(this);
                if (!transform.HasValue)
                {
                    result = false;
                }
                else if (_visualToScreen == transform.Value)
                {
                    result = false;
                }
                else
                {
                    _visualToScreen = transform.Value;
                    result = true;
                }
            }
            return result;
        }

        private void UpdateDashlines()
        {
            this.UpdateDashLine(_xoyDash, _xAxisTickCnt, _yAxisTickCnt);
            this.UpdateDashLine(_xozDash, _zAxisTickCnt, _xAxisTickCnt);
            this.UpdateDashLine(_yozDash, _zAxisTickCnt, _yAxisTickCnt);
        }

        private void UpdateDashLine(AxisGrid.AxisDash axisDash, int horTicksCnt, int verTicksCnt)
        {
            if (horTicksCnt >= 2 && verTicksCnt >= 2)
            {
                double scrnLength = this.GetScreenLength(axisDash.HP0, axisDash.HP1);
                double cnt = Math.Floor(scrnLength / (double)(verTicksCnt - 1) / _dashGap) * (double)(verTicksCnt - 1);
                if (cnt < 1000.0)
                {
                    double portion = cnt / (double)axisDash.HDashCount;
                    if (portion < 0.8 || portion > 1.2)
                    {
                        axisDash.HDashCount = (int)cnt;
                    }
                }
                scrnLength = this.GetScreenLength(axisDash.VP0, axisDash.VP1);
                cnt = Math.Floor(scrnLength / (double)(horTicksCnt - 1) / _dashGap) * (double)(horTicksCnt - 1);
                if (cnt < 1000.0)
                {
                    double portion = cnt / (double)axisDash.VDashCount;
                    if (portion < 0.8 || portion > 1.2)
                    {
                        axisDash.VDashCount = (int)cnt;
                    }
                }
            }
        }

        private bool XoZOverCamera
        {
            get
            {
                var camera = this.Viewport.Camera;
                Vector3D xAixs = new Vector3D(1.0, 0.0, 0.0);
                return (camera.LookDirection - xAixs).Y > 0.0;
            }
        }
        private bool YoZOverCamera
        {
            get
            {
                var camera = this.Viewport.Camera;
                Vector3D yAxis = new Vector3D(0.0, 1.0, 0.0);
                return (camera.LookDirection - yAxis).X > 0.0;
            }
        }

        private bool XoYOverCamera
        {
            get
            {
                var camera = this.Viewport.Camera;
                Vector3D xAixs = new Vector3D(1.0, 0.0, 0.0);
                return (camera.LookDirection - xAixs).Z > 0.0;
            }
        }

        private void Update()
        {
            if (this.ShowAxisLabel)
            {
                UpdateAxisLabel();
            }

            UpdateGrid();
            UpdateAxisTicks();
        }

        private void UpdateAxisLabel()
        {
            TickLabelVisual3D xoyTickLabel = (TickLabelVisual3D)_xoyXAxisTickLabels.Children[0];
            TickLabelVisual3D xozTickLabel = (TickLabelVisual3D)_xozXAxisTickLabels.Children[0];
            TickLabelVisual3D yozTickLabel = (TickLabelVisual3D)_yozYAxisTickLabels.Children[0];
            if (this.XoZOverCamera && !this.XoYOverCamera)
            {
                _xoyXAxisTickLabels.Transform = new TranslateTransform3D(0.0, -_length - xoyTickLabel.ModelWidth - 0.4, 0.0);
                _xozXAxisTickLabels.Transform = new TranslateTransform3D(0.0, _length + xozTickLabel.ModelWidth + 0.4, 0.0);
                _xoyXAxisTitle.Transform = new TranslateTransform3D(0.0, -_length - _axisLabelDistance * 2.0, 0.0);
                _xozXAxisTitle.Transform = new TranslateTransform3D(0.0, _length + 2.0 * _axisLabelDistance, 0.0);
            }
            else if (!this.XoZOverCamera && this.XoYOverCamera)
            {
                _xoyXAxisTickLabels.Transform = new TranslateTransform3D(0.0, 0.0, _length + 0.2);
                _xozXAxisTickLabels.Transform = new TranslateTransform3D(0.0, 0.0, -_length - 0.4);
                _xoyXAxisTitle.Transform = new TranslateTransform3D(0.0, 0.0, _length + 1.0);
                _xozXAxisTitle.Transform = new TranslateTransform3D(0.0, 0.0, -_length - 1.0);
            }
            else
            {
                _xozXAxisTickLabels.Transform = Transform3D.Identity;
                _xoyXAxisTickLabels.Transform = Transform3D.Identity;
                _xoyXAxisTitle.Transform = Transform3D.Identity;
                _xozXAxisTitle.Transform = Transform3D.Identity;
            }
            if (this.YoZOverCamera && !this.XoYOverCamera)
            {
                _yozYAxisTickLabels.Transform = new TranslateTransform3D(_length + 0.4 + yozTickLabel.ModelWidth, 0.0, 0.0);
                _xoyYAxisTickLabels.Transform = new TranslateTransform3D(-_length - 0.4 - xoyTickLabel.ModelWidth, 0.0, 0.0);
                _yozYAxisTitle.Transform = new TranslateTransform3D(_length + 2.0 * _axisLabelDistance, 0.0, 0.0);
                _xoyYAxisTitle.Transform = new TranslateTransform3D(-_length - 2.0 * _axisLabelDistance, 0.0, 0.0);
            }
            else if (!this.YoZOverCamera && this.XoYOverCamera)
            {
                _yozYAxisTickLabels.Transform = new TranslateTransform3D(0.0, 0.0, -_length - 0.4);
                _xoyYAxisTickLabels.Transform = new TranslateTransform3D(0.0, 0.0, _length + 0.2);
                _yozYAxisTitle.Transform = new TranslateTransform3D(0.0, 0.0, -_length - 1.0);
                _xoyYAxisTitle.Transform = new TranslateTransform3D(0.0, 0.0, _length + 1.0);
            }
            else
            {
                _yozYAxisTickLabels.Transform = Transform3D.Identity;
                _xoyYAxisTickLabels.Transform = Transform3D.Identity;
                _xoyYAxisTitle.Transform = Transform3D.Identity;
                _yozYAxisTitle.Transform = Transform3D.Identity;
            }
            if ((this.XoZOverCamera && !this.YoZOverCamera) || (!this.XoZOverCamera && this.YoZOverCamera))
            {
                _xozZAxisTickLabels.Transform = new TranslateTransform3D(0.0, _length + 0.6, 0.0);
                _yozZAxisTickLabels.Transform = new TranslateTransform3D(0.0, -_length - 1.0, 0.0);
                _xozZAxisTitle.Transform = new TranslateTransform3D(0.0, _length + 1.0, 0.0);
                _yozZAxisTitle.Transform = new TranslateTransform3D(0.0, -_length - 2.0 * _axisLabelDistance, 0.0);
            }
            else
            {
                _yozZAxisTickLabels.Transform = Transform3D.Identity;
                _xozZAxisTickLabels.Transform = Transform3D.Identity;
                _yozZAxisTitle.Transform = Transform3D.Identity;
                _xozZAxisTitle.Transform = Transform3D.Identity;
            }
        }

        private void UpdateGrid()
        {
            if (this.XoZOverCamera)
            {
                _xozGrid.Transform = new TranslateTransform3D(new Vector3D(0.0, _length, 0.0));
            }
            else
            {
                _xozGrid.Transform = Transform3D.Identity;
            }

            if (this.YoZOverCamera)
            {
                _yozGrid.Transform = new TranslateTransform3D(new Vector3D(_length, 0.0, 0.0));
            }
            else
            {
                _yozGrid.Transform = Transform3D.Identity;
            }

            if (this.XoYOverCamera)
            {
                _xoYGrid.Transform = new TranslateTransform3D(new Vector3D(0.0, 0.0, _length));
            }
            else
            {
                _xoYGrid.Transform = Transform3D.Identity;
            }
        }

        private void UpdateAxisTicks()
        {
            if (this.XoZOverCamera && !this.XoYOverCamera)
            {
                _xoyXAxisTicks.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1.0, 0.0, 0.0), 180.0), new Point3D(0.0, _length / 2.0, 0.0));
                _xozXAxisTicks.Transform = new TranslateTransform3D(0.0, _length, 0.0);
            }
            else if (!this.XoZOverCamera && this.XoYOverCamera)
            {
                _xoyXAxisTicks.Transform = new TranslateTransform3D(0.0, 0.0, _length);
                _xozXAxisTicks.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1.0, 0.0, 0.0), 180.0), new Point3D(0.0, 0.0, _length / 2.0));
            }
            else
            {
                if (this.XoZOverCamera && !this.XoYOverCamera)
                {
                    _xoyXAxisTicks.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1.0, 0.0, 0.0), -90.0), new Point3D(0.0, _length, 0.0));
                    _xozXAxisTicks.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1.0, 0.0, 0.0), 90.0), new Point3D(0.0, 0.0, _length));
                }
                else
                {
                    _xoyXAxisTicks.Transform = Transform3D.Identity;
                    _xozXAxisTicks.Transform = Transform3D.Identity;
                }
            }

            if (this.YoZOverCamera && !this.XoYOverCamera)
            {
                _xoyYAxisTicks.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0.0, 1.0, 0.0), 180.0), new Point3D(_length / 2.0, 0.0, 0.0));
                _yozYAxisTicks.Transform = new TranslateTransform3D(_length, 0.0, 0.0);
            }
            else if (!this.YoZOverCamera && this.XoYOverCamera)
            {
                _yozYAxisTicks.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0.0, 1.0, 0.0), 180.0), new Point3D(0.0, 0.0, _length / 2.0));
                _xoyYAxisTicks.Transform = new TranslateTransform3D(0.0, 0.0, _length);
            }
            else
            {
                if (this.YoZOverCamera && this.XoYOverCamera)
                {
                    _xoyYAxisTicks.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0.0, 1.0, 0.0), 90.0), new Point3D(_length, 0.0, 0.0));
                    _yozYAxisTicks.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0.0, 1.0, 0.0), -90.0), new Point3D(0.0, 0.0, _length));
                }
                else
                {
                    _xoyYAxisTicks.Transform = Transform3D.Identity;
                    _yozYAxisTicks.Transform = Transform3D.Identity;
                }
            }
            if ((this.XoZOverCamera && !this.YoZOverCamera) || (!this.XoZOverCamera && this.YoZOverCamera))
            {
                if (this.XoZOverCamera && !this.YoZOverCamera)
                {
                    _yozZAxisTicks.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0.0, 0.0, 1.0), 180.0), new Point3D(0.0, _length / 2.0, 0.0));
                    _xozZAxisTicks.Transform = new TranslateTransform3D(0.0, _length, 0.0);
                }
                else
                {
                    _xozZAxisTicks.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0.0, 0.0, 1.0), 180.0), new Point3D(_length / 2.0, 0.0, 0.0));
                    _yozZAxisTicks.Transform = new TranslateTransform3D(_length, 0.0, 0.0);
                }
            }
            else
            {
                _xozZAxisTicks.Transform = Transform3D.Identity;
                _yozZAxisTicks.Transform = Transform3D.Identity;
            }
        }

        private double GetScreenLength(Point3D p0, Point3D p1)
        {
            Point3D p2 = _visualToScreen.Transform(p0);
            Point3D p3 = _visualToScreen.Transform(p1);
            return Math.Sqrt(Math.Pow(p2.X - p3.X, 2.0) + Math.Pow(p2.Y - p3.Y, 2.0));
        }
    }
}
