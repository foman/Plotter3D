using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.IO;

namespace Plotter3DDemo
{
    public class DataGenerator
    {
        public DataGenerator()
        {
            MouthWidthcoeff = 1;
        }

        public double MouthWidthcoeff
        {
            get;
            set;
        }

        /// <summary>
        /// Z=X^2+Y^2
        /// </summary>
        /// <returns></returns>
        public double[,] GenerateData(double width, int count)
        {
            Point3D center = new Point3D()
            {
                X = 2,
                Y = 2,
                Z = 0
            };
            double MaxY = width, MinY = width * -1, MaxX = width, MinX = width * -1;
            double stepx = width * 2 / Math.Sqrt(count), stepy = stepx;

            Point3D[,] points = new Point3D[(int)(MaxX / stepx * 2), (int)(MaxY / stepy * 2)];

            double[,] data = new double[(int)(MaxX / stepx * 2), (int)(MaxY / stepy * 2)];

            for (int ix = 0; ix < points.GetLength(0); ix++)
            {
                for (int iy = 0; iy < points.GetLength(1); iy++)
                {
                    double x = MinX + ix * stepx, y = MinY + iy * stepy;
                    var z = Math.Pow(x, 2) + Math.Pow(y, 2);
                    points[ix, iy] = new Point3D(x * MouthWidthcoeff / 2, y, z / 2 * MouthWidthcoeff);
                    data[ix, iy] = z;
                }
            }
            return data;
        }

        /// <summary>
        /// Z=X^2+Y^2
        /// </summary>
        /// <returns></returns>
        public Point3D[,] GenerateData2(int count)
        {
            Point3D center = new Point3D()
            {
                X = 2,
                Y = 2,
                Z = 0
            };
            double width = 4;
            double MaxY = width, MinY = width * -1, MaxX = width, MinX = width * -1;
            double stepx = width * 2 / Math.Sqrt(count), stepy = stepx;

            Point3D[,] points = new Point3D[(int)(MaxX / stepx * 2), (int)(MaxY / stepy * 2)];

            double[,] data = new double[(int)(MaxX / stepx * 2), (int)(MaxY / stepy * 2)];

            for (int ix = 0; ix < points.GetLength(0); ix++)
            {
                for (int iy = 0; iy < points.GetLength(1); iy++)
                {
                    double x = MinX + ix * stepx, y = MinY + iy * stepy;
                    var z = Math.Pow(x, 2) * MouthWidthcoeff / 2 + Math.Pow(y, 2) * MouthWidthcoeff / 2;
                    points[ix, iy] = new Point3D(x, y, z);
                    data[ix, iy] = z;
                }
            }
            return points;
        }

        /// <summary>
        /// Z=X^power+Y^power
        /// </summary>
        /// <returns></returns>
        public Point3D[,] GenerateData2(int count, double power)
        {
            Point3D center = new Point3D()
            {
                X = 2,
                Y = 2,
                Z = 0
            };

            double width = 4;
            double MaxY = width, MinY = 0, MaxX = width, MinX = 0;
            double stepx = width * 2 / Math.Sqrt(count), stepy = stepx;

            Point3D[,] points = new Point3D[(int)(MaxX / stepx * 2), (int)(MaxY / stepy * 2)];

            double[,] data = new double[(int)(MaxX / stepx * 2), (int)(MaxY / stepy * 2)];

            for (int ix = 0; ix < points.GetLength(0); ix++)
            {
                for (int iy = 0; iy < points.GetLength(1); iy++)
                {
                    double x = MinX + ix * stepx, y = MinY + iy * stepy;
                    var z = Math.Pow(Math.Abs(x - 4), power) + Math.Pow(Math.Abs(y - 4), power);
                    points[ix, iy] = new Point3D(x, y, z);
                    data[ix, iy] = z;
                }
            }
            return points;
        }

        /// <summary>
        /// Z=X^power+Y^power
        /// </summary>
        /// <returns></returns>
        public double[,] GenerateData3(int count, double power)
        {
 

            double width = 4;
            double MaxY = width, MinY = 0, MaxX = width, MinX = 0;
            double stepx = width * 2 / Math.Sqrt(count), stepy = stepx;

            Point3D[,] points = new Point3D[(int)(MaxX / stepx * 2), (int)(MaxY / stepy * 2)];

            double[,] data = new double[(int)(MaxX / stepx * 2), (int)(MaxY / stepy * 2)];

            for (int ix = 0; ix < points.GetLength(0); ix++)
            {
                for (int iy = 0; iy < points.GetLength(1); iy++)
                {
                    double x = MinX + ix * stepx, y = MinY + iy * stepy;
                    var z = Math.Pow(Math.Abs(x - 4), power) + Math.Pow(Math.Abs(y - 4), power);
                    data[ix, iy] = z;
                    data[ix, iy] = z;
                }
            }
            return data;
        }

        public void writeToFile(string fileFullPath, List<double[,]> dataList)
        {

            using (FileStream fs = File.Open(fileFullPath, FileMode.Append))
            {
                StreamWriter writer = new StreamWriter(fs);
                var dataFirst = dataList[0];
                string header = dataFirst.GetLength(0) + " " + dataFirst.GetLength(1);
                writer.WriteLine(header);

                foreach (var data in dataList)
                {
                    StringBuilder sb = new StringBuilder(data.GetLength(1) * 2);
                    for (int j = 0; j < data.GetLength(0); j++)
                    {
                        for (int k = 0; k < data.GetLength(1); k++)
                        {
                            if (data[j, k].ToString() == " " || data[j, k].ToString() == string.Empty)
                            {
                                int i = k;
                            }
                            sb.Append(data[j, k]);
                            sb.Append(' ');
                        }

                        writer.WriteLine(sb.ToString());
                        var arr = sb.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr.Length != 40)
                        {
                            j = 0;
                        }
                        sb.Length = 0;
                        sb.Clear();
                    }
                }
                writer.Flush();
            }

        }

        public Point3D[,] GenerateBall(int count)
        {

            double r = 2;
            double MaxY = r, MinY = r * -1, MaxX = r, MinX = r * -1;
            double stepx = Math.PI / ((int)Math.Sqrt(count) - 1);
            double stepy = 2 * Math.PI / ((int)Math.Sqrt(count) - 1);


            Point3D[,] points = new Point3D[(int)Math.Sqrt(count), (int)Math.Sqrt(count)];

            for (int ix = 0; ix < points.GetLength(0); ix++)
            {
                double x = Math.Cos(ix * stepx) * r;
                double ry = Math.Sin(ix * stepx) * r;

                for (int iy = 0; iy < points.GetLength(1); iy++)
                {
                    var y = Math.Cos(iy * stepy) * ry;
                    var z = Math.Sin(iy * stepy) * ry;

                    points[ix, iy] = new Point3D(x, y, z);
                }
            }
            return points;
        }
    }
}
