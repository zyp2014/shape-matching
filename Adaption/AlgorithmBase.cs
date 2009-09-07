using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using GUIIntegration;

namespace Adaption
{
    public abstract class AlgorithmBase : IMatchingAlgo
    {
        public abstract void Create(Image i_SourceImage, Image i_TargetImage);

        public abstract Type MyType
        { get; }

        public PropertyInfo[] PropertyList
        {
            get
            {
                Type propertyType = typeof(PropertyInfo[]);
                Func<PropertyInfo, bool> isNotPropertyInfoTypePredicate = (property) =>
                    {
                        if (property.GetType() == propertyType)
                        {
                            return false;
                        }

                        return true;
                    };
                return
                MyType.GetProperties().Where<PropertyInfo>(isNotPropertyInfoTypePredicate).ToArray<PropertyInfo>();
            }
        }

        public virtual void SetProperty(CAlgoProp i_PropertyData)
        {
            PropertyInfo[] propArr = PropertyList;
            bool propertyFound = false;

            foreach (PropertyInfo realProp in propArr)
            {
                if (realProp.Name == i_PropertyData.Name)
                {
                    realProp.SetValue(this, i_PropertyData.Value, null);
                    propertyFound = true;
                    break;
                }
            }

            if (! propertyFound)
            {
                throw new AdaptionException("A property " + i_PropertyData.Name + " does not exist, please recheck your property list");
            }
        }

        private void setProperties(CAlgoProp[] i_Properties)
        {
            foreach (CAlgoProp property in i_Properties)
            {
                SetProperty(property);
            }
        }

        public abstract ICData Run();

        #region GUIIntegration.IMatchingAlgo Members

        public abstract object Instance { get; }

        public GUIIntegration.ICData Run(Image i_OriginalImg, Image i_TransferImg, GUIIntegration.CAlgoProp[] i_Properties)
        {
            Create(i_OriginalImg, i_TransferImg);

            if (i_Properties != null)
            {
                setProperties(i_Properties);
            }
            
            return Run();
        }

        #endregion
    }
}
