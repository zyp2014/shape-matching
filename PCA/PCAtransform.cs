using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiniarAlgebra;

namespace PCA
{
    /// <Name>              PCA calculation algorithm               </Name>
    /// <Version>           0.1a Pre release                        </Version>
    /// <FileName>          PCAtransform.cs                         </FileName>
    /// <ClassName>         PCAtransform                            </ClassName>
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
    ///The following code was created by Yanir Taflev. 
    ///It is released for use under the AFL v3.0 (Academic Free License):
    ///This program is open source.  For license terms, see the AFL v3.0 terms at http://www.opensource.org/licenses/afl-3.0.php.
    ///Copyright (c) 2008 - 2009, Y. Taflev
    ///All rights reserved.
    ///
    ///     Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
    ///
    ///         * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    ///         * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    ///
    ///     THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
    ///     "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
    ///     LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
    ///     A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
    ///     CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
    ///     EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
    ///     PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
    ///     PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
    ///     LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
    ///     NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
    ///     SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    /// </License>
    /// <summary>
    /// The PCA algorithm applied on two dimensional matrix, when each row represent a dimension, 
    /// each column represent values of a vector (multi-dimensional vector).
    ///     Assuming the input matrix is M x N size (M rows and N columns), the output after running a PCA transform on it
    /// will be symmetric matrix M x M size, with orthogonal vectors directing to the main course of the deviation of the data
    /// from the original matrix (the new axis that represent the most data aroud him). Those are the eigen vectors of the covarience matrix.
    ///     Another type of results is array of M eigen values, those can be treated as STD. deviation values,
    /// and can be used to normalize the data by axis. (to get few sets to a uniform representation).
    /// Remark: I found that those eigen values are best fitting to be used for normalization ,when using a square root of them.
    /// </summary>
    /// <References>
    /// http://en.wikipedia.org/wiki/Principal_component_analysis
    /// http://www.cs.otago.ac.nz/cosc453/student_tutorials/principal_components.pdf
    /// Not used as a source reference but will help to understand:
    /// http://www.cs.cmu.edu/~elaw/papers/pca.pdf
    /// http://genetics.agrsci.dk/statistics/courses/Rcourse-DJF2006/day3/PCA-computing.pdf
    /// </References>
    /// <SpecialThanks></SpecialThanks>
    public class PCAtransform
    {
        #region Private members

        private DoubleMatrix m_DimsAvg;
        private DoubleMatrix m_PointsMatrix;
        private DoubleMatrix m_CenteredPoints;
        private DoubleMatrix m_EigenVectors;
        private double[]     m_EigenValues;

        #endregion

        /// <summary>
        /// Creating a PCA transformation based on point set converted into DoubleMatrix
        /// </summary>
        /// <param name="i_MxNpoints">MxN matrix - N vectors,each vector M sized.</param>
        public PCAtransform(DoubleMatrix i_MxNpoints) 
        {
            m_PointsMatrix = i_MxNpoints;
        }

        /// <summary>
        /// Calculating a PCA transformation based on point set that given as DoubleMatrix in the c'tor
        /// </summary>
        /// <returns>An M x M matrix representing the main axises of the data deviation</returns>
        public DoubleMatrix Calculate()
        {
            m_DimsAvg = new DoubleMatrix(m_PointsMatrix.SumRows() / m_PointsMatrix.ColumnsCount);
            m_CenteredPoints = new DoubleMatrix(m_PointsMatrix);
            Utils.SubstractScalarsByDims(ref m_CenteredPoints, m_DimsAvg);
            DoubleMatrix covMatrix = new DoubleMatrix(LiniarAlgebraFunctions.Covarience<double>(m_CenteredPoints));
            m_EigenVectors = LiniarAlgebraFunctions.EigenMatrix(covMatrix, out m_EigenValues);
            return m_EigenVectors;
        }

        #region Resulting properties (Values)
        /// <summary>
        /// Return M x 1 vector each cell stores the averege for its dimension.
        /// Note:The value will be valid only after running Calculate() mathod.
        /// </summary>
        public DoubleMatrix AverageByDimension 
        {
            get
            {
                return m_DimsAvg;
            }
        }

        /// <summary>
        /// Returns the eigen vectors matrix, according to the calculations from Calculate() method.
        /// Note:The value will be valid only after running Calculate() mathod.
        /// </summary>
        public DoubleMatrix EigenVectors
        {
            get
            {
                return m_EigenVectors;
            }
        }

        /// <summary>
        /// Returns a set of points after centrezation,
        /// actually after each point was substraced by the average value for each dimension.
        /// Note:The value will be valid only after running Calculate() mathod. 
        /// </summary>
        public DoubleMatrix CenteredPoints
        {
            get
            {
                return m_CenteredPoints;
            }
        }

        /// <summary>
        ///Returns an angle of the new axis , comparing to the old in radians.
        /// Note:The value will be valid only after running Calculate() mathod. 
        /// </summary>
        /// <param name="i_FirstAxis">The axis that we treat as X axis</param>
        /// <param name="i_SecondAxis">The axis that we treat as Y axis</param>
        /// <returns>The angle that the data need to be transformed (back) to, in order to be at as much as possible
        /// near the X axis</returns>
        public double GetAngle(int i_FirstAxis, int i_SecondAxis)
        {
            return Math.Atan(m_EigenVectors[0, i_FirstAxis] / m_EigenVectors[0, i_SecondAxis]);
        }

        /// <summary>
        /// Returns an array of eigen values that are the squares of the normalization parameters
        /// for each dimension by it's index.
        /// Note:The value will be valid only after running Calculate() mathod. 
        /// </summary>
        public double[] EigenValues
        {
            get
            {
                return m_EigenValues;
            }
        }

        #endregion
    }
}
