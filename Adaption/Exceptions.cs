using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adaption
{
    public class AdaptionException:Exception
    {
        public AdaptionException(string i_msg)
            : base(i_msg)
        { }
    }
}
