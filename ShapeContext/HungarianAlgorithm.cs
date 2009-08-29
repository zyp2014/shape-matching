using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LiniarAlgebra;

namespace ShapeContext
{
    public static class HungarianAlgorithm
    {
        public static readonly int r_PrimeRowIndex = 0;
        public static readonly int r_PrimeColIndex = 1;

        public static int[] Run(DoubleMatrix i_Matrix)
        {
            int starredColumnInPrimedRow;

            // non negative values are the index of the starred or primed zero in the row or column
            int[] starsByRowArr = new int[i_Matrix.RowsCount];
            Utils.InitArray<int>(starsByRowArr, Utils.sr_NoInit);

            int[] starsByColArr = new int[i_Matrix.ColumnsCount];
            Utils.InitArray<int>(starsByColArr, Utils.sr_NoInit);

            int[] primesByRowArr = new int[i_Matrix.RowsCount];
            Utils.InitArray<int>(primesByRowArr, Utils.sr_NoInit);

            //' NEEDED !!!
            bool[] isStarredRowArr = new bool[i_Matrix.RowsCount];
            bool[] isStarredColumnArr = new bool[i_Matrix.ColumnsCount];

            bool[] isCoveredColumnArr = new bool[i_Matrix.ColumnsCount];
            bool[] isCoveredRowArr = new bool[i_Matrix.ColumnsCount];

            // (step 1)
            // subtract minumum value from each rows, to create zeroes
            reduceMatrixByRowMin(i_Matrix);
            //reduceMatrixByColMin(i_Matrix);

            // (step 2/3)
            // star any zero that has no other starred zero in the same row or column
            initStars(
                i_Matrix,
                starsByRowArr,
                starsByColArr,
                isStarredRowArr,
                isStarredColumnArr,
                isCoveredColumnArr);

            // mark zero's column as covered
            // PUT IN InitStar !!!!
            //coverColumnsOfStarredZeroes(starsByColArr,isCoveredColumnArr);

            int[] primedZero;
            while (allColumnsRCovered(isCoveredColumnArr) == false)
            { // repeat untill all columns has zero

                // existing zero in an uncovered row =>  primedZero = (row,column)
                primedZero = primeUncoveredZero(i_Matrix,
                                                primesByRowArr,
                                                isCoveredRowArr,
                                                isCoveredColumnArr);

                while (primedZero == null)
                {// no uncovered zero found (step 6)

                    // make a new zero
                    makeMoreZeroes(
                                i_Matrix,
                                isCoveredRowArr,
                                isCoveredColumnArr);

                    // (part A of step 4)
                    primedZero = primeUncoveredZero(i_Matrix,
                                                primesByRowArr,
                                                isCoveredRowArr,
                                                isCoveredColumnArr);
                }

                // check if there is a starred zero in the primed zero's row
                starredColumnInPrimedRow = starsByRowArr[primedZero[r_PrimeRowIndex] /*== prime zero's row*/];
                if (starredColumnInPrimedRow == Utils.sr_NoInit)
                { // no starred zero in primmed row (step 4B)

                    // (step 5)
                    incrementSetOfStarredZeroes(
                                            primedZero,
                                            starsByRowArr,
                                            starsByColArr,
                                            primesByRowArr);

                    Utils.InitArray<int>(primesByRowArr, Utils.sr_NoInit);
                    Utils.InitArray<bool>(isCoveredRowArr, false);
                    Utils.InitArray<bool>(isCoveredColumnArr, false);

                    // mark zero's column as covered
                    coverColumnsOfStarredZeroes(starsByColArr, isCoveredColumnArr);

                }
                else
                { // found starred zero in new primed zero row (step 4C)

                    // cover primed row
                    isCoveredRowArr[primedZero[r_PrimeRowIndex]] = true;

                    // uncover starred zero column
                    isCoveredColumnArr[starredColumnInPrimedRow] = false;
                }
            }

            //// ok now we should have assigned everything
            //// take the starred zeroes in each column as the correct assignments

            int rowLen = starsByRowArr.Length;
            int[] retval = new int[rowLen];

            // retval = assigned job for 'i'
            for (int i = 0; i < rowLen; ++i)
            {
                retval[i] = starsByRowArr[i];
            }
            return retval;
        }


