using Core.Framework.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Framework.Res;

namespace Core.Test.map
{
    /// <summary>
    /// 給美術用地圖測試角色
    /// </summary>
    public class TestMapPlayer : MonoBehaviour
    {
        private GameObject mPlayer;

        private MapDataManager mMapData;

        public int speed = 40;

        private void Awake()
        {
            mPlayer = this.gameObject;
            mMapData = MapDataManager.Instance;
            SetMapCode();
        }

        private void Start()
        {
            
        }

        void Update()
        {
            Move();
        }

        void Move()
        {
            //! 重要:這是將角色附近Poly放進去的函式，如果沒放在角色Update裡角色會穿牆
            {
                var polys = mMapData.currentMapData.polys;
                var enablePolys = mMapData.currentMapData.enablePolys;
                enablePolys.Clear();
                var pt = new Vector2(mPlayer.transform.position.x * 100, mPlayer.transform.position.z * 100);
                foreach (var poly in polys)
                {
                    if (poly.CheckEnable(pt))
                    {
                        enablePolys.Add(poly);
                    }
                }
                // Debug.Log(enablePolys);
            }


            if (Input.GetKey(KeyCode.W))
            {
                float angle = mPlayer.transform.rotation.eulerAngles.y;
                if (angle > 0)
                {
                    angle = angle % 360;
                }
                else
                {
                    angle = (angle + 360) % 360;
                }
                Vector3 nextPos = mMapData.NextPosInMap((mPlayer.transform.position.x),
                (mPlayer.transform.position.z), angle);

                Vector3 targetPos = new Vector3(nextPos.x, mPlayer.transform.position.y, nextPos.z);
                mPlayer.transform.position += (targetPos - mPlayer.transform.position) * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                float angle = mPlayer.transform.rotation.eulerAngles.y;
                angle += 270;
                if (angle > 0)
                {
                    angle = angle % 360;
                }
                else
                {
                    angle = (angle + 360) % 360;
                }
                Vector3 nextPos = mMapData.NextPosInMap((mPlayer.transform.position.x),
                (mPlayer.transform.position.z), angle);

                Vector3 targetPos = new Vector3(nextPos.x, mPlayer.transform.position.y, nextPos.z);
                mPlayer.transform.position += (targetPos - mPlayer.transform.position) * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                float angle = mPlayer.transform.rotation.eulerAngles.y;
                angle += 90;
                if (angle > 0)
                {
                    angle = angle % 360;
                }
                else
                {
                    angle = (angle + 360) % 360;
                }
                Vector3 nextPos = mMapData.NextPosInMap(mPlayer.transform.position.x,
                mPlayer.transform.position.z, angle);

                Vector3 targetPos = new Vector3(nextPos.x, mPlayer.transform.position.y, nextPos.z);
                mPlayer.transform.position += (targetPos - mPlayer.transform.position) * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                float angle = mPlayer.transform.rotation.eulerAngles.y;
                angle += 180;
                if (angle > 0)
                {
                    angle = angle % 360;
                }
                else
                {
                    angle = (angle + 360) % 360;
                }

                Vector3 nextPos = mMapData.NextPosInMap((mPlayer.transform.position.x),
                (mPlayer.transform.position.z), angle);

                Vector3 targetPos = new Vector3(nextPos.x, mPlayer.transform.position.y, nextPos.z);
                mPlayer.transform.position += (targetPos - mPlayer.transform.position) * speed * Time.deltaTime;
            }
        }

        public void SetMapCode()
        {
            Debug.Log("設定當前場景MapCode");
            // MapData.SetGMapData(MapCode);
            mMapData.TestSetGMapData();
        }
    }
}