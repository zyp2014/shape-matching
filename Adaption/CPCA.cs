using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;

using PCA;
using LiniarAlgebra;


namespace Adaption
{
    public class CPCA : AlgorithmBase
    {
        private Image m_Image1 = null;
        private Image m_Image2 = null;

        private DoubleMatrix    m_sourceMatrix  = null;
        private DoubleMatrix    m_targetMatrix  = null;        
        private Size            m_commonSize;

        public CPCA()
        {
            TresholdColor = Utilities.sr_defaultTresholdColor;
        }

        #region IMatchingAlgo Members

        public override void Create(Image i_SourceImage, Image i_TargetImage)
        {
            m_Image1 = i_SourceImage;
            m_Image2 = i_TargetImage;
        }

        public override Type MyType
        {
            get
            {
                return typeof(CPCA);
            }
        }

        public override ICData Run()
        {
            List<Point> sourcePoints = Utilities.ExtractPoints(m_Image1, TresholdColor);
            List<Point> targetPoints = Utilities.ExtractPoints(m_Image2, TresholdColor);
            m_sourceMatrix = Utilities.ListToMatrix(sourcePoints);
            m_targetMatrix = Utilities.ListToMatrix(targetPoints);

            m_commonSize = new Size(Math.Max(m_Image1.Width, m_Image2.Width), Math.Max(m_Image1.Height, m_Image2.Height));

            PCAMatching matching = new PCAMatching(new DoubleMatrix((Matrix<double>)m_sourceMatrix.Clone()), m_targetMatrix);
            matching.Calculate();

            CPCAresultData retResult = new CPCAresultData(m_sourceMatrix, matching.Result, m_commonSize, matching);
            retResult.IncludeSource = IncludeSource;
            return retResult;
        }

        public override object Instance
        {
            get { return this; }
        }

        #endregion

        #region PreRun parameters
        public Color TresholdColor { get; set; }
        #endregion
    }

    public class CPCAresultData : ResultBase
    {
        private DoubleMatrix    m_Source = null;
        private DoubleMatrix    m_Target = null;
        private DoubleMatrix    m_GlobalShift = null;
        
        private PCAMatching     m_Matching = null;
        private Size            m_CommonSize;

        public CPCAresultData(DoubleMatrix i_Source,DoubleMatrix i_Target,Size i_ImageSize,PCAMatching i_matching)
        {
            ///Determine global shifting if required, and calculating the image size according to manipulated data
            m_CommonSize = ShiftToPozitives(ref i_Source, ref i_Target, i_ImageSize);
            ///Creating new bitmap and setting fields
            m_ResultlingBitmap = new Bitmap(m_CommonSize.Width, m_CommonSize.Height);
            m_Source = i_Source;
            m_Target = i_Target;
            SourceColor = Utilities.sr_defaultSourceColor;
            TargetColor = Utilities.sr_defaultTargetColor;
            m_Matching = i_matching;
        }

        private Size ShiftToPozitives(ref DoubleMatrix io_Source, ref DoubleMatrix io_Target, Size i_ImageCurrSize)
        {
            //Calculating Optimal(valid) size for targets
            DoubleMatrix minMaxTarget = Utils.ShiftToPositives(ref io_Target,io_Source);
            Size targetSize = new Size(
                (int)Math.Max(Math.Ceiling(minMaxTarget[sr_X, Utils.sr_MaxCol]) + 2, i_ImageCurrSize.Width),
                (int)Math.Max(Math.Ceiling(minMaxTarget[sr_Y, Utils.sr_MaxCol]) + 2, i_ImageCurrSize.Height));
            //Calculating Optimal(valid) size for sources
            DoubleMatrix minMaxSource = Utils.ShiftToPositives(ref io_Source, io_Target);
            Size sourceSize = new Size(
                (int)Math.Max(Math.Ceiling(minMaxSource[sr_X, Utils.sr_MaxCol]) + 2, i_ImageCurrSize.Width),
                (int)Math.Max(Math.Ceiling(minMaxSource[sr_Y, Utils.sr_MaxCol]) + 2, i_ImageCurrSize.Height));

            //Returning Optimal(valid) size for both
            return new Size(
                (int)Math.Max(targetSize.Width, sourceSize.Width),
                (int)Math.Max(targetSize.Height, sourceSize.Height));
        }

        #region ResultBase Members

        public override Image ResultImage
        {
            get
            {
                if (IncludeSource)
                {
                    Utilities.PutOnBitmap(ref m_ResultlingBitmap, m_Source, SourceColor);  
                }
                
                Utilities.PutOnBitmap(ref m_ResultlingBitmap, m_Target, TargetColor);

                return m_ResultlingBitmap;
            }
        }

        public override Image SourceImage
        {
            get
            {
                {
                    Bitmap retBitmap = new Bitmap(OptimalImageSize.Width, OptimalImageSize.Height);
                    Utilities.PutOnBitmap(ref retBitmap, m_Source, SourceColor);
                    return retBitmap;
                }
            }
        }

        public override Image TargetImage
        {
            get
            {
                {
                    Bitmap retBitmap = new Bitmap(OptimalImageSize.Width, OptimalImageSize.Height);
                    Utilities.PutOnBitmap(ref retBitmap, m_Target, TargetColor);
                    return retBitmap;
                }
            }
        }

        public override Size OptimalImageSize
        {
            get 
            {
                return m_CommonSize;
            }
        }

        public override Type MyType
        {
            get 
            {
                return typeof(CPCAresultData);
            }
        }

        public override PropertyInfo[] PropertyList
        {
            get
            {
                return MyType.GetProperties();
            }
        }

        /// <summary>
        /// Set these properties before using ResultImage prop..
        /// </summary>
        #region PreRun Properties
        public override Color SourceColor { get; set; }
        public override Color TargetColor { get; set; }
        #endregion

        #endregion

        #region PostRun Properties - over to ICData Members

        public double AngleFromSource
        {
            get
            {
                return m_Matching.AngleFromSource;
            }
        }

        public DoubleMatrix GlobalShiftingCorrection
        {
            get
            {
                return m_GlobalShift;
            }
        }

        public double XScaleFromSource
        {
            get
            {
                return m_Matching.XScaleFromSource;
            }
        }

        public double YScaleFromSource
        {
            get
            {
                return m_Matching.YScaleFromSource;
            }
        }

        public List<Point> SourcePoints
        {
            get
            {
                return Utilities.MatrixToList(m_Source);
            }
        }

        public List<Point> TargetPoints
        {
            get
            {
                return Utilities.MatrixToList(m_Target);
            }
        }

        #endregion
    }
}
