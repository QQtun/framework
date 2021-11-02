using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Utility
{
    public static class TransformEx
    {
        private static Queue<Transform> sFindRoot = new Queue<Transform>();
        public static Transform FindInChildren(this Transform self, string name)
        {
            sFindRoot.Clear();
            for (int i = 0; i < self.childCount; i++)
            {
                var curTransform = self.GetChild(i);
                if (curTransform.name == name)
                    return curTransform;
                if(curTransform.childCount > 0)
                    sFindRoot.Enqueue(curTransform);
            }
            int count = 0;
            while (sFindRoot.Count != 0)
            {
                var curTransform = sFindRoot.Dequeue();
                for (int i = 0; i < curTransform.childCount; i++)
                {
                    var t = curTransform.GetChild(i);
                    if (t.name == name)
                        return t;
                    if (t.childCount > 0)
                        sFindRoot.Enqueue(t);
                }
                count++;
                if (count > 1000)
                    break;
            }
            return null;
        }
    }
}