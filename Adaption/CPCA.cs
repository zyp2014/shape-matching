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

    public class CPCAresultData : ICData
    {
        private DoubleMatrix    m_Source = null;
        private DoubleMatrix    m_Target = null;
        private DoubleMatrix    m_GlobalShift = null;
        private Bitmap          m_ResultlingBitmap = null;
        private PCAMatching   m_Matching = null;
        private Size            m_CommonSize;

        public CPCAresultData(DoubleMatrix i_Source,DoubleMatrix i_Target,Size i_ImageSize,PCAMatching i_matching)
        {
            ///determine global shifting if required, and calculating the image size according to manipulated data
            m_CommonSize = determineImageSize(i_ImageSize, i_Target);
            Utils.AddScalarsByDims(ref i_Source, m_GlobalShift);
            Utils.AddScalarsByDims(ref i_Target, m_GlobalShift);
            ///Creating new bitmap and setting fields
            m_ResultlingBitmap = new Bitmap(m_CommonSize.Width, m_CommonSize.Height);
            m_Source = i_Source;
            m_Target = i_Target;
            SourceColor = Utilities.sr_defaultSourceColor;
            TargetColor = Utilities.sr_defaultTargetColor;
            m_Matching = i_matching;
        }

        private Size determineImageSize(Size i_ImageSize, DoubleMatrix i_Target)
        {
            Size retSize = i_ImageSize;

            m_GlobalShift = new DoubleMatrix(2, 1);
            m_GlobalShift.Init(0);

            DoubleMatrix dimsMinMax = Utilities.MinMaxByRow(i_Target);
            if (dimsMinMax[Utilities.sr_Xrow, Utilities.sr_MinRow] < 0)
            {
                int GrowBy = Math.Abs((int)Math.Round(dimsMinMax[Utilities.sr_Xrow, Utilities.sr_MinRow]));
                m_GlobalShift[Utilities.sr_Xrow, 0] = GrowBy;
                retSize.Width += GrowBy + 1;
            }
            if (dimsMinMax[Utilities.sr_Yrow, Utilities.sr_MinRow] < 0)
            {
                int GrowBy = Math.Abs((int)Math.Round(dimsMinMax[Utilities.sr_Yrow, Utilities.sr_MinRow]));
                m_GlobalShift[Utilities.sr_Yrow, 0] = GrowBy;
                retSize.Height += GrowBy + 1;
            }
            Utils.AddScalarsByDims(ref dimsMinMax, m_GlobalShift);
            retSize.Width  = Math.Max(retSize.Width,  (int)Math.Round(dimsMinMax[Utilities.sr_Xrow, Utilities.sr_MaxRow]) + 1);
            retSize.Height = Math.Max(retSize.Height, (int)Math.Round(dimsMinMax[Utilities.sr_Yrow, Utilities.sr_MaxRow]) + 1);
            return retSize;
        }

        #region ICData Members

        public Image ResultImage
        {
            get
            {
                Utilities.PutOnBitmap(ref m_ResultlingBitmap, m_Source, SourceColor);
                Utilities.PutOnBitmap(ref m_ResultlingBitmap, m_Target, TargetColor);

                return m_ResultlingBitmap;
            }
        }

        public Image SourceImage
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

        public Image TargetImage
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

        public Size OptimalImageSize
        {
            get 
            {
                return m_CommonSize;
            }
        }

        public Type MyType
        {
            get 
            {
                return typeof(CPCAresultData);
            }
        }

        public PropertyInfo[] PropertyList
        {
            get
            {
                return MyType.GetProperties();
            }
        }

        #endregion

        /// <summary>
        /// Set these properties before using ResultImage prop..
        /// </summary>
        #region PreRun Properties
        public Color SourceColor { get; set; }
        public Color TargetColor { get; set; }
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
