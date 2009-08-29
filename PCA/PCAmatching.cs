using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LiniarAlgebra;

namespace PCA
{
    /// <Name>              PCA matching algorithm                  </Name>
    /// <Version>           0.1a Pre release                        </Version>
    /// <FileName>          PCAMatching.cs                          </FileName>
    /// <ClassName>         PCA2dMatching                           </ClassName>
    ///<Creator>***********************************************************
    ///     <Name>          Yanir Taflev                            </Name>
    ///     <Email>         yanirta@gmail.com                       </Email>
    ///</Creator>**********************************************************
    /// <Guidance>
    ///     <Name>                                                  </Name>
    ///     <Email>                                                 </Email>
    /// </Guidance>
    /// <Institution>
    /// </Institution>
    /// <Date>              Aug. - 2009                             </Date>
    /// <License>
    /// This class library is free to use as long as it used unmodified, including the credits above.
    /// For modifications please contact with the creator to get an approval.
    /// </License>
    /// <summary>
    /// This class using PCAtransform (PCAtransform.cs) to adjust scaling and rotation angle between two shapes
    /// represented as set of N coordinates translated to 2 x N matrix. (it can be M x N but no meaning for this)
    ///     Given a source and a target points set (not necessarily with the same amount), the result after matching will be
    /// a new set of coordinates based on target set but after scaling and rotation to match the source.
    /// The angle value and scaling factors are also available after processing.
    /// </summary>
    /// <References>
    /// http://en.wikipedia.org/wiki/Principal_component_analysis
    /// http://www.cs.otago.ac.nz/cosc453/student_tutorials/principal_components.pdf
    /// Not used as a source reference but will help to understand:
    /// http://www.cs.cmu.edu/~elaw/papers/pca.pdf
    /// http://genetics.agrsci.dk/statistics/courses/Rcourse-DJF2006/day3/PCA-computing.pdf
    /// </References>
    /// <SpecialThanks></SpecialThanks> 
    public class PCAMatching
    {
        #region Private members

        private static readonly int sr_X = 0;
        private static readonly int sr_Y = 1;

        private PCAtransform m_SourceTransform   = null;
        private PCAtransform m_TargetTransform   = null;

        private DoubleMatrix m_EigenSourceMatrix = null;
        private DoubleMatrix m_EigenTargetMatrix = null;

        private DoubleMatrix m_ResultTarget      = null;

        #endregion

        /// <summary>
        /// Creating a matching object based on two sets of points.
        /// </summary>
        /// <param name="i_Source">2 x M1 points the first row is the X value, the second is the Y value</param>
        /// <param name="i_Target">2 x M2 points the first row is the X value, the second is the Y value</param>
        public PCAMatching(DoubleMatrix i_Source,DoubleMatrix i_Target)
        {
            if (i_Source.RowsCount != i_Target.RowsCount)
            {
                throw new PCAException("Cannot match between two sets with different dimensions");
            }
            m_SourceTransform   = new PCAtransform(i_Source);
            m_TargetTransform   = new PCAtransform(i_Target);
        }

        /// <summary>
        /// Calculating and creating a new set of points that as possibly matched to the source set given in the c'tor.
        /// </summary>
        public void Calculate()
        {
            m_EigenSourceMatrix = m_SourceTransform.Calculate();
            m_EigenTargetMatrix = m_TargetTransform.Calculate();

            normalize(m_SourceTransform, m_TargetTransform);
        }

        #region private section

        private void normalize(PCAtransform i_SourceTransform, PCAtransform i_TargetTransform)
        {
            double angle = AngleFromSource;
            double scaleByX = Math.Sqrt(i_SourceTransform.EigenValues[sr_X] / i_TargetTransform.EigenValues[sr_X]);
            //Two options of taking scale factor of the axis:
                //i_SourceTransform.AverageByDimension[sr_X, 0] / i_TargetTransform.AverageByDimension[sr_X, 0];
                //i_SourceTransform.EigenValues[sr_X] / i_TargetTransform.EigenValues[sr_X];
            double scaleByY = Math.Sqrt(i_SourceTransform.EigenValues[sr_Y] / i_TargetTransform.EigenValues[sr_Y]);
            //Two options of taking scale factor of the axis:
                //i_SourceTransform.AverageByDimension[sr_Y, 0] / i_TargetTransform.AverageByDimension[sr_Y, 0];
                //i_SourceTransform.EigenValues[sr_Y] / i_TargetTransform.EigenValues[sr_Y];

            m_ResultTarget = translate(i_TargetTransform.CenteredPoints, angle, scaleByX, scaleByY);
            Utils.AddScalarsByDims(ref m_ResultTarget, i_SourceTransform.AverageByDimension);
        }

        private DoubleMatrix translate(DoubleMatrix i_CenteredMatrix, double i_Angle, double i_XScale, double i_YSale)
        {
            DoubleMatrix retResult = new DoubleMatrix((Matrix<double>)i_CenteredMatrix.Clone());

            DoubleMatrix rotateMatrix = new DoubleMatrix(2, 2);
            rotateMatrix[0, 0] = Math.Cos(i_Angle);
            rotateMatrix[0, 1] = -1 * Math.Sin(i_Angle);
            rotateMatrix[1, 0] = Math.Sin(i_Angle);
            rotateMatrix[1, 1] = Math.Cos(i_Angle);
            DoubleMatrix scaleMatrix = new DoubleMatrix(2, 2);
            scaleMatrix.Init(0);
            scaleMatrix[0, 0] = i_XScale;
            scaleMatrix[1, 1] = i_YSale;

            DoubleMatrix transform = scaleMatrix * rotateMatrix;
            return transform * retResult;
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Return a factor of scaling by X of the target set, comparing to the source set.
        /// Note: Will return correct result only after calling Calculate() method.
        /// </summary>
        public double XScaleFromSource
        {
            get
            {
                return m_TargetTransform.EigenValues[sr_X] / m_SourceTransform.EigenValues[sr_X];
            }
        }

        /// <summary>
        /// Return a factor of scaling by Y of the target set, comparing to the source set.
        /// Note: Will return correct result only after calling Calculate() method.
        /// </summary>
        public double YScaleFromSource
        {
            get
            {
                return m_TargetTransform.EigenValues[sr_Y] / m_SourceTransform.EigenValues[sr_Y];
            }
        }

        /// <summary>
        /// Return the angle between the original shape and target shape
        /// In Radians.
        /// </summary>
        public double AngleFromSource
        {
            get
            {
                return Math.Abs(m_TargetTransform.GetAngle(0, 1) - m_SourceTransform.GetAngle(0, 1));
            }
        }

        /// <summary>
        /// Return the newly calculated transformed set.
        /// Note: Will return correct result only after calling Calculate() method.
        /// </summary>
        public DoubleMatrix Result
        {
            get
            {
                return m_ResultTarget;
            }
        }

        /// <summary>
        /// Return the PCAtrasform object correspondind to the source point set.
        /// Note:   some of the properties of the object will return a correct result
        ///         only after calling Calculate() method.
        /// </summary>
        public PCAtransform SourceTransform
        {
            get
            {
                return m_SourceTransform;
            }
        }

        /// <summary>
        /// Return the PCAtrasform object correspondind to the target point set.
        /// Note:   some of the properties of the object will return a correct result
        ///         only after calling Calculate() method.
        /// </summary>
        public PCAtransform TargetTransform
        {
            get
            {
                return m_TargetTransform;
            }
        }

        #endregion

    }
}
