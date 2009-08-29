using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ShapeContext
{
    public struct Pair<Type1, Type2>
    {
        public Type1 Element1;
        public Type2 Element2;
    }

    public static class Utils
    {
        public static readonly int sr_MinPointsForPolygon = 3;
        public static readonly int sr_NoInit = -1;

        public static double EuclideanDistance(Point i_Point1, Point i_Point2)
        {
            double distX = i_Point1.X - i_Point2.X;
            double distY = i_Point1.Y - i_Point2.Y;

            return Math.Sqrt(distX * distX + distY * distY);
        }

        public static double[] LogSpace(double i_LowBoundery, double i_HighBoundery, int i_NumOfSlices)
        {
            if (i_HighBoundery == Math.PI)
            {
                i_HighBoundery = Math.Log10(Math.PI);
            }

            double[] logSpace = new double[i_NumOfSlices];
            double distance = i_HighBoundery - i_LowBoundery;
            int numOfSlices = i_NumOfSlices - 1;

            for (short i = 0; i < numOfSlices; ++i)
            {
                logSpace[i] = Math.Pow( 10,
                                        i_LowBoundery + i * distance / numOfSlices);
            }

            logSpace[numOfSlices] = Math.Pow(10, i_HighBoundery);
            return logSpace;
        }

        static public void InitArray<Type> (Type[] i_Array, Type i_Value)
        {
            int len = i_Array.Length;
            for (int i = 0; i < len; ++i)
            {
                i_Array[i] = i_Value;
            }
        }

        static public Point[] GetSamplePointsFromPath(Point[] i_pointRepresentedPath, int i_desiredNumOfSamples,bool i_isPolygon)
        {
            Point[] pointsLocalCopy;
            double PathTotalLength = 0;

            #region Condition checking and preperations
            if (i_isPolygon == true)
            {
                if ((i_pointRepresentedPath.Length < sr_MinPointsForPolygon))
                {
                    return null;
                }

                pointsLocalCopy = new Point[i_pointRepresentedPath.Length + 1];
                i_pointRepresentedPath.CopyTo(pointsLocalCopy, 0);
                pointsLocalCopy[i_pointRepresentedPath.Length] = i_pointRepresentedPath[0]; //Closing a shape
            }
            else
            {
                pointsLocalCopy = (Point[])i_pointRepresentedPath.Clone();
            }

            PathTotalLength = CalculateTotalPathLength(pointsLocalCopy);
            if (PathTotalLength / i_desiredNumOfSamples < 1)
            { //It will be impossible to create this amount of unique points
                throw new ShapeContextUtilsException(
                    "It will be impossible to create " + 
                    i_desiredNumOfSamples + 
                    ", thus the total shape lenght is less than the amount of the desired samples");
            }
            #endregion

            #region Samples selection

            HashSet<Point>  uniquePointSet      = new HashSet<Point>();
            Random          pointOnTheLineRand  = new Random(unchecked((int)DateTime.Now.Ticks));
            int             LinesCount          = pointsLocalCopy.Length;
            int             sampledCouner       = 0; //Tracking of defacto number of samples
            for (int lineNum = 1; lineNum < LinesCount; ++lineNum)
            {
                Point   starting        = pointsLocalCopy[lineNum - 1];
                Point   ending          = pointsLocalCopy[lineNum];
                int     lineLenght      = (int)Math.Round(TwoPointsDistance(starting, ending));

                ///Calculating relative amount of samples for this line 
                double relativePortion = lineLenght / PathTotalLength;
                int lineSamples = (int)Math.Round((double)(relativePortion * i_desiredNumOfSamples));

                for (int lineSampleNum = 0; lineSampleNum < lineSamples; ++lineSampleNum)
                {
                    Point randPoint;
                    do /// this while makes sure that the point list(hash table currently) will be unique
                    {
                        int lineStageSelect = pointOnTheLineRand.Next(lineLenght); //will store the percentage of the line where we want to get a point
                        randPoint = ExtractStagePointFromLine(starting, ending, lineStageSelect, lineLenght);
                    } while (uniquePointSet.Contains(randPoint));

                    uniquePointSet.Add(randPoint);
                }

                sampledCouner += lineSamples;
            }

            #endregion

            ///Potential bug detection, if exception is thrown, sample taking algorithm debugging is needed
            if (sampledCouner != i_desiredNumOfSamples)
            {
                throw new ShapeContextUtilsException("Sampled amount of poins is lower than expected to be, it is a certain bug!!!");
            }
            return uniquePointSet.ToArray<Point>();
        }

        /// <summary>
        /// Calculates the point that is located on a specipic stage on the line
        /// </summary>
        /// <param name="i_startLinePoint"></param>
        /// <param name="i_endLinePoint"></param>
        /// <param name="i_lineStage"></param>
        /// <param name="i_StagesGranularity"></param>
        /// <returns></returns>
        public static Point ExtractStagePointFromLine(
            Point   i_startLinePoint, 
            Point   i_endLinePoint, 
            int     i_lineStage,
            int     i_StagesGranularity)
        {
            double x_step = (double)(i_endLinePoint.X - i_startLinePoint.X) / i_StagesGranularity;
            double y_step = (double)(i_endLinePoint.Y - i_startLinePoint.Y) / i_StagesGranularity;

            int new_X = (int)Math.Round((double)i_startLinePoint.X + x_step * (double)i_lineStage);
            int new_Y = (int)Math.Round((double)i_startLinePoint.Y + y_step * (double)i_lineStage);

            return new Point(new_X, new_Y);
        }

        static public Point[] GetSamplePoints(Point[] i_Points, int i_desiredNumOfSamples)
        {
            int origPointsNum = i_Points.GetLength(0);
            Random rndObj = new Random();
            HashSet<Point> pointsUniqueSet = new HashSet<Point>();

            if (origPointsNum < i_desiredNumOfSamples)
            {
                throw new ShapeContextUtilsException("Total points collection is less than desired number of samples");
            }

            for (int i = 0; i < i_desiredNumOfSamples; ++i)
            {
                Point currPoint;
                do
                {
                    int rndNum = rndObj.Next(0, origPointsNum);
                    currPoint = i_Points[rndNum];
                } while (pointsUniqueSet.Contains(currPoint));

                pointsUniqueSet.Add(currPoint);
            }

            return pointsUniqueSet.ToArray<Point>();
        }

        public static Point[] GetIndexedSamplePoints(Point[] i_FullSet, int i_desiredNumOfSamples)
        {
            int origPointsNum = i_FullSet.GetLength(0);
            Random rndObj = new Random();
            HashSet<Point> pointsUniqueSet = new HashSet<Point>();

            if (origPointsNum < i_desiredNumOfSamples)
            {
                throw new ShapeContextUtilsException("Total points collection is less than desired number of samples");
            }

            for (int i = 0; i < i_desiredNumOfSamples; ++i)
            {
                Point currPoint;
                do
                {
                    int rndIndex = rndObj.Next(0, origPointsNum);
                    currPoint = i_FullSet[rndIndex];
                                      
                } while (pointsUniqueSet.Contains(currPoint));


                pointsUniqueSet.Add(currPoint);
            }

            return pointsUniqueSet.ToArray<Point>();
        }
        /// <summary>
        /// Aproximation of the lenght of the shape in pixels
        /// Only a best approximation, because the final drawing depends on the interpolation implementation
        /// </summary>
        /// <param name="i_LinesByPoints"></param>
        /// <returns></returns>
        public static double CalculateTotalPathLength(Point[] i_LinesByPoints)
        {
            Point prevPoint = i_LinesByPoints[0];
            double retShapeLenght = 0;

            foreach (Point currPoint in i_LinesByPoints) //Will loop n+1 times, the 1 time is trivial
            {  
                retShapeLenght += TwoPointsDistance(prevPoint,currPoint);
                prevPoint = currPoint;
            }

            return retShapeLenght;
        }

        public static double TwoPointsDistance(Point i_start,Point i_end)
        {
            int dx = i_end.X - i_start.X;
            int dy = i_end.Y - i_start.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }


    }
}
