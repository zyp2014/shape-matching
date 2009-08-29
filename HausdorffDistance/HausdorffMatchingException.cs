using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HausdorffDistance
{
    class HausdorffMatchingException:Exception
    {
        public HausdorffMatchingException(string i_message)
            : base(i_message)
        { }
    }
}
