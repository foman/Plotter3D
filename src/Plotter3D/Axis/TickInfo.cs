using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plotter3D
{
    public struct TickInfo
    {
        public TickInfo(bool isMayor, double value, double axisValue)
        {
            _isMayorTick = isMayor;
            _value = value;
            _axisValue = axisValue;
        }

        private bool _isMayorTick;
        public bool IsMayorTick
        {
            get { return _isMayorTick; }
        }

        private double _value;
        public double Value
        {
            get { return _value; }
        }

        private double _axisValue;

        public double AxisValue
        {
            get { return _axisValue; }
        }
    }
}
