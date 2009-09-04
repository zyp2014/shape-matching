using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LiniarAlgebra;

namespace ShapeContext
{
    public static class Histogram
    {
        public static DoubleMatrix[] CreateHistogram(Point[] i_Points, int i_NumOfThetaBins, int i_NumOfBins, out double o_Avg)
        {
            // compute theta angles
            DoubleMatrix anglesMatrix = setAngleMatrix(i_Points);

            Func<double, double> SomeMathOps = input => Math.Floor(input / (2 * Math.PI / i_NumOfThetaBins)); //Using lambda expression
            anglesMatrix.Iterate(SomeMathOps);
            
            // compute cell in bin
            DoubleMatrix radiusMatrix = setRadiusMatrix(i_Points, i_NumOfBins, out o_Avg);

            return setBinsPointReference(anglesMatrix, radiusMatrix, i_NumOfThetaBins, i_NumOfBins, i_Points.Length);
        }

        /// <summary>
        /// Return a matrix which has all angles from each point to all the other
        /// </summary>
        /// <param name="i_Points"></param>
        /// <returns></returns>
        private static DoubleMatrix setAngleMatrix(Point[] i_Points)
        {
            double xLength, yLength;
            int numOfSamples = i_Points.Length;

            double twoPI = 2 * Math.PI;

            DoubleMatrix anglesMatrix = new DoubleMatrix(numOfSamples);

            for (int i = 0; i < numOfSamples; ++i)
            {
                for (int j = 0; j < numOfSamples; ++j)
                {
                    xLength = i_Points[i].X - i_Points[j].X;
                    yLength = i_Points[i].Y - i_Points[j].Y;

                    anglesMatrix[i, j] = Math.Atan2(xLength, yLength);

                    // convert to positive numbers
                    anglesMatrix[i, j] = ((anglesMatrix[i, j] % twoPI) + twoPI) % twoPI;
                }
            }
            return anglesMatrix;
        }

        /// <summary>
        /// NOTE [i,j] == [j,i]
        /// set a matrix which specify references radius for each point (i.e:1..12)
        /// Matrix doesn't have to be of type double !!!
        /// each row contains radius between point 'i' and all other points
        /// </summary>
        /// <param name="i_Points"></param>
        /// <returns></returns>
        private static DoubleMatrix setRadiusMatrix(Point[] i_Points, int i_NumOfBins, out double o_Avg)
        {
            double sumDist = 0; 
            double euclideDist = 0;
            int numOfSamples = i_Points.Length;

            DoubleMatrix radiusMatrix = new DoubleMatrix(numOfSamples);

            // average => different function !!!!

            // set distance
            for (int i = 0; i < numOfSamples; ++i)
            { // symemtric metrix
                for (int j = i; j < numOfSamples; ++j)
                {
                    euclideDist = Utils.EuclidDistance(i_Points[i], i_Points[j]);
                    radiusMatrix[i, j] = euclideDist;
                    radiusMatrix[j, i] = euclideDist;

                    sumDist += (euclideDist * 2); //Because we're symmetric - running only on a half
                }
            }

            o_Avg = sumDist / (numOfSamples^2);
            if (o_Avg != 0)
            {
                radiusMatrix = radiusMatrix / o_Avg;
            }
            
            // set radius
            double val;
            double[] LogSpace = Utils.LogSpace(Math.Log10(.125), Math.Log10(2), i_NumOfBins);
            double upperLogSpaceValue = LogSpace[i_NumOfBins - 1];

            for (int i = 0; i < numOfSamples; ++i)
            {
                for (int j = 0; j < numOfSamples; ++j)
                {
                    val = radiusMatrix[i, j];

                    radiusMatrix[i, j] = Utils.sr_NoInit;

                    if (val > upperLogSpaceValue)
                    {
                        continue;
                    }

                    for (int k = 0; k < i_NumOfBins; ++k)
                    { // find the right radius

                        if (val < LogSpace[k])
                        {
                            radiusMatrix[i, j] = k;
                            break;
                        }

                    }
                }
            }

            return radiusMatrix;
        }

        private static DoubleMatrix[] setBinsPointReference(DoubleMatrix i_AnglesMatrix,
                                                            DoubleMatrix i_RadiusMatrix,
                                                            int          i_NumOfBins,
                                                            int          i_NumOfThetaBins,                                                            
                                                            int          i_NumOfSamples)
        {
            int rowLen = i_AnglesMatrix.RowsCount,
                columnLen = i_AnglesMatrix.ColumnsCount;

            if (rowLen != i_RadiusMatrix.RowsCount ||
                columnLen != i_RadiusMatrix.ColumnsCount)
            { // dimension are not the same
                return null;
            }

            DoubleMatrix[] histogram = new DoubleMatrix[i_NumOfSamples];

            int rLocation, binLocation;
            for (int i = 0; i < rowLen; ++i)
            { // NOTE: symetric matrix

                histogram[i] = new DoubleMatrix(i_NumOfBins, i_NumOfThetaBins);

                for (int j = i; j < columnLen; ++j)
                {
                    // histogram rows: cell in bin, columns: bin
                    // casting !!!!
                    rLocation = (int)i_RadiusMatrix[i, j];
                    binLocation = (int)i_AnglesMatrix[i, j];

                    if (rLocation != Utils.sr_NoInit && binLocation != Utils.sr_NoInit)
                    {
                        ++histogram[i][binLocation, rLocation];
                    }
                }
            }
            return histogram;
        }

    }
}
