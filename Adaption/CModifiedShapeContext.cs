using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LiniarAlgebra;
using PCA;

namespace Adaption
{
    public class CModifiedShapeContext : CShapeContext
    {
        private DoubleMatrix m_SourcePointsCordinates;

        public override void Create(Image i_SourceImage, Image i_TargetImage)
        {
            base.Create(i_SourceImage, i_TargetImage);
            
            m_SourcePointsCordinates = new DoubleMatrix(LiniarAlgebraFunctions.PointArrayToMatrix(m_sourcePoints, null).Transpose());
            Matching.OnIterationEnd = PCAAlignment;

        }

        public override ICData Run()
        {
            return base.Run();
        }

        private void PCAAlignment(ref Point[] i_PointsToAlign)
        {
            PCA.PCAMatching matching = new PCA.PCAMatching(
                m_SourcePointsCordinates,
                new DoubleMatrix(LiniarAlgebraFunctions.PointArrayToMatrix(i_PointsToAlign, null).Transpose()));
            matching.Calculate();

            DoubleMatrix result = matching.Result;
            DoubleMatrix minMaxTarget = Utils.ShiftToPositives(ref result, m_SourcePointsCordinates);

            i_PointsToAlign = LiniarAlgebraFunctions.MatrixToPointArray(new DoubleMatrix(result.Transpose()));

            
            m_commonSize = new Size((int)Math.Max(minMaxTarget[sr_X, Utils.sr_MaxRow], m_commonSize.Width),
                                    (int)Math.Max(minMaxTarget[sr_Y, Utils.sr_MaxRow], m_commonSize.Height));
            
        }
    }
}
