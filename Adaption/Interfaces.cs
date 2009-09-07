using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;

using GUIIntegration;

namespace Adaption
{
    public interface ICData : GUIIntegration.ICData
    {
        Image ResultImage { get; }
        Image SourceImage { get; }
        Image TargetImage { get; }
        Size OptimalImageSize { get; }
        Type MyType { get; }
        PropertyInfo[] PropertyList { get; }
    }

    public interface IMatchingAlgo : GUIIntegration.IMatchingAlgo
    {
        void Create(Image i_SourceImage, Image i_TargetImage);
        Type MyType { get; }

        ICData Run();
    }
}
