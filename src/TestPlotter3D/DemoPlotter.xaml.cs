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
using System.Windows.Shapes;
using System.Windows.Threading;
using Plotter3D.Common;
using Plotter3D;
using System.Windows.Media.Media3D;
using System.Collections;
using System.IO;
using Microsoft.Win32;

namespace Plotter3DDemo
{
    /// <summary>
    /// Interaction logic for window1.xaml
    /// </summary>
    public partial class DemoPlotterWindow : Window
    {
        private ModelVisual3D model = new ModelVisual3D();
        private double coeffWidth = 1;
        private double power = 1;
        //private double coeffStep = 0.1;
        //private int width = 2;

        public DemoPlotterWindow()
        {
            InitializeComponent();
        }


        void OnGenerate(object sender, EventArgs e)
        {
            DataGenerator gen = new DataGenerator();
            //coeffWidth += 0.1;
            gen.MouthWidthcoeff = coeffWidth;

            //var data = gen.GenerateData(width++, Convert.ToInt32(this.textBox1.Text));
            //this.chartPlotter3D1.Data = data;

            var points = gen.GenerateData3(Convert.ToInt32(this.textBox1.Text), 2);
            this.chartPlotter3D1.Data = points;

            //var points = gen.GenerateBall(Convert.ToInt32(this.textBox1.Text));
            //this.chartPlotter3D1.Points = points;
            //LoadPlotterData();
        }

        private void LoadPlotterData()
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "All files (*.*)|*.*;"
                };
                if (dialog.ShowDialog().Value)
                {
                    using (FileStream fileStream = new FileStream(dialog.FileName, FileMode.Open))
                    {
                        TextReader tr = new StreamReader(fileStream);
                        string header = tr.ReadLine().Trim();
                        string[] dimensions = header.Split(new char[]
                        {
                            ' '
                        });
                        int d0 = Convert.ToInt32(dimensions[0]);
                        int d1 = Convert.ToInt32(dimensions[1]);
                        double[,] data = new double[d0, d1];
                        int i = 0;
                        while (tr.Peek() != -1)
                        {
                            string str = tr.ReadLine().Trim();
                            string[] arr = str.Split(new char[]
                            {
                                ' '
                            }, StringSplitOptions.RemoveEmptyEntries);
                            for (int j = 0; j < d1; j++)
                            {
                                if (arr.Length != d1)
                                {
                                    j = 0;
                                }

                                double result = 0;
                                if (!Double.TryParse(arr[j].Trim(), out result))
                                {
                                    result = Double.NaN;
                                }

                                data[i, j] = result;
                            }

                            i++;
                            if (i == d0)
                                break;
                        }

                        this.chartPlotter3D1.Data = data;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
