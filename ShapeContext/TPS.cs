using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiniarAlgebra;
using System.Drawing;

namespace ShapeContext
{
    public class TPS
    {
        private static readonly int sr_Xaxis = 0;
        private static readonly int sr_Yaxis = 1;
        private static readonly int sr_TPSminNumOfSamples = 4;

        private DoubleMatrix m_SourceMapping = null;
        private DoubleMatrix m_PixelMapping = null;
        private Size         m_MeshSize;

        public TPS(Size i_MeshSize)
            : this(i_MeshSize, null)
        { }

        public TPS(Size i_MeshSize, DoubleMatrix i_SourceMapping)
        {
            m_MeshSize = i_MeshSize;
            m_SourceMapping = i_SourceMapping;
        }

        /// <summary>
        /// Calculates Thin Plate Splines warping
        /// </summary>
        /// <param name="i_SourceMapping">Source Mapping points Nx2</param>
        /// <param name="i_TargetMapping">Target Mapping points Nx2</param>
        /// <param name="io_FullSet">The full set of points to wrap</param>
        public void Calculate(DoubleMatrix i_SourceMapping, DoubleMatrix i_TargetMapping, ref DoubleMatrix io_FullSet)
        {
            if (i_TargetMapping.RowsCount >= sr_TPSminNumOfSamples)
            {
                m_SourceMapping = i_SourceMapping;
                m_PixelMapping = pixelMapping(m_SourceMapping, i_TargetMapping, m_MeshSize);
                interpolate2D(m_MeshSize, m_PixelMapping, ref io_FullSet);                
            }
        }

        public void Calculate(DoubleMatrix i_SourceMapping, DoubleMatrix i_TargetMapping, ref Point[] io_FullSet)
        {
            if (i_TargetMapping.RowsCount >= sr_TPSminNumOfSamples)
            {
                m_SourceMapping = i_SourceMapping;
                m_PixelMapping = pixelMapping(m_SourceMapping, i_TargetMapping, m_MeshSize);
                interpolate2D(m_MeshSize, m_PixelMapping, ref io_FullSet);
            }
        }

        public void Calculate(DoubleMatrix i_SourceMapping, DoubleMatrix i_TargetMapping)
        {
            if (i_TargetMapping.RowsCount >= sr_TPSminNumOfSamples)
            {
                m_SourceMapping = i_SourceMapping;
                m_PixelMapping = pixelMapping(m_SourceMapping, i_TargetMapping, m_MeshSize);
            }
        }

        public void Calculate(DoubleMatrix i_TargetMapping)
        {
            if (m_SourceMapping != null)
            {
                m_PixelMapping = pixelMapping(m_SourceMapping, i_TargetMapping, m_MeshSize);
            }
        }

        public void Interpolate(ref Point[] io_Points)
        {
            if (m_PixelMapping != null)
            {
                interpolate2D(m_MeshSize, m_PixelMapping, ref io_Points);
            }
        }

        private static DoubleMatrix pixelMapping(DoubleMatrix i_SourceMapping, DoubleMatrix i_TargetMapping, Size i_MeshSize)
        {
            if (i_SourceMapping.RowsCount != i_TargetMapping.RowsCount ||
                i_SourceMapping.ColumnsCount != i_TargetMapping.ColumnsCount)
            {
                throw new TPSException("Thin Plate Splines algorithm cannot map source to target with different sizes.");
            }

            DoubleMatrix mapping = mapLandMarks(i_SourceMapping);
            DoubleMatrix targetVector = mapTargets(i_TargetMapping);
            if (mapping.Inverse() == false)
            {
                throw new TPSException("Inverse matrix calculation failed.");
            }
            DoubleMatrix remapped = mapping * targetVector;
            DoubleMatrix newMap = tpsMAP(remapped, i_MeshSize, i_SourceMapping);
            return newMap;
        }        

        private static void interpolate2D(Size i_MeshSize, DoubleMatrix newMap, ref Point[] io_FullSet)
        {
            for (int row = 0; row < io_FullSet.Length; ++row)
            {

                double Xval = io_FullSet[row].X;
                double Yval = io_FullSet[row].Y;

                int fsIndex = (int)(i_MeshSize.Width * Yval + Xval);

                io_FullSet[row].X = (int)Math.Max(0, Math.Min(Math.Round(newMap[fsIndex, sr_Yaxis]), i_MeshSize.Width - 1));
                io_FullSet[row].Y = (int)Math.Max(0, Math.Min(Math.Round(newMap[fsIndex, sr_Xaxis]), i_MeshSize.Height - 1));

            }
        }

