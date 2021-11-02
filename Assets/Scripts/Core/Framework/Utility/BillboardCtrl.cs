using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Utility
{
    public class BillboardCtrl : MonoBehaviour
    {
        public Transform cameraTransform;
        public bool freezeX = true;
        public bool freezeY;
        public bool freezeZ = true;

        private Transform mTransform;

        // Update is called once per frame
        private void Update()
        {
            if(UnityEngine.Camera.main != null && 
                cameraTransform != UnityEngine.Camera.main.transform)
            {
                cameraTransform = UnityEngine.Camera.main.transform;
            }
            if (cameraTransform == null)
                return;
            if (mTransform == null)
                mTransform = transform;

            var dir = mTransform.position - cameraTransform.position;
            var q = Quaternion.LookRotation(dir, Vector3.up);
            var angles = q.eulerAngles;
            if (freezeX)
                angles.x = 0;
            if (freezeY)
                angles.y = 0;
            if (freezeZ)
                angles.z = 0;
            q = Quaternion.Euler(angles);
            mTransform.rotation = q;
        }
    }
}