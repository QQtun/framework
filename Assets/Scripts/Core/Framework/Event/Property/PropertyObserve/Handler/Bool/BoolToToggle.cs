using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Bool
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Bool To Toggle Handler
    /// </summary>
    public class BoolToToggle : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = BoolHandleType.ToToggle.ToString();

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
                    if (info.toggle != null)
                    {
                        info.toggle.isOn = value;
                    }
                });

            if (listener != null && info.toggle != null)
            {
                info.toggle.isOn = listener.Value;
            }

            return listener;
        }
    }
}