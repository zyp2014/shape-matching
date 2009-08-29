using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCA
{
    public class PCAException:Exception
    {
        public PCAException(string i_message):base(i_message) { }
    }
}
