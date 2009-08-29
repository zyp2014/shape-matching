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
                return MyType.GetProperties();
            }
        }

        public virtual CProperty[] PropertyStrings
        {
            get
            {
                List<CProperty> propList = new List<CProperty>();
                PropertyInfo[] propArr = PropertyList;

                foreach (PropertyInfo property in propArr)
                {
                    if ((property.PropertyType == typeof(int)) ||
                        (property.PropertyType == typeof(double)) ||
                        (property.PropertyType == typeof(float)) ||
                        (property.PropertyType == typeof(string)))
                    {
                        CProperty currProp = new CProperty();
                        currProp.Name = property.Name;
                        currProp.Value = property.GetValue(this, null).ToString();
                        propList.Add(currProp);
                    }
                }

                return propList.ToArray<CProperty>();
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                PropertyInfo[] propArr = PropertyList;

                foreach (CProperty stringProp in value)
                {
                    bool propertyFound = false;
                    ///Locating a real property equals to stringProp
                    foreach (PropertyInfo realProp in propArr)
                    {
                        if (realProp.Name == stringProp.Name)
                        {
                            propertyFound = true;

                            if (realProp.PropertyType == typeof(int))
                            {
                                realProp.SetValue(this, int.Parse(stringProp.Value), null);
                            }
                            else if (realProp.PropertyType == typeof(double))
                            {
                                realProp.SetValue(this, double.Parse(stringProp.Value), null);
                            }
                            else if (realProp.PropertyType == typeof(float))
                            {
                                realProp.SetValue(this, float.Parse(stringProp.Value), null);
                            }
                            else if (realProp.PropertyType == typeof(string))
                            {
                                realProp.SetValue(this, stringProp.Value, null);
                            }
                            else
                            {
                                throw new AdaptionException("A property " + stringProp.Name + " is a non recognizable type.(use only int,double,float or string.)");
                            }

                            break;
                        }
                    }

                    if (!propertyFound)
                    {
                        throw new AdaptionException("A property " + stringProp.Name + " does not exist, please recheck your property list");
                    }
                }
            }
        }

        public virtual CAlgoProp[] Properties
        {
            get
            {
                List<CAlgoProp> propList = new List<CAlgoProp>();
                PropertyInfo[] propArr = PropertyList;
                foreach (PropertyInfo property in propArr)
                {
                    propList.Add(new CAlgoProp(property.Name,property.Name,property.GetValue(this,null),property.GetType()));
                }
                return propList.ToArray<CAlgoProp>();
            }
            set
            {
                foreach (CAlgoProp property in value)
                {
                    SetProperty(property);
                }
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

        public abstract ICData Run();

        public virtual ICData Run(Image i_Image1, Image i_Image2, CProperty[] i_AlgorithmProps)
        {
            Create(i_Image1, i_Image2);
            PropertyStrings = i_AlgorithmProps;
            return Run();
        }

        #region GUIIntegration.IMatchingAlgo Members

        public abstract object Instance { get; }

        public GUIIntegration.ICData Run(Image i_OriginalImg, Image i_TransferImg, GUIIntegration.CAlgoProp[] i_Properties)
        {
            Create(i_OriginalImg, i_TransferImg);
            Properties = i_Properties;
            return Run();
        }

        #endregion
    }
}
