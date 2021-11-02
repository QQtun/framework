using Core.Framework.Map;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace Core.Framework.Nav
{
    public class TestNav : MonoBehaviour
    {
        //Nav算法參數
        public int nSearchLimit;

        //顯示線的LineRender
        public LineRenderer line;

        //測試終點
        public Transform gameEnd;

        //測試起點
        public Transform gameStart;

        //測試Nav線時，是否啟用Update模式
        public bool isLineUpdate = false;

        //檢測精度
        public int detectNum = 1;

        //測全地圖時是否開啟球放置
        public bool isBall = true;

        //測腳色圓心半徑
        public int detectRadius = 1;

        //放置物件球的父物件
        public GameObject ballParent;

        private void Start()
        {
            ballParent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        private void Update()
        {
            if (isLineUpdate)
            {
                TestNavLine();
            }
        }

        /// <summary>
        /// 檢測全地圖Nav的點
        /// </summary>
        public void TestMapNav()
        {
            List<Vector3Int> nodeList = new List<Vector3Int>(); ;
            int width = MapDataManager.Instance.currentMapData.mapWidth / 100;
            int height = MapDataManager.Instance.currentMapData.mapHeight / 100;
            int posX = (int)this.transform.position.x;
            int posY = (int)this.transform.position.z;
            DestroyImmediate(ballParent);
            ballParent = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            Stopwatch sw = new Stopwatch();
            sw.Start();


            //以腳色為圓心計算左下
            for (int x_max = 0; x_max >= -(width - (width - posX)); x_max -= detectNum)
            {
                for (int y_max = 0; y_max >= -(height - (height - posY)); y_max -= detectNum)
                {
                    nodeList = NavAStar.FindPath(new Vector3Int(posX, 0, posY), new Vector3Int(posX + x_max, 0, posY + y_max), nSearchLimit);

                    if (nodeList != null)
                    {
                        if (isBall == true)
                        {
                            GameObject newG = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            newG.transform.position = new Vector3(posX + x_max, 0, posY + y_max);
                            newG.transform.localScale *= detectNum;
                            newG.transform.parent = ballParent.transform;
                            newG.GetComponent<MeshRenderer>().material.color = Color.red;
                        }
                    }

                }
            }

            nodeList = new List<Vector3Int>(); ;

            //以腳色為圓心計算左上
            for (int x_max = 0; x_max >= -(width - (width - posX)); x_max -= detectNum)
            {
                for (int y_max = 0; y_max <= +(height - posY); y_max += detectNum)
                {
                    nodeList = NavAStar.FindPath(new Vector3Int(posX, 0, posY), new Vector3Int(posX + x_max, 0, posY + y_max), nSearchLimit);

                    if (nodeList != null)
                    {
                        if (isBall == true)
                        {
                            GameObject newG = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            newG.transform.position = new Vector3(posX + x_max, 0, posY + y_max);
                            newG.transform.localScale *= detectNum;
                            newG.transform.parent = ballParent.transform;
                            newG.GetComponent<MeshRenderer>().material.color = Color.blue;
                        }
                    }
                }
            }

            nodeList = new List<Vector3Int>(); ;

            //以腳色為圓心計算右下
            for (int x_max = 0; x_max <= +(width - posX); x_max += detectNum)
            {
                for (int y_max = 0; y_max >= -(height - (height - posY)); y_max -= detectNum)
                {
                    nodeList = NavAStar.FindPath(new Vector3Int(posX, 0, posY), new Vector3Int(posX + x_max, 0, posY + y_max), nSearchLimit);

                    if (nodeList != null)
                    {
                        if (isBall == true)
                        {
                            GameObject newG = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            newG.transform.position = new Vector3(posX + x_max, 0, posY + y_max);
                            newG.transform.localScale *= detectNum;
                            newG.transform.parent = ballParent.transform;
                            newG.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        }
                    }
                }
            }

            nodeList = new List<Vector3Int>(); ;

            //以腳色為圓心計算右上
            for (int x_max = 0; x_max <= +(width - posX); x_max += detectNum)
            {
                for (int y_max = 0; y_max <= +(height - posY); y_max += detectNum)
                {
                    nodeList = NavAStar.FindPath(new Vector3Int(posX, 0, posY), new Vector3Int(posX + x_max, 0, posY + y_max), nSearchLimit);

                    if (nodeList != null)
                    {
                        if (isBall == true)
                        {
                            GameObject newG = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            newG.transform.position = new Vector3(posX + x_max, 0, posY + y_max);
                            newG.transform.localScale *= detectNum;
                            newG.transform.parent = ballParent.transform;
                            newG.GetComponent<MeshRenderer>().material.color = Color.green;
                        }
                    }
                }
            }

            sw.Stop();
            UnityEngine.Debug.Log(string.Format("全體走完時間: {0} ms", sw.ElapsedMilliseconds));
        }

        /// <summary>
        /// 檢測角色半徑內Nav的點
        /// </summary>
        public void TestPlayerNav()
        {

            List<Vector3Int> nodeList = new List<Vector3Int>(); ;
            int posX = (int)this.transform.position.x;
            int posY = (int)this.transform.position.z;
            DestroyImmediate(ballParent);
            ballParent = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            Stopwatch sw = new Stopwatch();
            sw.Start();


            //以腳色為圓心計算左下
            for (int x_max = 0; x_max >= -detectRadius; x_max -= detectNum)
            {
                for (int y_max = 0; y_max >= -detectRadius; y_max -= detectNum)
                {
                    nodeList = NavAStar.FindPath(new Vector3Int(posX, 0, posY), new Vector3Int(posX + x_max, 0, posY + y_max), nSearchLimit);

                    if (nodeList != null)
                    {
                        if (isBall == true)
                        {
                            GameObject newG = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            newG.transform.position = new Vector3(posX + x_max, 0, posY + y_max);
                            newG.transform.localScale *= detectNum;
                            newG.transform.parent = ballParent.transform;
                            newG.GetComponent<MeshRenderer>().material.color = Color.red;
                        }
                    }

                }
            }

            nodeList = new List<Vector3Int>(); ;

            //以腳色為圓心計算左上
            for (int x_max = 0; x_max >= -detectRadius; x_max -= detectNum)
            {
                for (int y_max = 0; y_max <= +detectRadius; y_max += detectNum)
                {
                    nodeList = NavAStar.FindPath(new Vector3Int(posX, 0, posY), new Vector3Int(posX + x_max, 0, posY + y_max), nSearchLimit);

                    if (nodeList != null)
                    {
                        if (isBall == true)
                        {
                            GameObject newG = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            newG.transform.position = new Vector3(posX + x_max, 0, posY + y_max);
                            newG.transform.localScale *= detectNum;
                            newG.transform.parent = ballParent.transform;
                            newG.GetComponent<MeshRenderer>().material.color = Color.blue;
                        }
                    }
                }
            }

            nodeList = new List<Vector3Int>(); ;

            //以腳色為圓心計算右下
            for (int x_max = 0; x_max <= +detectRadius; x_max += detectNum)
            {
                for (int y_max = 0; y_max >= -detectRadius; y_max -= detectNum)
                {
                    nodeList = NavAStar.FindPath(new Vector3Int(posX, 0, posY), new Vector3Int(posX + x_max, 0, posY + y_max), nSearchLimit);

                    if (nodeList != null)
                    {
                        if (isBall == true)
                        {
                            GameObject newG = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            newG.transform.position = new Vector3(posX + x_max, 0, posY + y_max);
                            newG.transform.localScale *= detectNum;
                            newG.transform.parent = ballParent.transform;
                            newG.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        }
                    }
                }
            }

            nodeList = new List<Vector3Int>(); ;

            //以腳色為圓心計算右上
            for (int x_max = 0; x_max <= +detectRadius; x_max += detectNum)
            {
                for (int y_max = 0; y_max <= +detectRadius; y_max += detectNum)
                {
                    nodeList = NavAStar.FindPath(new Vector3Int(posX, 0, posY), new Vector3Int(posX + x_max, 0, posY + y_max), nSearchLimit);

                    if (nodeList != null)
                    {
                        if (isBall == true)
                        {
                            GameObject newG = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            newG.transform.position = new Vector3(posX + x_max, 0, posY + y_max);
                            newG.transform.localScale *= detectNum;
                            newG.transform.parent = ballParent.transform;
                            newG.GetComponent<MeshRenderer>().material.color = Color.green;
                        }
                    }
                }
            }

            sw.Stop();
            UnityEngine.Debug.Log(string.Format("全體走完時間: {0} ms", sw.ElapsedMilliseconds));


        }

        /// <summary>
        /// 測單一條線，時間距離
        /// </summary>
        public void TestNavLine()
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();

            line.positionCount = 0;

            List<Vector2Int> path;

            path = NavAStar.FindPath(
                new Vector2Int((int)gameStart.position.x, (int)gameStart.position.z),
                new Vector2Int((int)gameEnd.position.x, (int)gameEnd.position.z), nSearchLimit);

            foreach (Vector2Int a in path)
            {
                line.SetPosition(line.positionCount++, new Vector3((a.x), 1, (a.y)));

            }

            sw.Stop();
            UnityEngine.Debug.Log(string.Format("單一條線時間: {0} ms", sw.ElapsedMilliseconds));
            UnityEngine.Debug.Log(Vector2Int.Distance(new Vector2Int((int)gameStart.position.x, (int)gameStart.position.z),
              new Vector2Int((int)gameEnd.position.x, (int)gameEnd.position.z)));
        }

    }
}