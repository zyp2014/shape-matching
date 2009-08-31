using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ShapeContext.Properties;

using LiniarAlgebra;


namespace ShapeContext
{
    /// <summary>
    /// A delegate supporting selection of points from a full set of points.
    /// </summary>
    /// <param name="i_FullSet">A set of points to select from</param>
    /// <returns>An array of selected points</returns>
    public delegate Point[] SelectSamples(Point[] i_FullSet);
    /// <summary>
    /// A delegate supporting measuring of distance between two sets of points representing dark lines on
    /// 2D surface (drawing).
    /// </summary>
    /// <param name="i_ShapeA">A set to measure for the first shape</param>
    /// <param name="i_ShapeB">A set to measure for the second shape</param>
    /// <param name="i_Size">The size of the 'world' for both shapes</param>
    /// <returns>The value of the distance</returns>
    public delegate double Distance(DoubleMatrix i_ShapeA, DoubleMatrix i_ShapeB, Size i_Size);
    /// <Name>          A Shape Context algorithm based matching.   </Name>
    /// <Version>           0.1a Pre release                        </Version>
    /// <FileName>          ShapeContextMatching.cs                 </FileName>
    /// <ClassName>         ShapeContextMatching                    </ClassName>
    /// <OriginalAlgorithmBy>**************************************************
    ///     <Name>          Shaul Gaidelman                         </Name>
    ///     <Email>         shaulg@...                              </Email>
    /// </OriginalAlgorithmBy>
    /// <RefactoredAndModified>
    ///     <Name>          Yanir Tafkev                            </Name>
    ///     <Email>         yanirta@gmail.com                       </Email>
    /// </RefactoredAndModified>***********************************************
    /// <Guidance>
    ///     <Name>                                                  </Name>
    ///     <Email>                                                 </Email>
    /// </Guidance>
    /// <Institution>
    /// </Institution>
    /// <Date>              Aug. - 2009                             </Date>
    /// <License>
    /// This class library is free to use as long as it used unmodified, including the credits above.
    /// For modifications please contact with one of the creators to get the approval.
    /// </License>
    /// <summary>
    /// Shape context is an algorithm matching two monocolored (dark colored) , described by outlines, drawings.
    /// The algorithm gets two sets of points, sampled from the drawings (First from the original,Second from the target). 
    /// Note that the length of the two sets must be identical, otherwise exception is thrown.
    /// The output of the algorithms is array of indexes that indicates for each point from the original set,
    /// what is the corresponding point at the target set.
    /// 
    /// The algorithm provide a good base to identify differences between two shapes, based on the distances betweend
    /// matching points. The more runs of the algorithm, the better indication for the matching.
    /// Some matchings may be false, identify a length for a treshold that suits best for your purposes.
    /// </summary>
    /// <References>
    /// http://en.wikipedia.org/wiki/Shape_context
    /// Matching with Shape Contexts - S. Belongie and J. Malik (2000).
    /// Shape Matching and Object Recognition Using Shape Contexts - S. Belongie, J. Malik, and J. Puzicha (April 2002).
    /// Thin Plate Splines matlab implementation by Fitzgerald J Archibald.
    /// </References>
    /// <SpecialThanks></SpecialThanks> 
    public class ShapeContextMatching
    {
        private static readonly int[]   sr_NoMapping = null;
        private static readonly int     sr_Xaxis = 0;
        private static readonly int     sr_Yaxis = 1;

        private Point[] m_Shape1Samples;
        private Point[] m_Shape2Samples;

        private Point[] m_Shape1Points;     // the original model points
        private Point[] m_Shape2Points;     // the imitation model points
        private Size    m_SurfaceSize;

        

        private DoubleMatrix[] m_Shape1Histogram;
        private DoubleMatrix[] m_Shape2Histogram;

        private int[] m_Matches;

        public ShapeContextMatching(Point[] i_Shape1Points, Point[] i_Shape2Points,Size i_SurfaceSize, SelectSamples samplesSelectionLogic)
	    {
            NumOfThetaBins   = int.Parse(Resources.k_DefaultThetaBins);
            NumOfBins        = int.Parse(Resources.k_DefaultNumOfBins);
            NumOfIterations  = int.Parse(Resources.k_DefaultNumOfIterations);
            DistanceTreshold = double.Parse(Resources.k_DefaultDistanceTreshold);
            m_Matches = null;
            
            m_Shape1Points  = i_Shape1Points;
            m_Shape2Points  = i_Shape2Points;
            m_SurfaceSize   = i_SurfaceSize;
            SelectionLogic  = samplesSelectionLogic;
	    }

