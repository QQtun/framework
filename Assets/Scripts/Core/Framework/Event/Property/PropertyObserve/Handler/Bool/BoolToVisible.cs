using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Bool
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Bool To Active Handler
    /// </summary>
    internal class BoolToVisible : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = BoolHandleType.Visible.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.Bool;
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
            IPropertyListener<bool> listener = PropertyManager.Instance.Subscribe<bool>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.gameObject != null)
                    {
                        info.gameObject.SetActive(value);
                    }
                });

            if (listener != null && info.gameObject != null)
            {
                info.gameObject.SetActive(listener.Value);
            }

            return listener;
        }
    }
}