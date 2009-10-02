using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using LiniarAlgebra;

namespace Adaption
{
    public static class Utilities
    {
        public static readonly int sr_Xrow      = 0;
        public static readonly int sr_Yrow      = 1;
        public static readonly int sr_NoLine    = 0;
        public static readonly int sr_Line      = 1;

        public static readonly Color sr_defaultTresholdColor    = Color.FromArgb(200, 200, 200);
        public static readonly Color sr_defaultSourceColor      = Color.Red;
        public static readonly Color sr_defaultTargetColor      = Color.Green;
        public static readonly Color sr_defaultMatchingColor    = Color.Black;

        public static List<Point> ExtractPoints(Image i_Image,Color i_SelectTreshold)
        {
            List<Point> retPoinList = new List<Point>();
            Bitmap bitmap = new Bitmap(i_Image);
            Color currPixel;

            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    currPixel = bitmap.GetPixel(j, i);

                    if (currPixel.R < i_SelectTreshold.R &&
                        currPixel.G < i_SelectTreshold.G &&
                        currPixel.B < i_SelectTreshold.B)
                    {
                        retPoinList.Add(new Point(j, i));
                    }
                }
            }
            return retPoinList;
        }

        public static Point[] ListToArray(List<Point> i_PointList)
        {
            Point[] retPointArr = i_PointList.ToArray<Point>();
            return retPointArr;
        }

        public static DoubleMatrix ListToMatrix(List<Point> i_PointList)
        {
            DoubleMatrix retMatrix = new DoubleMatrix(2, i_PointList.Count);
            int matrixIdx = 0;
            foreach (Point pt in i_PointList)
            {
                retMatrix[sr_Xrow, matrixIdx] = pt.X;
                retMatrix[sr_Yrow, matrixIdx] = pt.Y;
                ++matrixIdx;
            }

            return retMatrix;
        }

        public static void PutOnBitmap(ref Bitmap io_Bitmap, Point[] i_Source, Color i_Color)
        {
            foreach (Point pt in i_Source)
            {
                io_Bitmap.SetPixel(pt.X, pt.Y, i_Color);
            }
        }

        public static void PutOnBitmap(ref Bitmap io_Bitmap, DoubleMatrix i_Source, Color i_Color)
        {
            for (int col = 0; col < i_Source.ColumnsCount; ++col)
            {
                io_Bitmap.SetPixel((int)Math.Round(i_Source[sr_Xrow, col]), (int)Math.Round(i_Source[sr_Yrow, col]), i_Color);
            }
        }

        public static List<Point> MatrixToList(DoubleMatrix m_Source)
        {
            List<Point> retList = new List<Point>(m_Source.ColumnsCount);
            
            for (int col = 0; col < m_Source.ColumnsCount; ++col)
            {
                Point currPoint = new Point((int)Math.Round(m_Source[sr_Xrow, col]), (int)Math.Round(m_Source[sr_Yrow, col]));
                retList.Add(currPoint);
            }

            return retList;
        }

        public static IntMatrix ToBinaryMap(Image i_GrayScaleImage, Color i_Treshold, Size i_newCanvasSize)
        {
            IntMatrix retBinaryMap = new IntMatrix(i_newCanvasSize.Height, i_newCanvasSize.Width);
            Bitmap bitmap = new Bitmap(i_GrayScaleImage);
            ///Logic for single cell
            Func<int, int, int, int> filterBlacks = (row, col, val) =>
                {
                    if (row < bitmap.Height && col < bitmap.Width)
                    {
                        Color currColor = bitmap.GetPixel(col, row);
                        if (currColor.R < i_Treshold.R &&
                            currColor.G < i_Treshold.G &&
                            currColor.B < i_Treshold.B)
                        {
                            return sr_Line;
                        }
                        else
                        {
                            return sr_NoLine;
                        }
                    }
                    else
                    {
                        return sr_NoLine;
                    }

                };
            ///Applying logic for whole matrix
            retBinaryMap.Iterate(filterBlacks);

            return retBinaryMap;
        }

        public static IntMatrix ToBinaryMap(DoubleMatrix i_PointSet, Size i_newCanvasSize)
        {
            IntMatrix retMap = new IntMatrix(i_newCanvasSize.Height, i_newCanvasSize.Width);
            retMap.Init(sr_NoLine);

            for (int col = 0; col < i_PointSet.ColumnsCount; ++col)
            {
                int PixelRow = (int)i_PointSet[sr_Yrow, col];
                int PixelCol = (int)i_PointSet[sr_Xrow, col];
                retMap[PixelRow, PixelCol] = sr_Line;
            }
            return retMap;
        }

        public static Bitmap ToBitmap(IntMatrix i_Matrix, HausdorffMatchingResult.ColoringConvension ColoringFunction)
        {
            Bitmap retBitmap = new Bitmap(i_Matrix.ColumnsCount, i_Matrix.RowsCount);
            int LocalMax = DetermineMax(i_Matrix);
            //Logic for single cell
            Func<int, int, int, int> bitmapSet = (row, col, val) =>
                {
                    retBitmap.SetPixel(col, row, ColoringFunction(val, LocalMax));
                    return val;
                };
            ///Applying logic for each cell
            i_Matrix.Iterate(bitmapSet);

            return retBitmap;
        }

        public static int DetermineMax(IntMatrix i_Matrix)
        {
            int retMax = int.MinValue;
            Func<int, int> maxFinder = (Value) =>
                {
                    retMax = Math.Max(retMax, Value);
                    return Value;
                };

            i_Matrix.Iterate(maxFinder);
            return retMax;
        }
    }
}
