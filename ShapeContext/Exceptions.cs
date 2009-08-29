using System;

namespace ShapeContext
{
    public class ShapeContextAlgoException : Exception
    {
        public ShapeContextAlgoException(string msg)
            : base(msg)
        {
        }
    }

    public class ShapeContextUtilsException : Exception
    {
        public ShapeContextUtilsException(string msg)
            : base(msg)
        {
        }
    }

    public class TPSException : Exception
    {
        public TPSException(string msg)
            : base(msg)
        {
        }
    }
}