using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Test
{
    public class PrototypeTest : MonoBehaviour
    {
        public Text fpsText;
        public Slider countSlier;
        public Text countText;

        public Camera mainCamera;
        public GameObject[] testModels;
        //public RuntimeAnimatorController animatorContorller;
        public Bounds bounds;
        public int count;

        private List<GameObject> mAllObjects = new List<GameObject>();
        private List<Vector3> mTargetPos = new List<Vector3>();

        private float mTriggerDt;
        private float mMovingDt;
        private float mFpsTimer;
        private float mHudRefreshRate = 1f;

        private int mCurrentModel;

        private void Start()
        {
            Application.targetFrameRate = 120;

            countSlier.minValue = 1;
            countSlier.maxValue = 30;
            countSlier.SetValueWithoutNotify(count);
            countText.text = count.ToString();

            foreach (var obj in testModels)
            {
                obj.SetActive(false);
            }

            TestModel(0);
        }

        public void TestModel(int index)
        {
            mCurrentModel = index;

            if(mAllObjects.Count > 0)
            {
                foreach(var obj in mAllObjects)
                {
                    Destroy(obj);
                }
                mAllObjects.Clear();
            }

            var prefab = testModels[index];
            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(prefab);
                obj.SetActive(true);
                var animator = obj.GetComponentInChildren<Animator>() ?? obj.AddComponent<Animator>();
                //animator.runtimeAnimatorController = animatorContorller;
                animator.SetTrigger($"Action{Random.Range(0, 11) + 1}");

                var r = Random.insideUnitCircle;
                var ranPos = bounds.center + new Vector3(r.x * bounds.extents.x, 0, r.y * bounds.extents.z);
                obj.transform.position = ranPos;
                mAllObjects.Add(obj);

                r = Random.insideUnitCircle;
                ranPos = bounds.center + new Vector3(r.x * bounds.extents.x, 0, r.y * bounds.extents.z);
                mTargetPos.Add(ranPos);
            }
        }

        private void Update()
        {
            mTriggerDt += Time.deltaTime;
            if (mTriggerDt > 2)
            {
                for (int i = 0; i < mAllObjects.Count; i++)
                {
                    var animator = mAllObjects[i].GetComponentInChildren<Animator>();
                    animator.SetTrigger($"Action{Random.Range(0, 11) + 1}");
                }
                mTriggerDt = 0;
            }

            mMovingDt += Time.deltaTime;
            if(mMovingDt > 10)
            {
                for (int i = 0; i < mAllObjects.Count; i++)
                {
                    var r = Random.insideUnitCircle;
                    var ranPos2 = bounds.center + new Vector3(r.x * bounds.extents.x, 0, r.y * bounds.extents.z);
                    mTargetPos[i] = ranPos2;
                }
                mMovingDt = 0;
            }

            for (int i = 0; i < mAllObjects.Count; i++)
            {
                var targetPos = mTargetPos[i];
                var obj = mAllObjects[i];
                if((obj.transform.position - targetPos).sqrMagnitude > 0.01f)
                    obj.transform.position += (targetPos - obj.transform.position).normalized * Time.deltaTime;
            }

            if (Time.unscaledTime > mFpsTimer)
            {
                int fps = (int)(1f / Time.unscaledDeltaTime);
                fpsText.text = "FPS: " + fps;
                mFpsTimer = Time.unscaledTime + mHudRefreshRate;
            }
        }

        public void OnSliderValueChange(float value)
        {
            var newCount = Mathf.RoundToInt(value);
            if(newCount != count)
            {
                count = newCount;
                countText.text = count.ToString();

                if (count > mAllObjects.Count)
                {
                    int diff = count - mAllObjects.Count;
                    var prefab = testModels[mCurrentModel];
                    for (int i = 0; i < diff; i++)
                    {
                        var obj = Instantiate(prefab);
                        obj.SetActive(true);
                        var animator = mAllObjects[i].GetComponentInChildren<Animator>();
                        //animator.runtimeAnimatorController = animatorContorller;
                        animator.SetTrigger($"Action{Random.Range(0, 11) + 1}");

                        var r = Random.insideUnitCircle;
                        var ranPos = bounds.center + new Vector3(r.x * bounds.extents.x, 0, r.y * bounds.extents.z);
                        obj.transform.position = ranPos;
                        mAllObjects.Add(obj);

                        r = Random.insideUnitCircle;
                        ranPos = bounds.center + new Vector3(r.x * bounds.extents.x, 0, r.y * bounds.extents.z);
                        mTargetPos.Add(ranPos);
                    }
                }
                else if (count < mAllObjects.Count)
                {
                    int diff = mAllObjects.Count - count;
                    for (int i = 0; i < diff; i++)
                    {
                        var obj = mAllObjects[mAllObjects.Count - 1];
                        mAllObjects.RemoveAt(mAllObjects.Count - 1);
                        Destroy(obj);
                    }
                }
            }
        }
    }
}