using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using LiniarAlgebra;

namespace HausdorffDistance
{
    /// <Name>       Hausdorff distance mapping algorithm           </Name>
    /// <Version>           0.1a Pre release                        </Version>
    /// <FileName>         HausdorffMatching.cs                     </FileName>
    /// <ClassName>         HausdorffMatching                       </ClassName>
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
    /// <Date>              Sep. - 2009                             </Date>
    /// <License>
    /// This class library is free to use as long as it used unmodified, including the credits above.
    /// For modifications please contact with the creator to get an approval.
    /// </License>
    /// <summary>
    /// Having two binary maps (for example we can represent B/W drawing as binary map).
    /// Hausdorff distance mapping algorithm will create a new distance map that 
    /// represets the distance of each pixel in the first b.map(binary map) to the most
    /// nearest pixel in the second b.map.
    /// There are two types of Hausdorff distance maps that the algorithm knows to generate.
    ///     - Single side distance - Can be generates as first to second or as second to first b.maps
    ///                              this type will take one map and will represent the distances 
    ///                              from the other b.map.
    ///     - Double side distance - Will create distance map that is the union (the sum of the distances)
    ///                              of two single sided maps. ({the first from the second} union {the second from the first})
    ///                              
    /// </summary>
    /// <References>
    /// 
    /// </References>
    /// <SpecialThanks></SpecialThanks> 
    public class HausdorffMatching
    {
        private static readonly int sr_NoPixel  = 0;
        private static readonly int sr_Pixel = 1;

        private IntMatrix m_BinaryMap1 = null;
        private IntMatrix m_BinaryMap2 = null;
        private IntMatrix m_DistanceMap1 = null;
        private IntMatrix m_DistanceMap2 = null;
        private IntMatrix m_Map1onMap2 = null;
        private IntMatrix m_Map2onMap1 = null;
        private IntMatrix m_TwoSides   = null;

        public HausdorffMatching(IntMatrix i_BinaryMap1, IntMatrix i_BinaryMap2)
        {
            m_BinaryMap1 = i_BinaryMap1;
            m_BinaryMap2 = i_BinaryMap2;
        }

        public IntMatrix CalculateTwoSides()
        {
            m_TwoSides = new IntMatrix(Calculate1on2() + Calculate2on1());
            return m_TwoSides;
        }

        public IntMatrix Calculate1on2()
        {
            m_DistanceMap2 = CalcDistanceMatrix(m_BinaryMap2);
            m_Map1onMap2 = new IntMatrix(m_DistanceMap2.MultiplyOrgans(m_BinaryMap1));
            return m_Map1onMap2;
        }

        public IntMatrix Calculate2on1()
        {
            m_DistanceMap1 = CalcDistanceMatrix(m_BinaryMap1);
            m_Map2onMap1 = new IntMatrix(m_DistanceMap1.MultiplyOrgans(m_BinaryMap2));
            return m_Map2onMap1;
        }

        public IntMatrix ResultMap1
        {
            get
            {
                return m_Map2onMap1;
            }
        }

        public IntMatrix ResultMap2
        {
            get
            {
                return m_Map1onMap2;
            }
        }

