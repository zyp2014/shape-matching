using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;

using LiniarAlgebra;
using HausdorffDistance;
namespace Adaption
{
    public class CHausdorffDistance : AlgorithmBase
    {
        private Image m_Image1 = null;
        private Image m_Image2 = null;

        private IntMatrix m_BinaryMap1 = null;
        private IntMatrix m_BinaryMap2 = null;

        private IntMatrix m_Result = null;

        public CHausdorffDistance()
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
                return typeof(CHausdorffDistance);
            }
        }

        public override ICData Run()
        {
            Size CommonCanvasSize = new Size(Math.Max(m_Image1.Width, m_Image2.Width), Math.Max(m_Image1.Height, m_Image2.Height));

            m_BinaryMap1 = Utilities.ToBinaryMap(m_Image1, TresholdColor, CommonCanvasSize);
            m_BinaryMap2 = Utilities.ToBinaryMap(m_Image2, TresholdColor, CommonCanvasSize);

            
            HausdorffMatching matching = new HausdorffMatching(m_BinaryMap1, m_BinaryMap2);
            m_Result = matching.CalculateTwoSides();

            return new HausdorffMatchingResult(matching.ResultMap1, matching.ResultMap2, m_Result);
        }

        public override object Instance
        {
            get { return this; }
        }

        #endregion

        #region preRun Properties
        public Color TresholdColor { get; set; }
        #endregion


    }

    public class HausdorffMatchingResult : ResultBase
    {
        private IntMatrix m_Result  = null;
        private IntMatrix m_Side1   = null;
        private IntMatrix m_Side2   = null;

        public HausdorffMatchingResult(IntMatrix i_Side1,IntMatrix i_Side2,IntMatrix i_Result)
        {
            m_Side1 = i_Side1;
            m_Side2 = i_Side2;
            m_Result = i_Result;
            ColoringFunction = defaultColoringConvension;
        }

        #region ResultBase Members

        public override Image ResultImage
        {
            get
            {
                return Utilities.ToBitmap(m_Result, ColoringFunction);
            }
        }

        public override Image SourceImage
        {
            get 
            {
                return Utilities.ToBitmap(m_Side1, ColoringFunction);
            }
        }

        public override Image TargetImage
        {
            get 
            {
                return Utilities.ToBitmap(m_Side2, ColoringFunction);
            }
        }

        public override Size OptimalImageSize
        {
            get 
            {
                return new Size(m_Result.ColumnsCount, m_Result.RowsCount);
            }
        }

        public override Type MyType
        {
            get
            {
                return typeof(HausdorffMatchingResult);
            }
        }

        public override PropertyInfo[] PropertyList
        {
            get 
            {
                return MyType.GetProperties();
            }
        }

        #region Obsolete Members

        public override Color SourceColor
        {
            get
            {
                return Color.Empty;
            }
            set
            {

            }
        }
        
        public override Color TargetColor
        {
            get
            {
                return Color.Empty;
            }
            set
            {

            }
        }

        #endregion

        #endregion

        #region PostRun Properties - over to ICData Members
        public delegate Color ColoringConvension(int i_Value,int i_LocalMax);
        public ColoringConvension ColoringFunction;

        private Color defaultColoringConvension(int i_Value, int i_LocalMax)
        {
            return Color.FromArgb(255 - (int)Math.Round(255 / (double)i_LocalMax * Math.Log(i_Value + 1,10)), 255 - (int)Math.Round(255 / (double)i_LocalMax * Math.Log(i_Value + 1, 2)), 255 - (int)Math.Round(255 / (double)i_LocalMax * i_Value));
            ///Color.FromArgb(((int)Math.Round(int.MaxValue / (double)i_LocalMax * Math.Pow(i_Value,2))));
            ///Color.FromArgb(((int)Math.Round(int.MaxValue / (double)i_LocalMax * i_Value)));
            ///Color.FromArgb(((int)Math.Round((double)255 / (double)i_LocalMax * i_Value)), ((int)Math.Round((double)255 / (double)i_LocalMax * i_Value)), ((int)Math.Round((double)255 / (double)i_LocalMax * i_Value)));
            ///Color.FromArgb(i_Value, i_Value, i_Value);
            ///Color.FromArgb(int.MaxValue - ((int)Math.Round(i_Value / (double)i_LocalMax * int.MaxValue)));
        }
        #endregion


    }
}
