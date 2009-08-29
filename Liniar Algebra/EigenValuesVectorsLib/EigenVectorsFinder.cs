using System;

namespace LiniarAlgebra
{
    internal static class EigenValuesFinder
    {
        /*************************************************************************
        Finding the eigenvalues and eigenvectors of a symmetric matrix

        The algorithm finds eigen pairs of a symmetric matrix by reducing it to
        tridiagonal form and using the QL/QR algorithm.

        Input parameters:
            N       -   size of matrix A.
            IsUpper -   storage format.
            ZNeeded -   flag controlling whether the eigenvectors are needed or not.
                        If ZNeeded is equal to:
                         * 0, the eigenvectors are not returned;
                         * 1, the eigenvectors are returned.

        Output parameters:
            D       -   eigenvalues in ascending order.
                        Array whose index ranges within [0..N-1].
            Z       -   if ZNeeded is equal to:
                         * 0, Z hasn’t changed;
                         * 1, Z contains the eigenvectors.
                        Array whose indexes range within [0..N-1, 0..N-1].
                        The eigenvectors are stored in the matrix columns.

        Result:
            True, if the algorithm has converged.
            False, if the algorithm hasn't converged (rare case).

          -- ALGLIB --
             Copyright 2005-2008 by Bochkanov Sergey
        *************************************************************************/
        public static double[,] CalcEigenVectors(bool isupper, out double[] d, double[,] matrix)
        {
            const int zNeeded = 1; //true
            int n = matrix.GetLength(0);
            double[] tau = new double[0];
            double[] e = new double[0];
            //double[] d = new double[n];
            d = new double[n];
            double[,] ca = (double[,])matrix.Clone();
            double[,] retEigenVectsd = (double[,])matrix.Clone();

            System.Diagnostics.Debug.Assert(matrix.GetLength(0) == matrix.GetLength(1), "The matrix should be NxN");

            tridiagonal.smatrixtd(ref ca, n, isupper, ref tau, ref d, ref e);
            tridiagonal.smatrixtdunpackq(ref ca, n, isupper, ref tau, ref retEigenVectsd);
            tdevd.smatrixtdevd(ref d, e, n, zNeeded, ref retEigenVectsd);
            return retEigenVectsd;
        }
    }
}