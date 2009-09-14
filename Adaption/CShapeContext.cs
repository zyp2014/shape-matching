using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;

using ShapeContext;
using LiniarAlgebra;

namespace Adaption
{
    public class CShapeContext : AlgorithmBase
    {
        public static readonly int sr_X = 0;
        public static readonly int sr_Y = 1;

        private Image m_Image1 = null;
        private Image m_Image2 = null;

        protected   Point[] m_sourcePoints  = null;
        protected   Point[] m_targetPoints = null;
        protected   Size    m_commonSize;

        private ShapeContextMatching m_matching;

        public CShapeContext()
        {
            TresHoldColor       = Utilities.sr_defaultTresholdColor;
            NumberOfSamples     = Utils.sr_NoInit;
            NumberOfIterations  = Utils.sr_NoInit;
            NumberOfBins        = Utils.sr_NoInit;
            NumberOfThetaBins   = Utils.sr_NoInit;
            SourceSamples       = null;
            TargetSamples       = null;
        }

        #region PreRun parameters

        /// <summary>
        /// Determines which colors will be selected for processing,
        /// Lighter colors(Higher values) than the treshold will be dropped.
        /// This property has to be set before Create(...) method. otherwise not consedered.
        /// </summary>  
        public Color    TresHoldColor       { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <get>If the value is set to -1 , will use the default</get>
        public int      NumberOfSamples     { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <get>If the value is set to -1 , will use the default</get>
        public int      NumberOfIterations  { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <get>If the value is set to -1 , will use the default</get>
        public int      NumberOfBins        { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <get>If the value is set to -1 , will use the default</get>
        public int      NumberOfThetaBins   { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <get>If the value is set to null , will use the default</get>
        public Point[]  SourceSamples       { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <get>If the value is set to null , will use the default</get>
        public Point[]  TargetSamples       { get; set; }

        #endregion

        #region IMatchingAlgo Members

        public override void Create(Image i_SourceImage, Image i_TargetImage)
        {
            m_Image1 = i_SourceImage;
            m_Image2 = i_TargetImage;

            List<Point> source = Utilities.ExtractPoints(m_Image1, TresHoldColor);
            List<Point> target = Utilities.ExtractPoints(m_Image2, TresHoldColor);

            m_sourcePoints = Utilities.ListToArray(source);
            m_targetPoints = Utilities.ListToArray(target);
            
            m_commonSize = new Size(Math.Max(m_Image1.Width, m_Image2.Width), Math.Max(m_Image1.Height, m_Image2.Height));

            m_matching = new ShapeContextMatching(m_sourcePoints, m_targetPoints, m_commonSize, new SelectSamplesDelegate(SamplePoints));
        }

        public override Type MyType
        {
            get
            {
                return typeof(CShapeContext);
            }
        }

        public override ICData Run()
        {
            if (NumberOfSamples == Utils.sr_NoInit)
            {///if no amount of samples is set
                NumberOfSamples = Math.Min(m_sourcePoints.Length, m_targetPoints.Length) / 100; ///Hard reduction
                //NumberOfSamples = 25;
            }
            
            #region Default params override

            if (NumberOfIterations != Utils.sr_NoInit)
            {
                m_matching.NumOfIterations = NumberOfIterations;
            }
            if (NumberOfBins != Utils.sr_NoInit)
            {
                m_matching.NumOfBins = NumberOfBins;
            }
            if (NumberOfThetaBins!= Utils.sr_NoInit)
            {
                m_matching.NumOfThetaBins = NumberOfThetaBins;
            }

            #endregion

            m_matching.Calculate();

            CShapeContextResultData retResult = new CShapeContextResultData(
                m_sourcePoints,
                m_matching.ResultPoints,
                m_commonSize,
                m_matching.LastSourceSamples,
                m_matching.LastTargetSamples
                );

            return retResult;
        }

        public override object Instance
        {
            get { return this; }
        }

        #endregion

        private Point[] SamplePoints(Point[] i_FullSet, int i_NumOfSamples)
        {
            if (i_NumOfSamples > 0)
            {
                return Utils.GetIndexedSamplePoints(i_FullSet, i_NumOfSamples);
            }
            else
            {
                return Utils.GetIndexedSamplePoints(i_FullSet, NumberOfSamples);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <get>Return full matching object.</get>
        public ShapeContextMatching Matching
        {
            get
            {
                return m_matching;
            }
        }
    }

    public class CShapeContextResultData : ICData
    {
        private Bitmap  m_ResultingBitmap;

        private Point[] m_SourcePoints = null;
        private Point[] m_TargetPoints = null;

        private Point[] m_SourceSamples = null;
        private Point[] m_TargetSamples = null;

        private Size    m_CommonSize;

        #region C'tors

        public CShapeContextResultData(
            Point[] i_source,
            Point[] i_target,
            Size i_resultSize)
        {
            SourceColor  = Utilities.sr_defaultSourceColor;
            TargetColor  = Utilities.sr_defaultTargetColor;
            MatchesColor = Utilities.sr_defaultMatchingColor;

            m_CommonSize = i_resultSize;
            m_ResultingBitmap  = new Bitmap(i_resultSize.Width, i_resultSize.Height);
            m_SourcePoints      = i_source;
            m_TargetPoints      = i_target;
        }

        public CShapeContextResultData(
            Point[] i_source,
            Point[] i_target,
            Size i_resultSize,
            Point[] i_SourceSamples,
            Point[] i_TargetSamples)
            : this(i_source, i_target, i_resultSize)
        {
            m_SourceSamples = i_SourceSamples;
            m_TargetSamples = i_TargetSamples;
        }

        #endregion

        #region ICData Members

        public Image ResultImage
        {
            get 
            {
                Utilities.PutOnBitmap(ref m_ResultingBitmap, m_SourcePoints, SourceColor);
                Utilities.PutOnBitmap(ref m_ResultingBitmap, m_TargetPoints, TargetColor);

                if ((m_SourceSamples != null && m_TargetSamples != null) &&
                    (m_SourceSamples.Length == m_TargetSamples.Length))
                {
                    Graphics gfx = Graphics.FromImage(m_ResultingBitmap);
                    placeMatches(ref gfx, m_SourceSamples, m_TargetSamples, MatchesColoringConvension);
                    gfx.Dispose();
                }

                Image retImage = m_ResultingBitmap;

                return retImage;
            }
        }

        private void placeMatches(ref Graphics io_graphics, Point[] i_Source, Point[] i_Target, CloringConvension ColoringMethodHandler)
        {
            Pen linePen = new Pen(MatchesColor);

            for (int i = 0; i < i_Source.Length; ++i)
            {
                if (ColoringMethodHandler != null)
                {
                    linePen.Color = ColoringMethodHandler(linePen.Color, i);
                }

                io_graphics.DrawLine(linePen, i_Source[i], i_Target[i]);
            }
        }

        public Size OptimalImageSize
        {
            get 
            {
                return m_CommonSize;
            }
        }

        public Image SourceImage
        {
            get 
            {
                Bitmap retImage = new Bitmap(OptimalImageSize.Width, OptimalImageSize.Height);
                Utilities.PutOnBitmap(ref retImage, m_SourcePoints, SourceColor);
                return retImage;
            }
        }

        public Image TargetImage
        {
            get
            {
                Bitmap retImage = new Bitmap(OptimalImageSize.Width, OptimalImageSize.Height);
                Utilities.PutOnBitmap(ref retImage, m_TargetPoints, TargetColor);
                return retImage;
            }
        }

        public Type MyType
        {
            get 
            {
                return typeof(CShapeContextResultData);
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

        public Color SourceColor    { get; set; }
        public Color TargetColor    { get; set; }
        public Color MatchesColor   { get; set; }

        public delegate Color CloringConvension(Color i_prevColor, int i_IterationNum);
        public CloringConvension MatchesColoringConvension; ///A function of preveous color and iteration number
        
        #endregion

        #region PostRun Properties - over to ICData Members

        public Point[] SourceSamples
        {
            get
            {
                return m_SourcePoints;
            }
        }

        public Point[] TargetSamples
        {
            get
            {
                return m_TargetPoints;
            }
        }

        public List<Point> SourcePoints
        {
            get
            {
                return new List<Point>(m_SourcePoints);
            }
        }

        public List<Point> TargetPoints
        {
            get
            {
                return new List<Point>(m_TargetPoints);
            }
        }

        #endregion
    }
}
