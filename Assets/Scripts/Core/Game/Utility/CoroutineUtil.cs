using System.Collections;
using UnityEngine;

namespace Core.Game.Utility
{
    public class CoroutineUtil : Singleton<CoroutineUtil>
    {
        public static new Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return ((MonoBehaviour)Instance).StartCoroutine(enumerator);
        }

        public static new void StopCoroutine(Coroutine co)
        {
            ((MonoBehaviour)Instance).StopCoroutine(co);
        }
    }
}