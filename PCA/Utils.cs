using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiniarAlgebra;

namespace PCA
{
    public static class Utils
    {
        public static void SubstractScalarsByDims(ref DoubleMatrix io_leftHandMatrix, DoubleMatrix i_rightHandVector)
        {
            if ((i_rightHandVector.ColumnsCount > 1) || (io_leftHandMatrix.RowsCount != i_rightHandVector.RowsCount))
            {
                throw new PCAException("Dimension are not meet for substraction of a vector from matrix by rows");
            }

            Func<int, int, double, double> substractByDim = (row, col, Val) => (Val - (double)i_rightHandVector[row, 0]);
            io_leftHandMatrix.Iterate(substractByDim);
        }

        public static void AddScalarsByDims(ref DoubleMatrix io_leftHandMatrix, DoubleMatrix i_rightHandVector)
        {
            if ((i_rightHandVector.ColumnsCount > 1) || (io_leftHandMatrix.RowsCount != i_rightHandVector.RowsCount))
            {
                throw new PCAException("Dimension are not meet for substraction of a vector from matrix by rows");
            }

            Func<int, int, double, double> substractByDim = (row, col, Val) => (Val + (double)i_rightHandVector[row, 0]);
            io_leftHandMatrix.Iterate(substractByDim);
        }
    }
}
