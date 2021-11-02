using Core.Game.Logic;
using UnityEngine;
using Sprite = Core.Game.Logic.Sprite;

namespace Core.Game.Utility
{
    public class SceneObjectPool : MonoBehaviour
    {
        private Transform mRoot;

        private void Awake()
        {
            var root = new GameObject("PoolRoot");
            mRoot = root.transform;
            mRoot.SetParent(transform);
            root.SetActive(false);
        }

        public void Recycle(ISceneObject obj)
        {
            // TODO
            if(obj.Type == SceneObjectType.Sprite)
            {
                var sprite = obj as Sprite;
                Destroy(sprite.gameObject);
            }
        }
    }
}