        private static void interpolate2D(Size i_MeshSize, DoubleMatrix newMap, ref DoubleMatrix io_FullSet)
        {
            for (int row = 0; row < io_FullSet.RowsCount; ++row)
			{

                double Xval = io_FullSet[row, sr_Xaxis];
                double Yval = io_FullSet[row, sr_Yaxis];

                int fsIndex = (int)(i_MeshSize.Width * Yval + Xval);

                io_FullSet[row, sr_Xaxis] = Math.Max(0, Math.Min(Math.Round(newMap[fsIndex, sr_Yaxis]), i_MeshSize.Width - 1));
                io_FullSet[row, sr_Yaxis] = Math.Max(0, Math.Min(Math.Round(newMap[fsIndex, sr_Xaxis]), i_MeshSize.Height - 1));

			}
        }

        private static DoubleMatrix tpsMAP(DoubleMatrix remapped, Size i_MeshSize, DoubleMatrix i_SourceMapping)
        {
            DoubleMatrix TPSmap = new DoubleMatrix(i_MeshSize.Height * i_MeshSize.Width, i_SourceMapping.RowsCount + 3);

            for (int height = 0; height < i_MeshSize.Height; ++height)
            {
                for (int width = 0; width < i_MeshSize.Width; ++width)
                {
                    for (int mapIndex = 0; mapIndex < i_SourceMapping.RowsCount; ++mapIndex)
                    {
                        double result = Math.Sqrt(
                            Math.Pow(i_SourceMapping[mapIndex, sr_Xaxis] - height, 2) +
                            Math.Pow(i_SourceMapping[mapIndex, sr_Yaxis] - width, 2));

                        ///Radial basis
                        if (result != 0)
                        {
                            result = 2 * Math.Pow(result, 2) * Math.Log(result); 
                        }

                        TPSmap[height * i_MeshSize.Width + width, mapIndex] = result;
                    }
                    TPSmap[height * i_MeshSize.Width + width, i_SourceMapping.RowsCount] = 1;
                    TPSmap[height * i_MeshSize.Width + width, i_SourceMapping.RowsCount + sr_Xaxis + 1] = height;
                    TPSmap[height * i_MeshSize.Width + width, i_SourceMapping.RowsCount + sr_Yaxis + 1] = width;
                }
            }

            return TPSmap * remapped;
        }

        private static DoubleMatrix mapTargets(DoubleMatrix i_TargetMapping)
        {
            DoubleMatrix retNormalized = new DoubleMatrix(i_TargetMapping.RowsCount + 3, i_TargetMapping.ColumnsCount);
            Func<int, int, double, double> transferCell = (row, col, value) =>
                {
                    if (row >= i_TargetMapping.RowsCount)
                    {
                        return 0;
                    }
                    else
                    {
                        return i_TargetMapping[row, col];
                    }
                };

            retNormalized.Iterate(transferCell);
            return retNormalized;
        }

        private static DoubleMatrix mapLandMarks(DoubleMatrix i_SourceMapping)
        {
            DoubleMatrix retMapping = new DoubleMatrix(i_SourceMapping.RowsCount + 3);

            ///Calculating square distances
            for (int i = 0; i < i_SourceMapping.RowsCount; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    double result = Math.Sqrt(
                        Math.Pow(i_SourceMapping[i, sr_Xaxis] - i_SourceMapping[j, sr_Xaxis], 2) +
                        Math.Pow(i_SourceMapping[i, sr_Yaxis] - i_SourceMapping[j, sr_Yaxis], 2));

                    ///Radial basis
                    if (result != 0)
                    {
                        result = 2 * Math.Pow(result, 2) * Math.Log(result);                       
                    }

                    retMapping[i, j] = retMapping[j, i] = result;
                }
            }
            ///Adding parametric details
            for (int row = 0; row < i_SourceMapping.RowsCount; ++row)
            {
                retMapping[row, i_SourceMapping.RowsCount] = retMapping[i_SourceMapping.RowsCount, row] = 1;
                retMapping[row, i_SourceMapping.RowsCount + sr_Xaxis + 1] = retMapping[i_SourceMapping.RowsCount + sr_Xaxis + 1, row] = i_SourceMapping[row, sr_Xaxis];
                retMapping[row, i_SourceMapping.RowsCount + sr_Yaxis + 1] = retMapping[i_SourceMapping.RowsCount + sr_Yaxis + 1, row] = i_SourceMapping[row, sr_Yaxis];
            }

            ///Adding Affine params
            for (int row = i_SourceMapping.RowsCount; row < i_SourceMapping.RowsCount + 3; ++row)
            {
                for (int col = i_SourceMapping.RowsCount; col < i_SourceMapping.RowsCount + 3; ++col)
                {
                    retMapping[row, col] = 0;
                }
            }

            return retMapping;
        }
    }
}