        // the first step of the hungarian algorithm
        // is to find the smallest element in each row
        // and subtract it's values from all elements
        // in that row
        private static void reduceMatrixByRowMin(DoubleMatrix i_Matrix)
        {
            int rowLen = i_Matrix.RowsCount;
            int colLen = i_Matrix.ColumnsCount;
            double minVal;

            for (int i = 0; i < rowLen; ++i)
            {
                minVal = double.MaxValue;
                for (int j = 0; j < colLen; ++j)
                { // go over each row

                    // find minimum in row
                    if (minVal > i_Matrix[i, j])
                    {
                        minVal = i_Matrix[i, j];
                    }
                }

                // reduce minimum from each cell in same row
                for (int j = 0; j < colLen; ++j)
                { // reduce minmum value from each cell in row
                    i_Matrix[i, j] -= minVal;
                }
            }
        }


        //init starred zeroes

        //for each column find the first zero
        //if there is no other starred zero in that row
        //then star the zero, cover the column and row and
        //go onto the next column
         
        // look for zero that its rox/column wasn't marked akready by another zero
        // i_StarsByRow - i_StarsByRow[ row index ] = < zero (column) index>
        // i_StarsByCol - i_StarsByCol[ column index ] = < zero (row) index>
        private static void initStars(
                DoubleMatrix    i_CostMatrix,
                int[]           i_StarsByRow,
                int[]           i_StarsByCol,
                bool[]          i_IsRowStarredZero,
                bool[]          i_IsColStarredZero,
                bool[]          i_IsCoveredColumnArr)
        {
            int rowLen = i_CostMatrix.RowsCount;
            int colLen = i_CostMatrix.ColumnsCount;

            for (int i = 0; i < rowLen; ++i)
            {
                for (int j = 0; j < colLen; ++j)
                {
                    if (i_CostMatrix[i, j] == 0 &&
                        i_IsRowStarredZero[i] == false && i_IsColStarredZero[j] == false)
                    {  // cell itself is zero
                        // no zero was found in same row or column

                        // point for each row where zero was found
                        i_StarsByRow[i] = j;

                        // point for each column where zero was found
                        i_StarsByCol[j] = i;

                        // mark row/column (NEEDED !!! or i_StarsByRow/Col is enough )
                        i_IsRowStarredZero[i] = true;
                        i_IsColStarredZero[j] = true;

                        // cover starred zero column
                        i_IsCoveredColumnArr[j] = true;

                        break; // move to the next row
                    }
                }
            }
        }

        /// <summary>
        /// Check if all columns have zero
        /// </summary>
        /// <param name="i_CoveredCols"></param>
        /// <returns></returns>
        private static bool allColumnsRCovered(bool[] i_IsCoveredColumn)
        {
            int len = i_IsCoveredColumn.Length;
            for (int i = 0; i < len; ++i)
            {
                if (i_IsCoveredColumn[i] == false)
                {
                    return false;
                }
            }
            return true;
        }


        // if row has more than one zero and because first zero in an 
        // uncovered row and columns is marked
        // could be that there is another zero in same row that can help cover
        // an uncovered column
        private static int[] primeUncoveredZero(
                DoubleMatrix    i_CostMatrix,
                int[]           primesByRow,
                bool[]          i_IsCoveredRowArr,
                bool[]          i_IsCoveredColumnArr)
        {
            int rowLen = i_CostMatrix.RowsCount;
            int colLen = i_CostMatrix.ColumnsCount;

            // find an uncovered zero and prime it
            for (int i = 0; i < rowLen; ++i)
            {
                if (i_IsCoveredRowArr[i] == true)
                { // row is already covered
                    continue;
                }

                for (int j = 0; j < colLen; ++j)
                {
                    if (i_CostMatrix[i, j] == 0 && i_IsCoveredColumnArr[j] == false)
                    {
                        // prime it
                        primesByRow[i] = j;
                        return new int[] { i, j };
                    }
                }
            }
            return null;
        }

