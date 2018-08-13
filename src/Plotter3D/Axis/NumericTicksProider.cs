using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plotter3D
{
    public class NumericTicksProvider : ITickProvider
    {
        private static int[] mayorTickCnts = new int[]
        {
            1,
            2,
            3,
            6,
            11,
            21
        };

        private string _labelstrFormt = string.Empty;

        public int MayorTickCount
        {
            get;
            set;
        }

        public int MinorTicksCount
        {
            get;
            set;
        }

        public double MayorTickSize
        {
            get;
            set;
        }

        public double MinorTickSize
        {
            get;
            set;
        }

        public string LabelStringFormat
        {
            get
            {
                return _labelstrFormt;
            }
            set
            {
                _labelstrFormt = value;
            }
        }

        public NumericTicksProvider(int mayorTickCnt, int minorTickCnt, double mayorTickSize, double minorTickSize)
        {
            this.MayorTickCount = mayorTickCnt;
            this.MinorTicksCount = minorTickCnt;
            this.MayorTickSize = mayorTickSize;
            this.MinorTickSize = minorTickSize;
        }

        public void CreateMayorTicks(List<TickInfo> tickList, Range<double> range, double axixLength)
        {
            tickList.Clear();
            double step = (this.MayorTickCount > 1) ? (axixLength / (double)(this.MayorTickCount - 1)) : double.NaN;
            double valueStep = (this.MayorTickCount > 1) ? ((range.Max - range.Min) / (double)(this.MayorTickCount - 1)) : double.NaN;
            TickInfo[] ticks = new TickInfo[this.MayorTickCount];
            for (int i = 0; i < this.MayorTickCount; i++)
            {
                tickList.Add(new TickInfo(true, range.Min + valueStep * (double)i, step * (double)i));
            }
        }

        /// <summary>
        /// Create minor ticks
        /// eg: |_1_1_1_1_|_1_1_1_1_| MayorTick count is 3,MinorTicksCount is 4.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="axixLength"></param>
        /// <returns></returns>
        public void CreateMinorTicks(List<TickInfo> tickList, Range<double> range, double axixLength)
        {
            tickList.Clear();
            if (this.MayorTickCount != 0)
            {
                double step = axixLength / (double)((this.MayorTickCount - 1) * (this.MinorTicksCount + 1));
                double valueStep = (range.Max - range.Min) / (double)((this.MayorTickCount - 1) * (this.MinorTicksCount + 1));
                TickInfo[] ticks = new TickInfo[(this.MayorTickCount - 1) * (this.MinorTicksCount + 1)];
                for (int i = 0; i < this.MayorTickCount - 1; i++)
                {
                    for (int j = 0; j < this.MinorTicksCount; j++)
                    {
                        tickList.Add(new TickInfo(true, range.Min + valueStep * (double)(i * (this.MinorTicksCount + 1) + j + 1), step * (double)(i * (this.MinorTicksCount + 1) + j + 1)));
                    }
                }
            }
        }

        public int DecreaseMayorTickCnt(int count)
        {
            return NumericTicksProvider.mayorTickCnts.LastOrDefault((int x) => x < count);
        }

        public int IncreaseMayroTickCnt(int count)
        {
            return NumericTicksProvider.mayorTickCnts.FirstOrDefault((int x) => x > count);
        }

        public string GetLabelText(TickInfo tick)
        {
            string result;
            if (!string.IsNullOrEmpty(_labelstrFormt))
            {
                result = string.Format(this.LabelStringFormat, tick.Value);
            }
            else
            {
                result = tick.Value.ToString("0.00");
            }
            return result;
        }
    }
}
