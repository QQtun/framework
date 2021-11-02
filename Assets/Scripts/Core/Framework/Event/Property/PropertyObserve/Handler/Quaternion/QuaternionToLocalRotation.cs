using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Quaternion
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Set Quaternion To Transform LocalRotation Handler
    /// </summary>
    public class QuaternionToLocalRotation : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = QuaternionHandleType.ToLocalRotation.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.Quaternion; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<UnityEngine.Quaternion> listener =
                PropertyManager.Instance.Subscribe<UnityEngine.Quaternion>(info.propertyKey,
                    (lastValue, value) =>
                    {
                        if (info.transform != null)
                        {
                            info.transform.localRotation = value;
                        }
                    });

            if (listener != null && info.transform != null)
            {
                info.transform.localRotation = listener.Value;
            }

            return listener;
        }
    }
}