using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Framework.Event.Property.PropertyObserve
{
    public class PropertysObserver : MonoBehaviour
    {
        [FormerlySerializedAs("_bindingInfos")]
        [HideInInspector]
        public List<PropertyBindInfo> bindingInfos = new List<PropertyBindInfo>();

        private List<IPropertyListener> mListeners = new List<IPropertyListener>();

        private void Awake()
        {
            List<PropertyBindInfo>.Enumerator iter = bindingInfos.GetEnumerator();
            while (iter.MoveNext())
            {
                if (iter.Current == null)
                {
                    continue;
                }

                PropertyBindInfo info = iter.Current;
                IPropertyListener listener = PropertyBindingHandler.HandleBindingInfo(gameObject, info);

                if (listener != null)
                {
                    mListeners.Add(listener);
                }
            }
            iter.Dispose();
        }

        private void OnDestroy()
        {
            List<IPropertyListener>.Enumerator iter = mListeners.GetEnumerator();
            while (iter.MoveNext())
            {
                if (iter.Current != null)
                {
                    iter.Current.Unsubscribe();
                }
            }
            iter.Dispose();
        }
    }
}