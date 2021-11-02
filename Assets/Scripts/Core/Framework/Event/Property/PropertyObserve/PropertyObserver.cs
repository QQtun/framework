using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Framework.Event.Property.PropertyObserve
{
    public class PropertyObserver : MonoBehaviour
    {
        [FormerlySerializedAs("_bindingInfo")]
        [HideInInspector]
        public PropertyBindInfo bindingInfo = new PropertyBindInfo();

        private IPropertyListener mListener;

        private void Awake()
        {
            mListener = PropertyBindingHandler.HandleBindingInfo(gameObject, bindingInfo);
        }

        private void OnDestroy()
        {
            if (mListener != null)
            {
                mListener.Unsubscribe();
            }
        }
    }
}