        private static void makeMoreZeroes(
                DoubleMatrix i_Matrix,
                bool[] i_IsCoveredRowArr,
                bool[] i_IsCoveredColumnArr)
        {
            int colLen = i_Matrix.ColumnsCount;
            int rowLen = i_Matrix.RowsCount;

            // find lowest uncovered value (step 4E)
            double minUncoveredValue = getMinValInMatrix(
                                                    i_Matrix,
                                                    i_IsCoveredRowArr,
                                                    i_IsCoveredColumnArr);

            for (int i = 0; i < rowLen; ++i)
            {
                for (int j = 0; j < colLen; ++j)
                {
                    if (i_IsCoveredRowArr[i] == true)
                    { // row is covered
                        i_Matrix[i, j] += minUncoveredValue;
                    }

                    if (i_IsCoveredColumnArr[j] == false)
                    { // column is covered
                        i_Matrix[i, j] -= minUncoveredValue;
                    }
                }
            }
        }

        // find smallest uncovered zero
        private static double getMinValInMatrix(
                DoubleMatrix i_CostMatrix,
                bool[] i_IsCoveredRowArr,
                bool[] i_IsCoveredColumnArr)
        {
            int rowLen = i_CostMatrix.RowsCount;
            int colLen = i_CostMatrix.ColumnsCount;

            double minUncoveredValue = double.MaxValue;

            // find an uncovered zeroes
            for (int i = 0; i < rowLen; ++i)
            {
                if (i_IsCoveredRowArr[i] == true)
                { // row is covered
                    continue;
                }

                for (int j = 0; j < colLen; ++j)
                {
                    if (i_IsCoveredColumnArr[j] == true)
                    { // column is covered
                        continue;
                    }

                    if (minUncoveredValue > i_CostMatrix[i, j])
                    {
                        minUncoveredValue = i_CostMatrix[i, j];
                    }
                }
            }
            return minUncoveredValue;
        }


        /// <summary>
        /// see step 5 for more explanation
        /// </summary>
        /// <param name="i_UnStarredRowOfPrimeZero">Zo (see step 5)</param>
        /// <param name="i_StarsByRow"></param>
        /// <param name="i_StarsByCol"></param>
        /// <param name="i_PrimesByRow"></param>
        /// <param name="i_PrimeRowIndex"></param>
        /// <param name="i_PrimeColIndex"></param>
        private static void incrementSetOfStarredZeroes(
                int[] i_UnStarredRowOfPrimeZero,
                int[] i_StarsByRow,
                int[] i_StarsByCol,
                int[] i_PrimesByRow)
        {
            int starRowOfPrimedZeroColumn, primedZeroColumn, primedZeroRow;

            // build the alternating zero sequence (<prime, star>, <prime, star>, etc)

            primedZeroRow = i_UnStarredRowOfPrimeZero[r_PrimeRowIndex];
            primedZeroColumn = i_UnStarredRowOfPrimeZero[r_PrimeColIndex];

            //zeroPairs[i_UnStarredRowOfPrimeZero] = true;

            do
            {
                // Z1 (see step 5)
                // find starred zero row of primed column
                starRowOfPrimedZeroColumn = i_StarsByCol[primedZeroColumn];

                // star the primmed zero
                // no need to update primes (will be all erase shortly)
                i_StarsByRow[primedZeroRow] = primedZeroColumn;
                i_StarsByCol[primedZeroColumn] = primedZeroRow;

                if (starRowOfPrimedZeroColumn == Utils.sr_NoInit)
                { // primed zero with unstarred column
                    break;
                }

                // we have a starred zero in primmed column

                // Z2 (see step 5)
                // find primed zero in starred zero row (should always be one) 
                primedZeroRow = starRowOfPrimedZeroColumn;
                primedZeroColumn = i_PrimesByRow[starRowOfPrimedZeroColumn];

            } while (true);
        }

        /// <summary>
        ///mark columns containing a starred zero
        /// </summary>
        /// <param name="i_StarsByCol"></param>
        /// <param name="i_IsCoveredCols"></param>
        private static void coverColumnsOfStarredZeroes(
                int[] i_StarsByCol,
                bool[] i_IsCoveredCols)
        {
            int len = i_StarsByCol.Length;
            for (int i = 0; i < len; ++i)
            {
                i_IsCoveredCols[i] = (i_StarsByCol[i] == Utils.sr_NoInit ? false : true);
            }
        }
    }
}