        /// <summary>
        /// Prepare Shape context matching on the samples that preveously given in the C'tor.
        /// </summary>
        /// The result will be stored under ResultPoints property.
        /// This property will return full set of points as they needed to be displayed.
        public void FindMatches()
        {
            double shape1DistanceAvg, shape2DistanceAvg;
            
            DoubleMatrix costMatrix;

            int iN = NumOfIterations;

            DoubleMatrix fullSourceSet = LiniarAlgebraFunctions.PointArrayToMatrix(m_Shape1Points, sr_NoMapping);
            DoubleMatrix fullTargetSet = LiniarAlgebraFunctions.PointArrayToMatrix(m_Shape2Points, sr_NoMapping);

            double minDistance = calculateDistance(fullSourceSet, fullTargetSet, m_SurfaceSize);

            do
            {
                m_Shape1Samples = SelectionLogic(m_Shape1Points);
                m_Shape2Samples = SelectionLogic(m_Shape2Points);

                //Calculate histogram - Next version can be in two threads
                m_Shape1Histogram = calcHistograms(m_Shape1Samples, out shape1DistanceAvg);
                m_Shape2Histogram = calcHistograms(m_Shape2Samples, out shape2DistanceAvg);
                
                costMatrix = calculateCostMatrix();

                m_Matches = HungarianAlgorithm.Run(costMatrix);

                
                Pair<DoubleMatrix, DoubleMatrix> SourceTargetMap = buildMappingByIndex(m_Shape1Samples, m_Shape2Samples, m_Matches);

                if (DistanceTreshold > 0)
                {
                    enforceDistance(ref SourceTargetMap, DistanceTreshold);
                    reduceNullDistance(ref SourceTargetMap);                    
                }

                DoubleMatrix currTargetSet = new DoubleMatrix((Matrix<double>)fullTargetSet.Clone());
                TPS.Calculate(SourceTargetMap.Element1, SourceTargetMap.Element2, ref currTargetSet, m_SurfaceSize);

                double currDistance = calculateDistance(fullSourceSet, currTargetSet, m_SurfaceSize);

                if (currDistance <= minDistance)
                {
                    minDistance = currDistance;
                    fullTargetSet = currTargetSet;
                    m_Shape2Points = LiniarAlgebraFunctions.MatrixToPointArray(fullTargetSet);
                }

            } while (iN-- > 0);

            //m_Shape2Points = LiniarAlgebraFunctions.MatrixToPointArray(fullTargetSet);
        }

        #region Private Section

        private double calculateDistance(DoubleMatrix i_Set1, DoubleMatrix i_Set2,Size i_Size)
        {
            if (TwoSetsDistance != null)
            {
                return TwoSetsDistance(i_Set1, i_Set2, i_Size);
            }
            else
            {
                return double.MaxValue;
            }
        }

        private void reduceNullDistance(ref Pair<DoubleMatrix, DoubleMatrix> io_PairedSets)
        {
            int ZeroDistanceCount = 0;
            DoubleMatrix sourcePoints = io_PairedSets.Element1;
            DoubleMatrix targetPoints = io_PairedSets.Element2;

            for (int row = 0; row < sourcePoints.RowsCount; ++row)
            {
                if (sourcePoints[row, sr_Xaxis] == targetPoints[row, sr_Xaxis] &&
                    sourcePoints[row, sr_Yaxis] == targetPoints[row, sr_Yaxis])
                {
                    ZeroDistanceCount++;
                }
            }

            int newRowIndex = 0;
            DoubleMatrix newSource = new DoubleMatrix(sourcePoints.RowsCount - ZeroDistanceCount, sourcePoints.ColumnsCount);
            DoubleMatrix newTarget = new DoubleMatrix(targetPoints.RowsCount - ZeroDistanceCount, targetPoints.ColumnsCount);

            for (int row = 0; row < sourcePoints.RowsCount; ++row)
            {
                if (sourcePoints[row, sr_Xaxis] != targetPoints[row, sr_Xaxis] ||
                    sourcePoints[row, sr_Yaxis] != targetPoints[row, sr_Yaxis])
                {
                    newSource[newRowIndex, sr_Xaxis] = sourcePoints[row, sr_Xaxis];
                    newSource[newRowIndex, sr_Yaxis] = sourcePoints[row, sr_Yaxis];
                    newTarget[newRowIndex, sr_Xaxis] = targetPoints[row, sr_Xaxis];
                    newTarget[newRowIndex, sr_Yaxis] = targetPoints[row, sr_Yaxis];
                    ++newRowIndex;
                }
            }

            io_PairedSets.Element1 = newSource;
            io_PairedSets.Element2 = newTarget;
        }

