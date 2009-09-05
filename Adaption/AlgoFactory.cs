using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GUIIntegration;

namespace Adaption
{
    public class AlgoFactory : IAlgoFactory
    {
        public static readonly string ShapeContext = "Shape context";
        public static readonly string ModifiedShapeContext = "Modified Shape context";
        public static readonly string PCA = "Princilal Component Analysis";
        public static readonly string Hausdorff = "Hausdorrf alignment";

        private  Dictionary<string, IMatchingAlgo> m_AlgoRepo;

        public AlgoFactory()
        {
            m_AlgoRepo = new Dictionary<string, IMatchingAlgo>();
            registerAlgorithms();
        }

        private void registerAlgorithms()
        {
            Register(ShapeContext, new CShapeContext());
            Register(PCA, new CPCA());
            Register(Hausdorff, new CHausdorffDistance());
            Register(ModifiedShapeContext, new CModifiedShapeContext());
            //s_AlgoRepo[Hausdorrf] = new ...
        }
        public void Register(string i_AlgoName, IMatchingAlgo i_AlgoAdoption)
        {
            m_AlgoRepo[i_AlgoName] = i_AlgoAdoption;
        }

        public GUIIntegration.IMatchingAlgo GetAlgorithm(string i_AlgoName)
        {
            return m_AlgoRepo[i_AlgoName];
        }

        public IMatchingAlgo GetAlgo(string i_AlgoName)
        {
            return m_AlgoRepo[i_AlgoName];
        }
    }
}
