using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LiniarAlgebra
{
    public static class LiniarAlgebraFunctions
    {
        private static readonly int sr_2D = 2;
        private static readonly int sr_Xaxis = 0;
        private static readonly int sr_Yaxis = 1;

        /// <summary>
        /// Calculate covarience of N vectors
        /// Each vector has M organs
        /// Assuming that the matrix is mormalized so that the mean of each row is 0
        /// </summary>
        /// <param name="i_MxNmatrix"></param>
        /// <returns></returns>
        public static Matrix<Type> Covarience<Type>(Matrix<Type> i_MxNmatrix) where Type : IComparable<Type>
        {
            int covMatSquareSize = i_MxNmatrix.RowsCount;
            ICalculator<Type> matrixCalc = i_MxNmatrix.Calculator;
            Matrix<Type> retCovMatrix = new Matrix<Type>(covMatSquareSize, i_MxNmatrix.Calculator);
            for (int i = 0; i < covMatSquareSize; ++i)
            {
                for (int j = 0; j <= i; ++j)
                {   ///calculating single vector covarience
                    Type covResult = matrixCalc.Zero();
                    for (int vecIndex = 0; vecIndex < i_MxNmatrix.ColumnsCount; ++vecIndex)
                    {
                        covResult = matrixCalc.Add(
                                        covResult,
                                        matrixCalc.Multiply(
                                            i_MxNmatrix[i, vecIndex], 
                                            i_MxNmatrix[j, vecIndex]
                                                            )
                                                   );
                    }
                    retCovMatrix[i, j] = retCovMatrix[j, i] = matrixCalc.Division(
                                covResult,
                                matrixCalc.FromDouble(i_MxNmatrix.ColumnsCount - 1)
                                                                                  );
                }
            }
            return retCovMatrix;
        }

        /// <summary>
        /// Wraps different implementation from third party
        /// </summary>
        /// <param name="i_MxNmatrix"></param>
        /// <param name="o_EigenValues"></param>
        /// <returns></returns>
        public static DoubleMatrix EigenMatrix(DoubleMatrix i_MxNmatrix,out double[] o_EigenValues)
        {
            double[,] retEigenVectors = EigenValuesFinder.CalcEigenVectors(true, out o_EigenValues, i_MxNmatrix.InnerMatrix);
            return new DoubleMatrix(retEigenVectors);
        }

        public static double Sub2ind(System.Drawing.Size i_MatrixSize, double i_col, double i_row)
        {
            return (i_row) * i_MatrixSize.Width + i_col;
        }

        public static DoubleMatrix PointArrayToMatrix(Point[] i_PTarray, int[] i_mapping)
        {            
            bool useMapping = (i_mapping != null);

            int length = (useMapping) ? i_mapping.Length : i_PTarray.Length;

            DoubleMatrix retMatrix = new DoubleMatrix(length, sr_2D);

            for (int row = 0; row < length; ++row)
            {
                if (useMapping)
                {
                    retMatrix[row, sr_Xaxis] = i_PTarray[i_mapping[row]].X;
                    retMatrix[row, sr_Yaxis] = i_PTarray[i_mapping[row]].Y;
                }
                else
                {
                    retMatrix[row, sr_Xaxis] = i_PTarray[row].X;
                    retMatrix[row, sr_Yaxis] = i_PTarray[row].Y;
                }
            }
            return retMatrix;
        }

        public static Point[] MatrixToPointArray(DoubleMatrix i_PointsMatrix)
        {
            List<Point> retPointList = new List<Point>();
            for (int row = 0; row < i_PointsMatrix.RowsCount; ++row)
            {
                Point currPoint = new Point((int)i_PointsMatrix[row, sr_Xaxis], (int)i_PointsMatrix[row, sr_Yaxis]);
                retPointList.Add(currPoint);
            }
            return retPointList.ToArray();
        }
    }
}