        private IntMatrix CalcDistanceMatrix(IntMatrix i_BinaryMatrix)
        {
            IntMatrix retHausdorffMatrix = new IntMatrix(i_BinaryMatrix.RowsCount,i_BinaryMatrix.ColumnsCount);

            InitDistances(ref retHausdorffMatrix,i_BinaryMatrix);
            Queue<Point> pointQueue = binaryToQueue(i_BinaryMatrix);

            int minVal = 0;
            int currPointValue = 0;
            Point currPoint;
            int RowsCount = retHausdorffMatrix.RowsCount;
            int ColsCount = retHausdorffMatrix.ColumnsCount;
            Point surrounder;

            while (pointQueue.Count != 0)
            {
                currPoint = pointQueue.Dequeue();
                currPointValue = retHausdorffMatrix[currPoint];
                ++currPointValue;

                ///walking clockwise as shown:
                /// 2 |     3     | 4
                ///-------------------
                /// 1 | currPoint | 5
                ///-------------------
                /// 8 |     7     | 6
                /// 

                /// 1st location
                surrounder = currPoint;
                --surrounder.X;

                if (surrounder.X >= 0)
                {
                    if (currPointValue < retHausdorffMatrix[surrounder])
                    {
                        pointQueue.Enqueue(surrounder);
                    }

                    minVal = Math.Min(currPointValue, retHausdorffMatrix[surrounder]);
                    retHausdorffMatrix[surrounder] = minVal;
                }
                /// 2nd location
                ++surrounder.Y;
                if (surrounder.X >= 0 && surrounder.Y < RowsCount)
                {
                    if (currPointValue < retHausdorffMatrix[surrounder])
                    {
                        pointQueue.Enqueue(surrounder);
                    }

                    minVal = Math.Min(currPointValue, retHausdorffMatrix[surrounder]);
                    retHausdorffMatrix[surrounder] = minVal;
                }
                ///3rd location
                ++surrounder.X;
                if (surrounder.Y < RowsCount)
                {
                    if (currPointValue < retHausdorffMatrix[surrounder])
                    {
                        pointQueue.Enqueue(surrounder);
                    }

                    minVal = Math.Min(currPointValue, retHausdorffMatrix[surrounder]);
                    retHausdorffMatrix[surrounder] = minVal;
                }
                ///4th location
                ++surrounder.X;
                if (surrounder.Y < RowsCount && surrounder.X < ColsCount)
                {
                    if (currPointValue < retHausdorffMatrix[surrounder])
                    {
                        pointQueue.Enqueue(surrounder);
                    }

                    minVal = Math.Min(currPointValue, retHausdorffMatrix[surrounder]);
                    retHausdorffMatrix[surrounder] = minVal;
                }
                ///5th location
                --surrounder.Y;
                if (surrounder.X < ColsCount)
                {
                    if (currPointValue < retHausdorffMatrix[surrounder])
                    {
                        pointQueue.Enqueue(surrounder);
                    }

                    minVal = Math.Min(currPointValue, retHausdorffMatrix[surrounder]);
                    retHausdorffMatrix[surrounder] = minVal;
                }
                ///6th location
                --surrounder.Y;
                if (surrounder.X < ColsCount && surrounder.Y >= 0)
                {
                    if (currPointValue < retHausdorffMatrix[surrounder])
                    {
                        pointQueue.Enqueue(surrounder);
                    }

                    minVal = Math.Min(currPointValue, retHausdorffMatrix[surrounder]);
                    retHausdorffMatrix[surrounder] = minVal;
                }
                ///7th location
                --surrounder.X;
                if (surrounder.Y >= 0)
                {
                    if (currPointValue < retHausdorffMatrix[surrounder])
                    {
                        pointQueue.Enqueue(surrounder);
                    }

                    minVal = Math.Min(currPointValue, retHausdorffMatrix[surrounder]);
                    retHausdorffMatrix[surrounder] = minVal;
                }
                ///8th location
                --surrounder.X;
                if (surrounder.Y >= 0 && surrounder.X >= 0)
                {
                    if (currPointValue < retHausdorffMatrix[surrounder])
                    {
                        pointQueue.Enqueue(surrounder);
                    }

                    minVal = Math.Min(currPointValue, retHausdorffMatrix[surrounder]);
                    retHausdorffMatrix[surrounder] = minVal;
                }
            }

            return retHausdorffMatrix;
        }

        private Queue<Point> binaryToQueue(IntMatrix i_Surface)
        {
            Queue<Point> retQueue = new Queue<Point>();
            Func<int, int, int, int> onesToPoints = (row, col, val) =>
                {
                    if (val == 1)
                    {
                        retQueue.Enqueue(new Point(col, row));
                        return val;
                    }
                    else if (val == 0)
                    {
                        return 0;                       
                    }

                    throw new HausdorffMatchingException("A non binary value found in a binary matrix!!! Non binary values are not allowed!");
                };

            i_Surface.Iterate(onesToPoints);

            return retQueue;
        }

        private void InitDistances(ref IntMatrix o_DistMatrix, IntMatrix i_BinaryMapBase)
        {
            Func<int, int, int, int> ToDifferentSizedCopy = (row, col, val) =>
            {///Building a logic for one cell
                if (row <= i_BinaryMapBase.RowsCount && col <= i_BinaryMapBase.ColumnsCount)
                {
                    int baseVale = i_BinaryMapBase[row, col];
                    if (baseVale == 1)
                    {
                        return 0;
                    }
                    else if (baseVale == 0)
                    {
                        return Int16.MaxValue;
                    }
                    else
                    {
                        throw new HausdorffMatchingException("A non binary value found in a binary matrix!!! Non binary values are not allowed!");
                    }
                }
                else
                {
                    return 0;
                }
            };

            ///Apllying the logic for whole matrix
            o_DistMatrix.Iterate(ToDifferentSizedCopy);
        }
    }
}
