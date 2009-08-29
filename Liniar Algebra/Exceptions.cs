using System;

namespace LiniarAlgebra
{
    public class WrongDimensionsException : Exception
    {
        public WrongDimensionsException(string msg)
            : base (msg)
        {
        }
    }
}
