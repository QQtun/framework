using Core.Framework.Utility;
using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Int
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Int To String With Commas As To UI Text Text Handler
    /// </summary>
    public class IntAddCommas : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = IntHandleType.AddCommas.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.Int; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<int> listener = PropertyManager.Instance.Subscribe<int>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.text != null)
                    {
                        info.text.text = StringConverter.ConvertIntToStringWithCommas(value);
                    }
                });

            if (listener != null && info.text != null)
            {
                info.text.text = StringConverter.ConvertIntToStringWithCommas(listener.Value);
            }

            return listener;
        }
    }
}