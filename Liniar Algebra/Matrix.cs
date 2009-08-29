using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LiniarAlgebra
{
    /// <Name>      Matrix representation for Liniar Algebra        </Name>
    /// <Version>           0.1a Pre release                        </Version>
    /// <FileName>          Matrix.cs                               </FileName>
    /// <ClassName>         Matrix<Type>                            </ClassName>
    /// <Creator>***************************************************************
    ///     <Name>          Yanir Taflev                            </Name>
    ///     <Email>         yanirta@gmail.com                       </Email>
    /// </Creator>**************************************************************
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
    /// This class generalize a Matrix representation of any type that can be used to implement ICalculator<Type>
    /// interface. The implementation supports all basic operations from Liniar Algebra.
    ///     For transperent specific implementation derive a new class and implement desired methods using
    /// base class's logic. Anyhow trasparent implementation is not must, 
    /// can be used also as specific with Matrix<specific>.
    /// </summary>
    /// <typeparam name="Type">Any Type that can be used to implement ICalculator<Type>. </typeparam>
    /// <References>
    /// </References>
    /// <SpecialThanks></SpecialThanks> 
    
    public class Matrix<Type>:ICloneable where Type : IComparable<Type>
    {
        protected Type[,] m_Matrix;
        private ICalculator<Type> m_Calculator;

        #region C'tors

        public Matrix(int i_Rows, int i_Columns, ICalculator<Type> i_Calculator)
        {
            m_Matrix = new Type[i_Rows, i_Columns];
            m_Calculator = i_Calculator;
        }

        /// <summary>
        /// Creating Square matrix
        /// </summary>
        /// <param name="i_NxN">i_NxN = i_Rows = i_Columns</param>
        public Matrix(int i_NxN, ICalculator<Type> i_Calculator)
            : this(i_NxN, i_NxN, i_Calculator)
        {
        }
        /// <summary>
        /// Can be used internally if internal stractures passed manually.
        /// Warning: Non wise usage will lead to errors,data loss and mistakes.
        /// </summary>
        /// <param name="i_Calculator"></param>
        private Matrix(ICalculator<Type> i_Calculator)
        {
            m_Calculator = i_Calculator;
        }

        /// <summary>
        /// Acts like a Copy constructor to avoid downcasting which exposes base type
        /// when deriving new specific class and making this to be transparent as much as possible.
        /// </summary>
        /// <param name="i_matrix">A matrix that the data will be taken from by-ref</param>
        protected Matrix(Matrix<Type> i_matrix)
        {
            m_Matrix = i_matrix.m_Matrix;
            m_Calculator = i_matrix.m_Calculator;
        }

        /// <summary>
        /// Another version of the same as above.
        /// Acts like a Copy constructor to avoid downcasting which exposes base type
        /// when deriving new specific class and making this to be transparent as much as possible.
        /// </summary>
        /// <param name="i_matrix">A matrix that the data will be taken from by-ref</param>
        /// <param name="i_Calculator">A calculator object implementation for the Type.</param>
        protected Matrix(Type[,] i_TypedMatrix, ICalculator<Type> i_Calculator)
        {
            m_Matrix = i_TypedMatrix;
            m_Calculator = i_Calculator;
        }
        #endregion

        #region Properties

        //Note that this accessebility as [Rows,Cols] is similar to [y,x]
        //And not as we used to in Cartesian systems
        public Type this[int i_Rows, int i_Columns]
        {
            get { return m_Matrix[i_Rows, i_Columns]; }
            set { m_Matrix[i_Rows, i_Columns] = value; }
        }

        public Type this[Point i_Location]
        {
            get { return m_Matrix[i_Location.Y, i_Location.X]; }
            set { m_Matrix[i_Location.Y, i_Location.X] = value; }
        }

        public int RowsCount
        {
            get { return m_Matrix.GetLength(0); }
        }

        public int ColumnsCount
        {
            get { return m_Matrix.GetLength(1); }
        }

        public int CellCount
        {
            get
            {
                return RowsCount * ColumnsCount;
            }
        }

        public ICalculator<Type> Calculator
        {
            get
            {
                return m_Calculator;
            }
        }

        public Type[,] InnerMatrix
        {
            get
            {
                return (Type[,])m_Matrix.Clone();
            }
        }

        #endregion

        #region Operators
        // The operators given as regular methods, for specific implementation
        // Prepare derived class and implement operators overloading using those operators
        
        /// <summary>
        /// Matrix + Matrix
        /// </summary>
        /// <param name="i_RightHandMatrix"></param>
        /// <returns></returns>
        public Matrix<Type> PlusOperator(Matrix<Type> i_RightHandMatrix)
        {
            if ((RowsCount != i_RightHandMatrix.RowsCount) ||
                (ColumnsCount != i_RightHandMatrix.ColumnsCount))
            {
                throw new WrongDimensionsException("Substract: Matrices need to have same dimension");
            }

            Matrix<Type> matrix =
                new Matrix<Type>(RowsCount, ColumnsCount, m_Calculator);

            for (int i = 0; i < RowsCount; ++i)
            {
                for (int j = 0; j < ColumnsCount; ++j)
                {
                    matrix[i, j] = m_Calculator.Add(m_Matrix[i, j], i_RightHandMatrix[i, j]);
                }
            }

            return matrix;
        }

        /// <summary>
        /// Matrix - Matrix
        /// </summary>
        /// <param name="i_RightHandMatrix"></param>
        /// <returns></returns>
        public Matrix<Type> MinusOperator(Matrix<Type> i_RightHandMatrix)
        {
            if ((RowsCount != i_RightHandMatrix.RowsCount) ||
                (ColumnsCount != i_RightHandMatrix.ColumnsCount))
            {
                throw new WrongDimensionsException("Substract: Matrices need to have same dimension");
            }

            Matrix<Type> matrix =
                new Matrix<Type>(RowsCount, ColumnsCount, m_Calculator);

            for (int i = 0; i < RowsCount; ++i)
            {
                for (int j = 0; j < ColumnsCount; ++j)
                {
                    matrix[i, j] = m_Calculator.Sub(m_Matrix[i, j],i_RightHandMatrix[i, j]);
                }
            }

            return matrix;
        }

        /// <summary>
        /// Matrix*scalar
        /// </summary>
        /// <param name="i_ScalarValue"></param>
        /// <returns></returns>
        public Matrix<Type> MultiplyOperator(Type i_ScalarValue)
        {
            Matrix<Type> matrix =
                new Matrix<Type>(RowsCount, ColumnsCount, m_Calculator);

            for (int i = 0; i < RowsCount; ++i)
            {
                for (int j = 0; j < ColumnsCount; ++j)
                {
                    matrix[i, j] = m_Calculator.Multiply(m_Matrix[i, j], i_ScalarValue);
                }
            }
            return matrix;
        }

        /// <summary>
        /// Matrix*Matrix
        /// </summary>
        /// <param name="i_Matrix1"></param>
        /// <param name="i_Matrix2"></param>
        /// <returns></returns>
        public Matrix<Type> MultiplyOperator(Matrix<Type> i_RightHandMatrix)
        {
            if (ColumnsCount != i_RightHandMatrix.RowsCount)
            {
                throw new WrongDimensionsException("Invalid dimension in multiply matrices");
            }

            Type sum = m_Calculator.Zero(); //Type cannot get number, just a value of function.

            Matrix<Type> matrix =
                new Matrix<Type>(RowsCount, i_RightHandMatrix.ColumnsCount, m_Calculator);

            for (int row = 0; row < RowsCount; ++row)
            {
                for (int column = 0; column < i_RightHandMatrix.ColumnsCount; ++column)
                {
                    sum = m_Calculator.Zero();
                    for (int VectorCellIndex = 0; VectorCellIndex < ColumnsCount; ++VectorCellIndex)
                    {
                        sum = m_Calculator.Add( sum,
                                                m_Calculator.Multiply(  m_Matrix[row, VectorCellIndex],
                                                                        i_RightHandMatrix.m_Matrix[VectorCellIndex, column]
                                                                        )
                                                );

                        //Same as:
                        //sum += m_Matrix[row, VectorCellIndex] * i_RightHandMatrix.m_Matrix[VectorCellIndex, column]
                        //But because of abstruction of Type cannot be done directly
                    }

                    matrix[row, column] = sum;
                }
            }

            return matrix;
        }
        #endregion

        #region Utility functions
        /// <summary>
        /// Setting a uniform value for whole matrix
        /// </summary>
        /// <param name="i_Value">A value to set</param>
        public void Init(Type i_Value)
        {
            int numOfRows = RowsCount,
                numOfColumns = ColumnsCount;

            for (int i = 0; i < numOfRows; ++i)
            {
                for (int j = 0; j < numOfColumns; ++j)
                {
                    m_Matrix[i, j] = i_Value;
                }
            }
        }

        /// <summary>
        /// Iterates and apply function on the organs of the matrix
        /// </summary>
        /// <param name="IterationFunc">Manipulation function</param>
        public void Iterate(Func<Type, Type> IterationFunc)
        {
            for (int i = 0; i < RowsCount; ++i)
            {
                for (int j = 0; j < ColumnsCount; ++j)
                {
                    m_Matrix[i, j] = IterationFunc(m_Matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Iterates and apply function on the organs of the matrix
        /// </summary>
        /// <param name="IterationFunc">Manipulation function with three input parameters
        /// the first for row location
        /// the second for col location
        /// the third for cell value
        /// Output param after manipulation</param>
        public void Iterate(Func<int, int, Type, Type> IterationFunc)
        {
            for (int i = 0; i < RowsCount; ++i)
            {
                for (int j = 0; j < ColumnsCount; ++j)
                {
                    m_Matrix[i, j] = IterationFunc(i,j,m_Matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Matrix + Scalar (for each object in Matrix)
        /// </summary>
        /// <param name="i_ScalarValue"></param>
        /// <returns></returns>
        public Matrix<Type> AddScalarOperator(Type i_ScalarValue)
        {
            Matrix<Type> matrix =
                new Matrix<Type>(RowsCount, ColumnsCount, m_Calculator);

            for (int i = 0; i < RowsCount; ++i)
            {
                for (int j = 0; j < ColumnsCount; ++j)
                {
                    matrix[i, j] = m_Calculator.Add(m_Matrix[i, j], i_ScalarValue);
                }
            }
            return matrix;
        }

        /// <summary>
        /// Matrix .* Matrix (Yes it is ".*")
        /// A[i,j] * B[i,j] => result matrix [i,j]
        /// </summary>
        /// <param name="i_RightHandMatrix">Right hand matrix with the same size</param>
        /// <returns>A new matrix that is the multiplication result of corresponding cells</returns>
        public Matrix<Type> MultiplyOrgans(Matrix<Type> i_RightHandMatrix)
        {
            if ((RowsCount != i_RightHandMatrix.RowsCount) ||
                (ColumnsCount != i_RightHandMatrix.ColumnsCount))
            {
                throw new WrongDimensionsException("Substract: Matrices need to have same dimension");
            }

            Matrix<Type> matrix =
                new Matrix<Type>(RowsCount, ColumnsCount, m_Calculator);

            for (int i = 0; i < RowsCount; ++i)
            {
                for (int j = 0; j < ColumnsCount; ++j)
                {
                    matrix[i, j] = m_Calculator.Multiply(m_Matrix[i, j], i_RightHandMatrix[i, j]);
                }
            }

            return matrix;
        }

        /// <summary>
        /// Matrix / Scalar
        /// </summary>
        /// <param name="i_Divider"></param>
        /// <returns>A new matrix that is devision by scalar result</returns>
        public Matrix<Type> DivisionByScalarOperator(Type i_Divider)
        {
            if (i_Divider.CompareTo(m_Calculator.Zero()) == 0)
            {
                throw new DivideByZeroException("Division by zero on Matrix type");
            }
            return MultiplyOperator(m_Calculator.Division(m_Calculator.One(),i_Divider));
            //Same as this .* (1/i_Divider);
        }

        /// <summary>
        /// Generic cell summing algorithm
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="i_Matrix"></param>
        /// <returns></returns>
        public Type Sum()
        {
            Type retSum = m_Calculator.Zero();

            Func<Type, Type> summingOp = CellValue =>
            {
                retSum = m_Calculator.Add(retSum, CellValue);
                return CellValue;
            };

            this.Iterate(summingOp);
            return retSum;
        }

        /// <summary>
        /// Summerize each row.
        /// 
        /// </summary>
        /// <returns>M x 1 where M is row count of this matrix. Each cell will store
        /// the summing result for the corresponding index.</returns>
        public Matrix<Type> SumRows()
        {
            Matrix<Type> sumVector = new Matrix<Type>(RowsCount, 1, m_Calculator);

            for (int i = 0; i < RowsCount; ++i)
            {
                Type currRowSum = m_Calculator.Zero();
                for (int j = 0; j < ColumnsCount; ++j)
                {
                    currRowSum = m_Calculator.Add(currRowSum, m_Matrix[i, j]);
                }
                sumVector[i,0] = currRowSum;
            }

            return sumVector;
        }

        /// <summary>
        /// Summerize each row.
        /// 
        /// </summary>
        /// <returns>1 x M where M is columns count of this matrix. Each cell will store
        /// the summing result for the corresponding index.</returns>
        public Matrix<Type> SumColumns()
        {
            Matrix<Type> sumVector = new Matrix<Type>(0, ColumnsCount, m_Calculator);

            for (int i = 0; i < ColumnsCount; ++i)
            {
                Type currColSum = m_Calculator.Zero();
                for (int j = 0; j < RowsCount; ++j)
                {
                    currColSum = m_Calculator.Add(currColSum, m_Matrix[j, i]);
                }
                sumVector[0, i] = currColSum;
            }

            return sumVector;
        }

        /// <summary>
        /// Transpose of the matrix
        /// </summary>
        /// <returns>(new) return[i,j] = this[j,i]</returns>
        public Matrix<Type> Transpose()
        {
            int rows = RowsCount;
            int columns = ColumnsCount;

            Matrix<Type> retTransposed = new Matrix<Type>(columns, rows, Calculator);

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < columns; ++j)
                {
                    retTransposed[j, i] = m_Matrix[i, j];
                }
            }

            return retTransposed;
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            Matrix<Type> colnedMatrix = new Matrix<Type>(m_Calculator);
            colnedMatrix.m_Matrix = m_Matrix.Clone() as Type[,];
            return colnedMatrix;
        }

        #endregion

        #region Default operators - prefer to not use them directly when deriving (will lead to downcasting)
        public static Matrix<Type> operator +(Matrix<Type> i_LeftHand, Matrix<Type> i_RightHand) { return i_LeftHand.PlusOperator(i_RightHand); }

        public static Matrix<Type> operator *(Matrix<Type> i_LeftHand, Matrix<Type> i_RightHand) { return i_LeftHand.MultiplyOperator(i_RightHand); }

        public static Matrix<Type> operator *(Matrix<Type> i_LeftHand, Type i_RightHand) { return i_LeftHand.MultiplyOperator(i_RightHand); }

        public static Matrix<Type> operator /(Matrix<Type> i_LeftHand, Type i_RightHand) { return i_LeftHand.DivisionByScalarOperator(i_RightHand); }

        public static Matrix<Type> operator -(Matrix<Type> i_LeftHand, Matrix<Type> i_RightHand) { return i_LeftHand.MinusOperator(i_RightHand); }

        #endregion
    }

    #region Icalculator
    /// <summary>
    /// This class used to get over a problem of calculations within generic Matrix class.
    /// 
    /// Implement this interface for single type (not a collection)
    /// For example for Int32 public override Int32 Sum(Int32 a,Int32 b){return a+b};
    /// </summary>
    /// <typeparam name="Type">A numeric types only, otherwise no meaning</typeparam>
    public interface ICalculator<Type> where Type:IComparable<Type>
    {
        Type Add(Type i_LeftOperator, Type i_RightOperator);
        Type Sub(Type i_LeftOperator, Type i_RightOperator);
        Type Multiply(Type i_LeftOperator, Type i_RightOperator);
        Type Division(Type i_LeftOperator, Type i_RightOperator);
        #region Specific types convertions
        double ToDouble(Type i_ValueToConvert);
        Type FromDouble(double i_ValueToConvert);
        #endregion
        #region Special Numbers
        Type Zero();
        Type One();
        #endregion
    }
    #endregion
}
