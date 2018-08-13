using System.Windows;
using Thorlabs.WPF.Plotter3D.Common;

namespace Thorlabs.WPF.Plotter3D
{
    /// <summary>
    /// Description for Test2.
    /// </summary>
    public partial class Test2 : Window
    {
        /// <summary>
        /// Initializes a new instance of the Test2 class.
        /// </summary>
        public Test2()
        {
            InitializeComponent();

            var range = RoundHelper.CreateRoundedRange(2.1, 3.5);

            double a = 10.2;
            double b = 1;
            var ia = (int)a;
            var ib = (int)b;
            ia = ia - (ia + ib) % ib;
            var c = a + b / 2;

            var d = c % b;

            a = a - d;

        }
    }
}