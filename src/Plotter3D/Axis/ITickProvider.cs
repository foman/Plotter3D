using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plotter3D
{
    public interface ITickProvider
    {
        int MayorTickCount
        {
            get;
            set;
        }

        int MinorTicksCount
        {
            get;
            set;
        }

        double MayorTickSize
        {
            get;
            set;
        }

        double MinorTickSize
        {
            get;
            set;
        }

        string LabelStringFormat
        {
            get;
            set;
        }

        void CreateMayorTicks(List<TickInfo> tickList, Range<double> range, double axixLength);

        void CreateMinorTicks(List<TickInfo> tickList, Range<double> range, double axixLength);

        int DecreaseMayorTickCnt(int count);

        int IncreaseMayroTickCnt(int count);

        string GetLabelText(TickInfo tick);
    }
}
