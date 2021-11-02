using System;
using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Float
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Float To Be Ceil UI Text Handler
    /// </summary>
    public class FloatCeil : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = FloatHandleType.Ceiling.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.Float;
            }
        }

        public string HandleType
        {
            get
            {
                return HandleTypeName;
            }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<float> listener = PropertyManager.Instance.Subscribe<float>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.text != null)
                    {
                        info.text.text = Math.Ceiling(value).ToString();
                    }
                });

            if (listener != null && info.text != null)
            {
                info.text.text = Math.Ceiling(listener.Value).ToString();
            }

            return listener;
        }
    }
}