        private void enforceDistance(ref Pair<DoubleMatrix, DoubleMatrix> io_PairedSets, double i_TresholdFromSurfSize)
        {
            if (i_TresholdFromSurfSize < 0 || i_TresholdFromSurfSize > 100)
            {
                throw new ShapeContextAlgoException("i_TresholdFromSurfSize parameter is not in percents format");
            }
            double[] treshold = new double[2];

            treshold[sr_Xaxis] = (double)m_SurfaceSize.Width / 100.0 * i_TresholdFromSurfSize;
            treshold[sr_Yaxis] = (double)m_SurfaceSize.Height / 100.0 * i_TresholdFromSurfSize;

            DoubleMatrix sourcePoints = io_PairedSets.Element1;
            DoubleMatrix targetPoints = io_PairedSets.Element2;

            Func<int, int, double, double> cellLogic = (row, col, val) =>
            {
                if (Math.Abs(val - sourcePoints[row, col]) > treshold[col])
                {
                    return sourcePoints[row, col];
                }
                return val;
            };

            targetPoints.Iterate(cellLogic);
        }

        private Pair<DoubleMatrix, DoubleMatrix> buildMappingByIndex(Point[] i_sourceMapping, Point[] i_targetMapping, int[] i_matches)
        {
            Pair<DoubleMatrix, DoubleMatrix> retMapping = new Pair<DoubleMatrix, DoubleMatrix>();
            retMapping.Element1 = LiniarAlgebraFunctions.PointArrayToMatrix(i_sourceMapping, sr_NoMapping);
            retMapping.Element2 = LiniarAlgebraFunctions.PointArrayToMatrix(i_targetMapping, i_matches);
            return retMapping;
        }

        private void calcHistograms(out double o_Shape1DistanceAvg, out double o_Shape2DistanceAvg)
        {
            // original model histogram
            m_Shape1Histogram = Histogram.CreateHistogram(m_Shape1Samples, NumOfThetaBins, NumOfBins, out o_Shape1DistanceAvg);

            // imitation model histogram
            m_Shape2Histogram = Histogram.CreateHistogram(m_Shape2Samples, NumOfThetaBins, NumOfBins, out o_Shape2DistanceAvg);
        }

        private DoubleMatrix[] calcHistograms(Point[] i_ShapePoints, out double o_ShapecalcHistograms)
        {
            return Histogram.CreateHistogram(i_ShapePoints, NumOfThetaBins, NumOfBins, out o_ShapecalcHistograms);
        }

        /// <summary>
        /// each entry in histogram points to NxM matrix
        /// were N is number of radius(dist rings) and M is the number of bins
        /// </summary>
        /// <returns></returns>
        private DoubleMatrix calculateCostMatrix()
        {
            if (m_Shape1Histogram.Length != m_Shape2Histogram.Length)
            {
                throw new ShapeContextAlgoException("Histogram doesn't have same points number");
            }

            int N = m_Shape1Histogram.Length;
            DoubleMatrix histogram1, histogram2;
            DoubleMatrix costMatrix = new DoubleMatrix(N);

            for (int i = 0; i < N; ++i)
            { // go over all points histograms

                histogram1 = m_Shape1Histogram[i];
                for (int j = 0; j < N; ++j)
                {
                    histogram2 = m_Shape2Histogram[j];

                    if (histogram1.CellCount != histogram2.CellCount)
                    {
                        throw new ShapeContextAlgoException("Histogram doesn't have same dimension");
                    }

                    costMatrix[i, j] = calcTwoHistogramsCost(histogram1, histogram2);
                }
            }

            return costMatrix;
        }

        private double calcTwoHistogramsCost(DoubleMatrix i_Histogram1, DoubleMatrix i_Histogram2)
        {
            double val1, val2, sum = 0, histogram1Sum, histogram2Sum;
            int rowLen = i_Histogram1.RowsCount, colLen = i_Histogram1.ColumnsCount;

            histogram1Sum = i_Histogram1.Sum();
            histogram2Sum = i_Histogram2.Sum();

            for (int i = 0; i < rowLen; ++i)
            {
                for (int j = 0; j < colLen; ++j)
                {
                    // normelized value
                    val1 = i_Histogram1[i, j] / histogram1Sum;
                    val2 = i_Histogram2[i, j] / histogram2Sum;

                    if ((val1 + val2) != 0)
                    {
                        sum += Math.Pow(val1 - val2, 2) / (val1 + val2);
                    }
                }
            }
            return sum / 2;
        }

        #endregion

        #region Properties

        public int NumOfThetaBins { get; set; }
        public int NumOfBins { get; set; }

        /// <summary>
        /// If optimization/Relaxing method is used, 
        /// NumOfIterations sets the number of times to run that optimization/Relaxing method.
        /// </summary>
        public int NumOfIterations { get; set; }

        /// <summary>
        /// The maximum distance that the matches will be still valid
        /// </summary>
        public double DistanceTreshold { get; set; }

        /// <summary>
        /// The full points set of the target after matching and relaxation.
        /// </summary>
        public Point[] ResultPoints
        {
            get
            { 
                return m_Shape2Points; 
            }
        }

        /// <summary>
        /// A method used to randomize selected amount of sample points.
        /// </summary>
        public SelectSamples SelectionLogic;

        /// <summary>
        /// A method used to calculate distance between two full sets of points.
        /// </summary>
        public Distance TwoSetsDistance;

        #endregion

    }
}
