using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiniarAlgebra
{
    public class IntMatrix : Matrix<int>
    {
        #region C'tors
        public IntMatrix(int i_RowsAsColumns):this(i_RowsAsColumns,i_RowsAsColumns)
        { }

        public IntMatrix(int i_Rows, int i_Columns):
            base(i_Rows, i_Columns,new IntCalculator())
        { }

        public IntMatrix(int[,] i_dMatrix)
            : base(i_dMatrix, new IntCalculator())
        { }

        public IntMatrix(Matrix<int> i_baseMatrix)
            : base(i_baseMatrix)
        { }
        #endregion
    }

    internal class IntCalculator : ICalculator<int>
    {
        #region ICalculator<int> Members

        public int Add(int i_LeftOperator, int i_RightOperator)
        {
            return i_LeftOperator + i_RightOperator;
        }

        public int Sub(int i_LeftOperator, int i_RightOperator)
        {
            return i_LeftOperator - i_RightOperator;
        }

        public int Multiply(int i_LeftOperator, int i_RightOperator)
        {
            return i_LeftOperator * i_RightOperator;
        }

        public int Division(int i_LeftOperator, int i_RightOperator)
        {
            return (int)(i_LeftOperator / i_RightOperator);
        }

        public int Zero()
        {
            return 0;
        }

        public int One()
        {
            return 1;
        }

        public double ToDouble(int i_ValueToConvert)
        {
            return i_ValueToConvert;
        }

        public int FromDouble(double i_ValueToConvert)
        {
            return (int)i_ValueToConvert;
        }

        #endregion
    }
}
