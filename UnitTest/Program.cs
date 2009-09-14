using System;
using System.Collections.Generic;

using System.Windows.Forms;
using LiniarAlgebra;

namespace UnitTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
            //DoubleMatrix a = new DoubleMatrix(2);
            //a[0, 0] = 2;
            //a[0, 1] = 2;
            //a[1, 0] = 2;
            //a[1, 1] = 2;

            //DoubleMatrix b = a * a;
            //double c = LiniarAlgebraFunctions.SumMatrix<double>(b);
            //Func<double, double> SomeMathOps = input => Math.Floor(input / (2 * Math.PI )); //Using lambda expression
            //b.Iterate(SomeMathOps);
        }
    }
}
