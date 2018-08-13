using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System;
using _3DTools;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Thorlabs.WPF.Plotter3D.Common;

namespace Thorlabs.WPF.Plotter3D
{
    /// <summary>
    /// Description for Test3.
    /// </summary>
    public partial class Test3 : Window
    {
        private ModelVisual3D curveModel = new ModelVisual3D();
        private ModelVisual3D pntsModel = new ModelVisual3D();
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        System.Timers.Timer timer = new System.Timers.Timer();
        /// <summary>
        /// Initializes a new instance of the Test3 class.
        /// </summary>
        public Test3()
        {
            InitializeComponent();

        }

        private Visual3D axisVisual3D;
        private AxisGridFrame axisFrame;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {


                timer.Interval = 2000;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
                //dispatcherTimer.Start();

                Surface3D curve3D = new Surface3D();

                NumericTicksProider numericProvider = new NumericTicksProider(6, 4, 0.2, 0.1);
                numericProvider.LabelStringFormat = "{0:0.00}";

                var axis = new AxisGridBuilder();
                axis.TicksProvider = numericProvider;
                axis.XRange = new Range<double>(0, 5);
                axis.YRange = new Range<double>(0, 5);
                axis.ZRange = new Range<double>(0, 5);

                axisVisual3D = axis.DrawAxisGrid();
                this.viewPort.Children.Add(axisVisual3D);

                axisFrame = new AxisGridFrame();
                if (showPoints)
                    this.viewPort.Children.Add(axisFrame);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private ModelVisual3D newCurveModel = new ModelVisual3D();
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (coeffWidth > 2)
                coeffWidth = 0.1;

            DataGenerator gen = new DataGenerator();
            coeffWidth += 0.1;
            gen.MouthWidthcoeff = coeffWidth;



            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Surface3D curve3D = new Surface3D();

                newCurveModel = curve3D.CreateSurfaceModel(gen.GenerateData(Convert.ToInt32(this.textBox1.Text)));
                this.viewPort.Children.Remove(curveModel);
                this.viewPort.Children.Remove(wireframe);
                this.viewPort.Children.Add(newCurveModel);
                curveModel = newCurveModel;
            }));
        }

        private double coeffWidth = 0.1;
        private double coeffStep = 0.1;
        private ScreenSpaceLines3D wireframe = new ScreenSpaceLines3D();
        private Stopwatch watch = new Stopwatch();
        private Dictionary<double, Point3D[,]> dicPoints = new Dictionary<double, Point3D[,]>();
        void dispatcherTimer_Tick(object sender, EventArgs e)
        {


            if (coeffWidth > 2)
                coeffWidth = 0.1;

            this.viewPort.Children.Remove(curveModel);
            this.viewPort.Children.Remove(pntsModel);
            this.viewPort.Children.Remove(wireframe);


            DataGenerator gen = new DataGenerator();
            coeffWidth += 0.1;
            gen.MouthWidthcoeff = coeffWidth;

            //if (!dicPoints.ContainsKey(gen.MouthWidthcoeff))
            //    dicPoints.Add(gen.MouthWidthcoeff, gen.GenerateData(Convert.ToDouble(this.textBox1.Text)));

            Surface3D curve3D = new Surface3D();

            Data = gen.GenerateData(Convert.ToInt32(this.textBox1.Text));
            AutoAdjustCoordinate();

            curveModel = curve3D.CreateSurfaceModel(Data);

            pntsModel = curve3D.CreateSurfacePoints(Data);

            if (!showPoints)
            {
                this.viewPort.Children.Add(curveModel);
                wireframe.MakeWireframe(curveModel.Content);
                this.viewPort.Children.Add(wireframe);
                wireframe.OnRender(null, null);
            }
            if (showPoints)
                this.viewPort.Children.Add(pntsModel);



            this.textBlock1.Text = this.watch.ElapsedMilliseconds.ToString();
            this.watch.Reset();
            this.watch.Start();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.dispatcherTimer.Stop();
            //timer.Stop();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.dispatcherTimer.Start();
            watch.Start();

            //timer.Start();
        }

        private bool removed = false;
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (!removed)
            {
                this.viewPort.Children.Remove(this.curveModel);
                removed = !removed;
            }
            else
            {
                this.viewPort.Children.Add(this.curveModel);
                removed = !removed;
            }
        }

        private Point3D[,] Data;

        private void AutoAdjustCoordinate()
        {
            this.viewPort.Children.Remove(axisVisual3D);

            NumericTicksProider numericProvider = new NumericTicksProider(6, 4, 0.2, 0.1);
            numericProvider.LabelStringFormat = "{0:0.00}";

            var axis = new AxisGridBuilder();
            axis.TicksProvider = numericProvider;

            //int x = Data.GetLength(0);
            axis.XRange = new Range<double>(-2, 2);

            //int y = Data.GetLength(1);
            axis.YRange = new Range<double>(-2, 2);

            axis.ZRange = CreateRangeOfZAxis();

            if (!showPoints)
            {
                this.axisVisual3D = axis.DrawAxisGrid();
                this.viewPort.Children.Add(this.axisVisual3D);
            }

            for (int i = 0; i <= Data.GetUpperBound(0); i++)
                for (int j = 0; j <= Data.GetUpperBound(1); j++)
                {
                    Data[i, j] = axis.DataToAxisTransform.Transform(Data[i, j]);
                }
        }

        private Range<double> CreateRangeOfZAxis()
        {
            double minZ = Data[0, 0].Z, maxZ = Data[0, 0].Z;
            for (int i = 0; i <= Data.GetUpperBound(0); i++)
                for (int j = 0; j <= Data.GetUpperBound(1); j++)
                {
                    if (Data[i, j].Z < minZ)
                        minZ = Data[i, j].Z;
                    else
                        if (Data[i, j].Z > maxZ)
                            maxZ = Data[i, j].Z;
                }

            return RoundHelper.CreateRoundedRange(minZ, maxZ);
        }

        bool showPoints = false;
        private void ColumnDefinition_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {            
        }


        private void Grid_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!showPoints)
            {
                showPoints = true;
                this.viewPort.Children.Remove(axisVisual3D);
                this.viewPort.Children.Remove(curveModel);
                this.viewPort.Children.Remove(wireframe);

                this.viewPort.Children.Add(axisFrame);
                this.viewPort.Children.Add(pntsModel);
            }
        }

        private void Grid_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (showPoints)
            {
                this.viewPort.Children.Remove(axisFrame);
                this.viewPort.Children.Remove(pntsModel);


                this.viewPort.Children.Add(axisVisual3D);
                this.viewPort.Children.Add(wireframe);
                this.viewPort.Children.Add(curveModel);

                showPoints = false;
            }
        }
    }
}