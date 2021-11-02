using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum EditState
{
    StateEditArea,
    StateFinishArea,
    StateOther
}

[CustomEditor(typeof(UnWalkEditor))]
public class EditorUnWalkable : Editor
{
    override public void OnInspectorGUI()
    {
        if (GUILayout.Button("開啟視窗", GUILayout.Height(50)))
        {
            UnWalkWindow.ShowWindow();
        }
    }
}


/// <summary>
/// 新版Unwalk Window
/// </summary>
public class UnWalkWindow : EditorWindow
{
    private int mPageNum;//當前分頁號碼

    private string[] mPageName = new string[] { "地圖編輯", "物件放置" };//分頁內容

    private string mSceneName; //當前場景名
    private string mTempFilePath;//暫存輸出路徑
    private string mSaveFilePath;//存檔路徑


    #region  地圖編輯器

    public const string ScriptName = "UnWalkEditor"; //物件名
    public const string ParentGameObjectName = "MyEdit/ParentPoint"; //放標記點父物件名

    //存檔檔案名
    public const string UnWalkFileName = "MapConfig.unwalk";

    //輸出檔名
    public const string ServerNavFileName = "Obs.xml";

    public const string SeverSafeRegion = "refuge.xml";

    public const string SeverWater = "water.xml";


    private GameObject mUnWalkGameObject;   //UnWalk腳本所在的物件
    private UnWalkEditor mUnWalkEditor;
    private GameObject mParentPoint = null; //放標記點物件

    // 編輯狀態
    private EditState mEditState = EditState.StateOther;

    private string mDeepParam = "";  //水深：水平面的世界坐標

    private bool mLastShowServerNavGrid = false;//顯示格子
    private bool mLastShowClientNavGrid = false;//顯示客戶端格子


    private Vector2 mScrollAreaPos; //Area列表
    private Vector2 mCrollPointPos; //Point列表
    private bool mToggleShowParent = true; //是否顯示Point

    //是否開啟Scene視窗自動跟隨
    private bool mAutoCameraLookAt = false;
    //地圖長
    private int mMapWidth;
    //地圖寬
    private int mMapHeight;
    //存檔列表
    private Vector2 mSaveData;

    #endregion


    #region  物件放置

    private ExcelDataEditor mExcelData;//資料的Class

    private GameObject mExcelDatePoint = null; //放標記點物件

    public const string ExcelDateGameObjectName = "MyEdit/ExcelDate"; //放標記點父物件名

    public const string MapExcelName = "Map";//正式資料名稱 .xlsm
    public const string MapTempExcelName = "MapTemp";//佔存資料名稱 .xlsx

    private Vector2 mScrollDataType; //類型列表
    private Vector2 mScrollDataObj; //物件列表

    private int mDataObjNum;//當前選擇的物件
    private int mDataObjLastNum;//上一次選擇的物件

    #endregion



    [MenuItem("美術工具/場景/障礙物製作")]
    public static void ShowWindow()
    {
        Debug.Log("ShowWindow");
        GetWindow(typeof(UnWalkWindow));
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        MapToolInit();
        ExcelDataInit();
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        SceneView.duringSceneGui -= OnSceneGUI;
        if (mEditState == EditState.StateEditArea)
        {
            if (mUnWalkEditor.gameObject != null)
            {
                Selection.activeGameObject = mUnWalkEditor.gameObject;

                //Debug.Log("請先關閉編輯狀態(點擊FinishArea按鈕)，才能選擇別的對象");
            }
        }
        //parentPoint.SetActiveRecursively(false);
    }

    private void OnGUI()
    {
        DrawGUI();

        switch (mPageNum)
        {
            case 0: //地圖編輯
                DrawMapToolGUI();
                break;
            case 1: //物件放置
                DraeExcelDataGUI();
                break;
        }
    }


    /// <summary>
    /// GUI共通面板
    /// </summary>
    private void DrawGUI()
    {

        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUI.skin.label.fontSize = 30;
                GUILayout.Label("地圖名稱 : " + mSceneName, GUILayout.Height(40));
                GUI.skin.label.fontSize = 12;

                GUI.backgroundColor = Color.gray;
                if (GUILayout.Button("重新整理", GUILayout.Width(150), GUILayout.Height(40)))
                {
                    ExcelDataInit();
                    MapToolInit();
                }
                GUI.backgroundColor = Color.white;

            }
            EditorGUILayout.EndHorizontal();

            using (new GUILayout.VerticalScope())
            {
                mPageNum = GUILayout.SelectionGrid(
                    mPageNum, mPageName, 2);
            }
        }
        EditorGUILayout.EndVertical();

        //分隔線
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    /// <summary>
    /// 點擊事件
    /// </summary>
    private void OnSceneGUI(SceneView sceneView)
    {
        # region 排除 + 確認edit狀態
        if (Event.current == null)
            return;

        Event e = Event.current;

        if (mUnWalkEditor == null || mUnWalkEditor.dataManager == null)
            return;


        switch (e.type)
        {
            case EventType.KeyDown:
            {
                if (Event.current.keyCode == (KeyCode.A))
                {
                    // Debug.Log("啟動編輯");
                    EditArea();
                }
                break;
            }

            case EventType.KeyUp:
            {
                if (Event.current.keyCode == (KeyCode.A))
                {
                    // Debug.Log("結束編輯");
                    FinishArea();
                }
                break;
            }
        }
        #endregion


        //開始編輯
        if (mEditState == EditState.StateEditArea)
        {
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
                {
                    //位置
                    var placePos = hit.point;

                    //編輯器，輸入事件 mPageNum分頁篩選
                    switch (mPageNum) //TODO: 編輯器，輸入事件
                    {
                        case 0: //地圖編輯功能
                        {
                            if (mUnWalkEditor.dataManager.allAreas.Count <= 0)
                                return;

                            Debug.Log("ADD POINT:" + placePos.x + "," + placePos.y + "," + placePos.z);

                            // generate obj
                            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            point.transform.position = placePos;
                            point.transform.parent = mParentPoint.transform;
                            point.transform.localScale /= 5;
                            mUnWalkEditor.dataManager.AddPoint(mUnWalkEditor.selArea, point);

                            // select the last one
                            mUnWalkEditor.selPoint = mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.selArea].points.Count - 1;
                        }
                        break;
                        case 1://物件放置
                        {
                            //放置的物件內容
                            switch (mExcelData.dataTypeNum)
                            {
                                case 0: //怪物
                                {
                                    mExcelData.AddInputData(mExcelData.mapListKey[mExcelData.dataTypeNum]
                                    , new ExcelDate()
                                    { data = new MonsterDate() { X = (int)(placePos.x * 100), Y = (int)(placePos.z * 100) } }
                                    , mExcelDatePoint, placePos);
                                }
                                break;
                                case 1: //NPC
                                {
                                    mExcelData.AddInputData(mExcelData.mapListKey[mExcelData.dataTypeNum]
                                    , new ExcelDate() { data = new NPCData() { X = (int)(placePos.x * 100), Y = (int)(placePos.z * 100) } }
                                    , mExcelDatePoint, placePos);
                                }
                                break;
                                case 2: //傳送門
                                {
                                    mExcelData.AddInputData(mExcelData.mapListKey[mExcelData.dataTypeNum]
                                    , new ExcelDate()
                                    { data = new TeleportData() { X = (int)(placePos.x * 100), Y = (int)(placePos.z * 100) } }
                                    , mExcelDatePoint, placePos);
                                }
                                break;
                            }

                            Debug.Log("添加物件:" + mExcelData.mapListKey[mExcelData.dataTypeNum] +
                             "  " + placePos.x + "," + placePos.y + "," + placePos.z);
                        }
                        break;
                    }
                }
                e.Use();
            }
        }

    }

    #region 物件放置

    /// <summary>
    /// 物件放置的初始化
    /// </summary>
    private void ExcelDataInit()
    {
        //這邊基本跟 MapTool那邊差不多
        GameObject obj = GameObject.Find(ScriptName);
        if (obj == null)
        {
            obj = new GameObject(ScriptName);
        }

        mUnWalkGameObject = obj;
        mExcelData = obj.GetComponent<ExcelDataEditor>() ?? obj.AddComponent<ExcelDataEditor>();
        Selection.activeGameObject = obj;

        //這邊稍不同，將地圖物件跟放置的物件分開放
        mExcelDatePoint = GameObject.Find(ExcelDateGameObjectName);

        if (mExcelDatePoint == null)
        {
            mExcelDatePoint = new GameObject(ExcelDateGameObjectName);
            mExcelDatePoint.transform.position = mUnWalkGameObject.transform.position;
            mExcelDatePoint.transform.parent = mUnWalkGameObject.transform;
        }

        //額外對存放資料 初始化
        mExcelData.Init();
    }

    /// <summary>
    /// 物件放置GUI繪製
    /// </summary>
    private void DraeExcelDataGUI()
    {

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            {
                //物件類型 例:怪物、NPC.....
                GUILayout.Label("物件類型");
                mScrollDataType = EditorGUILayout.BeginScrollView(mScrollDataType, GUILayout.Width(250), GUILayout.Height(150));
                {
                    mExcelData.dataTypeNum = GUILayout.SelectionGrid(
                                                  mExcelData.dataTypeNum, mExcelData.mapListKey, 1);
                }
                EditorGUILayout.EndScrollView();


                mAutoCameraLookAt = GUILayout.Toggle(mAutoCameraLookAt, "開啟Scene跟隨");
                GUILayout.Label("物件列表");
                mScrollDataObj = EditorGUILayout.BeginScrollView(mScrollDataObj, GUILayout.Width(250), GUILayout.Height(150));
                {
                    mDataObjNum = GUILayout.SelectionGrid(mDataObjNum,
                                                  mExcelData.GetListAllObjName(mExcelData.mapListKey[mExcelData.dataTypeNum]), 1);

                    //點擊後切換目標
                    if (mDataObjNum != mDataObjLastNum)
                    {
                        mDataObjLastNum = mDataObjNum;
                        FocusEditPanel();

                        if (mAutoCameraLookAt == true)
                        {
                            SceneView.lastActiveSceneView.LookAt(
                                mExcelData.mapList[mExcelData.mapListKey[mExcelData.dataTypeNum]][mDataObjNum].dataObj.transform.position);
                        }
                        else
                        {
                            if (mExcelData.mapList[mExcelData.mapListKey[mExcelData.dataTypeNum]].Count > 0)
                            {
                                //找到GameObject ID
                                GameObject go = EditorUtility.InstanceIDToObject(
                                   mExcelData.mapList[mExcelData.mapListKey[mExcelData.dataTypeNum]][mDataObjNum].dataObj.gameObject.GetInstanceID()) as GameObject;
                                //選取到物件
                                Selection.activeGameObject = go;
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    mExcelData.DeleteData(mExcelData.mapListKey[mExcelData.dataTypeNum], mDataObjNum);
                }
                GUI.backgroundColor = Color.white;

                GUILayout.Space(50);

                GUILayout.Label("存讀檔");
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Save", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        mExcelData.SaveExcelData(mTempFilePath + MapTempExcelName + mSceneName
                        , mSaveFilePath + MapExcelName + mSceneName);
                    }

                    if (GUILayout.Button("Load", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        //假如標記點(標記球)沒消失，刪除他的父物件
                        if (mExcelDatePoint.transform.childCount > 0)
                        {
                            GameObject.DestroyImmediate(mExcelDatePoint);
                            mExcelDatePoint = new GameObject(ExcelDateGameObjectName);
                            mExcelDatePoint.transform.position = mUnWalkGameObject.transform.position;
                            mExcelDatePoint.transform.parent = mUnWalkGameObject.transform;
                        }

                        mExcelData.LoadExcelData(mTempFilePath + MapTempExcelName + mSceneName
                        , mSaveFilePath + MapExcelName + mSceneName, mExcelDatePoint);
                    }
                }
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(500));
            {
                GUI.skin.label.fontSize = 20;
                GUILayout.Label("按下鍵盤A鍵，\n啟用Scene畫面編輯，放開則是完成編輯");
                GUI.skin.label.fontSize = 12;
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUILayout.Space(10);


                switch (mExcelData.dataTypeNum)
                {
                    case 0: //怪物
                    {
                        MonsterInspector();
                    }
                    break;
                    case 1: //NPC
                    {
                        NPCInspector();
                    }
                    break;
                    case 2://傳送門
                    {
                        TeleportInspector();
                    }
                    break;
                }
            }
            EditorGUILayout.EndVertical();

        }
        EditorGUILayout.EndHorizontal();
    }

    private void MonsterInspector()
    {
        if (mExcelData.mapList["Monster"].Count == 0)
        {
            return;
        }

        MonsterDate nowData = null;

        try
        {
            nowData = (MonsterDate)mExcelData.mapList["Monster"][mDataObjNum].data;
        }
        catch (ArgumentOutOfRangeException e)
        {
            Debug.LogError("物件不見，請重新點擊物件");
            return;
        }

        GUI.skin.label.fontSize = 20;

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("怪物ID : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.ID = EditorGUILayout.IntField(nowData.ID, GUILayout.Width(150), GUILayout.Height(20));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("怪物描述 : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.MonsterNote = EditorGUILayout.TextArea(nowData.MonsterNote);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("生怪範圍 : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                float radius = (nowData.Radius / 100.0f);
                radius = EditorGUILayout.FloatField(radius, GUILayout.Width(100), GUILayout.Height(20));
                nowData.Radius = (int)(radius * 100.0f);
                GUILayout.Label("unit");
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("怪物生成上限 : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.MonsterInsMax = EditorGUILayout.IntField(nowData.MonsterInsMax, GUILayout.Width(100), GUILayout.Height(20));
                GUILayout.Label("隻");
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("多少秒生成1次 : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.Timeslot = EditorGUILayout.IntField(nowData.Timeslot, GUILayout.Width(100), GUILayout.Height(20));
                GUILayout.Label("毫秒");
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();
    }
    private void NPCInspector()
    {
        if (mExcelData.mapList["NPC"].Count == 0)
        {
            return;
        }

        NPCData nowData = null;

        try
        {
            nowData = (NPCData)mExcelData.mapList["NPC"][mDataObjNum].data;
        }
        catch (ArgumentOutOfRangeException e)
        {
            Debug.LogError("物件不見，請重新點擊物件");
            return;
        }

        GUI.skin.label.fontSize = 20;

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("NPC ID : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.ID = EditorGUILayout.IntField(nowData.ID, GUILayout.Width(150), GUILayout.Height(20));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("NPC描述 : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.NPCNote = EditorGUILayout.TextArea(nowData.NPCNote);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("NPC角度 : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.Direction = (int)Math.Ceiling(mExcelData.mapList["NPC"][mDataObjNum].dataObj.transform.rotation.eulerAngles.y);
                GUILayout.Label(nowData.Direction + "度");
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

    }
    private void TeleportInspector()
    {
        if (mExcelData.mapList["Teleport"].Count == 0)
        {
            return;
        }

        TeleportData nowData = null;

        try
        {
            nowData = (TeleportData)mExcelData.mapList["Teleport"][mDataObjNum].data;
        }
        catch (ArgumentOutOfRangeException e)
        {
            Debug.LogError("物件不見，請重新點擊物件");
            return;
        }

        GUI.skin.label.fontSize = 20;

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("傳送門ID : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.ID = EditorGUILayout.IntField(nowData.ID, GUILayout.Width(150), GUILayout.Height(20));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("傳送門描述 : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.TeleportNote = EditorGUILayout.TextArea(nowData.TeleportNote);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("傳送門角度 : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.Direction = (int)Math.Ceiling(mExcelData.mapList["Teleport"][mDataObjNum].dataObj.transform.rotation.eulerAngles.y);
                GUILayout.Label(nowData.Direction + "度");
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("前往的地圖編號 : ", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            {
                nowData.ToMapCode = EditorGUILayout.IntField(nowData.ToMapCode);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginVertical();
        {
            GUILayout.Label("前往的地圖後角色誕生位置 : ");
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("X : ", GUILayout.Width(50));
                float toX = (nowData.ToX / 100.0f);
                toX = EditorGUILayout.FloatField(toX, GUILayout.Width(100), GUILayout.Height(20));
                nowData.ToX = (int)(toX * 100.0f);

                GUILayout.Space(20);

                GUILayout.Label("Y : ", GUILayout.Width(50));
                float toY = (nowData.ToY / 100.0f);
                toY = EditorGUILayout.FloatField(toY, GUILayout.Width(100), GUILayout.Height(20));
                nowData.ToY = (int)(toY * 100.0f);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

    }

    #endregion

    #region 地圖編輯


    /// <summary>
    /// MapTool初始化
    /// </summary>
    private void MapToolInit()
    {
        Debug.Log("MapTool初始化");
        InitObjects();
        InitSaveFilePath();

        mSceneName = SceneManager.GetActiveScene().name;
        //重新啟動得時候需要檢查一遍所有的點是否存在
        mUnWalkEditor.dataManager.CheckAllPoints();

        //添加Scene視窗事件
        SceneView.duringSceneGui += OnSceneGUI;

        //讀取多存檔列表
        mUnWalkEditor.dataManager.LoadUnwalkAreaList(mTempFilePath, mSceneName, UnWalkFileName);

    }

    /// <summary>
    /// 找尋UnWalk物件
    /// </summary>
    public void InitObjects()
    {
        GameObject obj = GameObject.Find(ScriptName);
        if (obj == null)
        {
            obj = new GameObject(ScriptName);
        }

        //地圖編輯父物件座標
        mUnWalkGameObject = obj;
        mUnWalkEditor = obj.GetComponent<UnWalkEditor>() ?? obj.AddComponent<UnWalkEditor>();
        Selection.activeGameObject = obj;

        mParentPoint = GameObject.Find(ParentGameObjectName);

        if (mParentPoint == null)
        {
            mParentPoint = new GameObject(ParentGameObjectName);
            mParentPoint.transform.position = mUnWalkGameObject.transform.position;
            mParentPoint.transform.parent = mUnWalkGameObject.transform;
        }

    }


    /// <summary>
    /// 初始化存檔路徑
    /// </summary>
    private void InitSaveFilePath()
    {
        string allPath = EditorApplication.currentScene;
        string[] path = allPath.Split(char.Parse("/"));
        string[] fileName = path[path.Length - 1].Split(char.Parse("."));
        allPath = allPath.Replace(path[path.Length - 1], "");
        allPath = fileName[0];

        mSaveFilePath = "Assets/Arts/Map/MapConfig";

        //生成資料夾
        if (!Directory.Exists(mSaveFilePath))
        {
            Directory.CreateDirectory(mSaveFilePath);
        }
        mSaveFilePath += "/";

        mTempFilePath = mSaveFilePath + "Temp";
        if (!Directory.Exists(mTempFilePath))
        {
            Directory.CreateDirectory(mTempFilePath);
        }
        mTempFilePath += "/";

    }

    /// <summary>
    /// 焦點轉移到編輯窗口
    /// </summary>
    private void FocusEditPanel()
    {
        if (SceneView.sceneViews.Count > 0)
        {
            SceneView myView = (SceneView)SceneView.sceneViews[0];
            myView.Focus();
        }
    }


    /// <summary>
    /// 工具面板
    /// </summary>
    private void DrawMapToolGUI()
    {

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical();
            {
                // toggle showable
                GUILayout.Label("Show/Hide");
                mToggleShowParent = GUILayout.Toggle(mToggleShowParent, "Show All Points");
                if (mParentPoint != null)
                {
                    if (mToggleShowParent != mParentPoint.active)
                    {
                        mParentPoint.SetActiveRecursively(mToggleShowParent);
                    }
                }

                // areas列表
                GUILayout.Label("All Areas");
                mScrollAreaPos = EditorGUILayout.BeginScrollView(mScrollAreaPos, GUILayout.Width(250), GUILayout.Height(150));
                {
                    mUnWalkEditor.selArea = GUILayout.SelectionGrid(mUnWalkEditor.selArea, mUnWalkEditor.dataManager.GetAllAreasName(), 1);
                    // active scene view for show unwalkable area
                    if (mUnWalkEditor.selArea != mUnWalkEditor.lastSelArea)
                    {
                        mUnWalkEditor.lastSelArea = mUnWalkEditor.selArea;
                        FocusEditPanel();
                    }
                }
                EditorGUILayout.EndScrollView();

                // points列表
                GUILayout.Label("All Points");
                mCrollPointPos = EditorGUILayout.BeginScrollView(mCrollPointPos, GUILayout.Width(250), GUILayout.Height(150));
                {
                    mUnWalkEditor.selPoint = GUILayout.SelectionGrid(mUnWalkEditor.selPoint,
                     mUnWalkEditor.dataManager.GetAllPointsName(mUnWalkEditor.selArea), 1);

                    if (mUnWalkEditor.selPoint != mUnWalkEditor.lastSelPoint)
                    {
                        mUnWalkEditor.lastSelPoint = mUnWalkEditor.selPoint;
                        FocusEditPanel();

                        if (mAutoCameraLookAt == true)
                        {
                            SceneView.lastActiveSceneView.LookAt(mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.lastSelArea].points[mUnWalkEditor.lastSelPoint].gameObject.transform.position);
                        }
                        else
                        {
                            if (mUnWalkEditor.dataManager.allAreas.Count > 0)
                            {
                                //找到GameObject ID
                                GameObject go = EditorUtility.InstanceIDToObject(mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.lastSelArea].points[mUnWalkEditor.lastSelPoint].gameObject.GetInstanceID()) as GameObject;
                                //選取到物件
                                Selection.activeGameObject = go;
                            }
                        }

                        // select the obj in scene window
                        //Selection.activeGameObject = UnWalkEditor.dataManager.allAreas[UnWalkEditor.selArea].points[UnWalkEditor.selPoint];
                    }
                }
                EditorGUILayout.EndScrollView();

                //存檔列表
                EditorGUILayout.BeginVertical();
                {
                    //存檔
                    GUILayout.Label("存檔列表");
                    mSaveData = EditorGUILayout.BeginScrollView(mSaveData, GUILayout.Width(250), GUILayout.Height(100));
                    {
                        //設定列表
                        mUnWalkEditor.selSaveData = GUILayout.SelectionGrid(
                            mUnWalkEditor.selSaveData,
                             mUnWalkEditor.dataManager.GetAllDataNumber()
                            , 1);

                        //點擊列表按鈕
                        if (mUnWalkEditor.selSaveData != mUnWalkEditor.lastSelSaveData)
                        {
                            mUnWalkEditor.lastSelSaveData = mUnWalkEditor.selSaveData;

                            FocusEditPanel();

                            WindowLoadMapData();
                        }
                    }
                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Save", GUILayout.Width(100), GUILayout.Height(30)))
                        {
                            try
                            {
                                WindowSaveMapData(
                                    mUnWalkEditor.dataManager.GetAllDataNumber()[mUnWalkEditor.lastSelSaveData], false);
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Debug.LogError("存檔列表錯誤");
                                WindowSaveMapData(null, true);
                            }


                        }

                        if (GUILayout.Button("New Save", GUILayout.Width(100), GUILayout.Height(30)))
                        {
                            WindowSaveMapData(null, true);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Label("");
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Delete Save", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        bool isDelete = false;
                        isDelete = EditorUtility.DisplayDialog("警告", "請問要刪除\n一旦刪除無法復原", "刪除", "取消");

                        if (isDelete == true)
                        {
                            WindowDeleteMapData(mUnWalkEditor.dataManager.GetAllDataNumber()[mUnWalkEditor.lastSelSaveData]);
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label("編輯工具");
                // buttons
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("新建障礙物區域", GUILayout.Width(100), GUILayout.Height(50)))
                    {
                        CreateArea(AreanType.Obstruction);
                    }
                    if (GUILayout.Button("新建邊界", GUILayout.Width(100), GUILayout.Height(50)))
                    {
                        CreateArea(AreanType.Boundary);
                    }
                    if (GUILayout.Button("新建安全區域", GUILayout.Width(100), GUILayout.Height(50)))
                    {
                        CreateArea(AreanType.Safe);
                    }

                    if (GUILayout.Button("水域", GUILayout.Width(100), GUILayout.Height(50)))
                    {
                        CreateArea(AreanType.Water);
                    }
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginVertical();
                {
                    GUI.skin.label.fontSize = 20;
                    GUILayout.Label("按下鍵盤A鍵，啟用Scene畫面編輯，放開則是完成編輯");
                    if (GUILayout.Button("CheckArea", GUILayout.Width(100), GUILayout.Height(50)))
                    {
                        FinishArea();
                    }
                    GUI.skin.label.fontSize = 12;
                }
                EditorGUILayout.EndVertical();
                // GUI.enabled = true;



                GUILayout.Label(" ");
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("DeleteArea", GUILayout.Width(100), GUILayout.Height(50)))
                    {
                        DeleteArea();
                    }
                    if (GUILayout.Button("InsertPoint", GUILayout.Width(100), GUILayout.Height(50)))
                    {
                        InsertPoint();
                    }
                    if (GUILayout.Button("DeletePoint", GUILayout.Width(100), GUILayout.Height(50)))
                    {
                        DeletePoint();
                    }
                    if (GUILayout.Button("CheckPoints", GUILayout.Width(100), GUILayout.Height(50)))
                    {
                        mUnWalkEditor.dataManager.CheckAllPoints();
                    }

                }
                EditorGUILayout.EndHorizontal();


                GUILayout.Label(" ");
                GUILayout.Label("Scene 工具");
                mAutoCameraLookAt = GUILayout.Toggle(mAutoCameraLookAt, "開啟Scene跟隨");
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Scene移動到Point", GUILayout.Width(150), GUILayout.Height(50)))
                    {
                        //移動Scene
                        // Debug.Log(UnWalkEditor.dataManager.allAreas[UnWalkEditor.lastSelArea].points[UnWalkEditor.lastSelPoint].gameObject.GetInstanceID());
                        SceneView.lastActiveSceneView.LookAt(mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.lastSelArea].points[mUnWalkEditor.lastSelPoint].gameObject.transform.position);
                    }

                    if (GUILayout.Button("選取Point物件", GUILayout.Width(150), GUILayout.Height(50)))
                    {
                        //找到GameObject ID
                        GameObject go = EditorUtility.InstanceIDToObject(mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.lastSelArea].points[mUnWalkEditor.lastSelPoint].gameObject.GetInstanceID()) as GameObject;
                        //選取到物件
                        Selection.activeGameObject = go;
                    }

                }
                EditorGUILayout.EndHorizontal();

                //服務器導出
                GUILayout.Label("Server Export.");

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("地圖長 : ", GUILayout.Width(60), GUILayout.Height(20));
                    mMapWidth = EditorGUILayout.IntField(mMapWidth, GUILayout.Width(100), GUILayout.Height(20));

                    GUILayout.Label("地圖寬 : ", GUILayout.Width(60), GUILayout.Height(20));
                    mMapHeight = EditorGUILayout.IntField(mMapHeight, GUILayout.Width(100), GUILayout.Height(20));
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                {
                    mUnWalkEditor.showServerNavGrid = GUILayout.Toggle(mUnWalkEditor.showServerNavGrid, "顯示格子", GUILayout.Width(100));
                    if (mLastShowServerNavGrid != mUnWalkEditor.showServerNavGrid)
                    {
                        FocusEditPanel();
                        mLastShowServerNavGrid = mUnWalkEditor.showServerNavGrid;
                    }
                    mUnWalkEditor.showClientNavGrid = GUILayout.Toggle(mUnWalkEditor.showClientNavGrid, "顯示客戶端格子", GUILayout.Width(100));
                    if (mLastShowClientNavGrid != mUnWalkEditor.showClientNavGrid)
                    {
                        FocusEditPanel();
                        mLastShowClientNavGrid = mUnWalkEditor.showClientNavGrid;
                    }

                    mUnWalkEditor.useClientGrid = GUILayout.Toggle(mUnWalkEditor.useClientGrid, "使用大阻擋", GUILayout.Width(100));

                    mUnWalkEditor.createMinimap = GUILayout.Toggle(mUnWalkEditor.createMinimap, "新建小地圖", GUILayout.Width(100));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical();
                {
                    GUILayout.Label("!!注意!! : 開啟顯示格子的話，角色有機會因Lag穿牆");
                    if (GUILayout.Button("計算格子", GUILayout.Width(150), GUILayout.Height(50)))
                    {
                        if (!mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.selArea].CheckLineCross())
                        {
                            EditorUtility.DisplayDialog("錯誤", "當前區域含有不合法的點，\n請保證線段沒有交叉 \n無法計算", "確認");
                            return;
                        }

                        if (mMapWidth == 0 || mMapHeight == 0)
                        {
                            Debug.LogError("請填入地圖長寬");
                            // GetWindow(typeof(ErrorWindow));
                            EditorUtility.DisplayDialog("錯誤", "請填入地圖長寬!", "確認");
                            return;
                        }

                        mUnWalkEditor.dataManager.ComputeNav(mMapWidth, mMapHeight, mUnWalkEditor.createMinimap, mUnWalkEditor.useClientGrid);
                    }
                }
                EditorGUILayout.EndVertical();

                GUILayout.Label("");
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Export .Nav File", GUILayout.Width(150), GUILayout.Height(50)))
                    {
                        if (!mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.selArea].CheckLineCross())
                        {
                            EditorUtility.DisplayDialog("錯誤", "當前區域含有不合法的點，\n請保證線段沒有交叉 \n無法Export .Nav File", "確認");
                        }
                        else
                        {
                            ExportMapConfigFiles();
                        }
                    }
                }
                mUnWalkEditor.navMeshHeight = (int)mParentPoint.transform.position.y;
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 存檔
    /// </summary>
    /// <param name="num">檔案編號</param>
    /// <param name="newSave">是否要建立新存檔(為true時 num無效)</param>
    private void WindowSaveMapData(string num, bool newSave)
    {
        //確認線段交叉，是否要繼續存檔
        bool saveBool = false;

        if (!mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.selArea].CheckLineCross())
        {
            saveBool = EditorUtility.DisplayDialog("錯誤", "當前區域含有不合法的點，\n請保證線段沒有交叉 \n無法計算", "繼續存檔", "取消");
        }
        else
        {
            saveBool = true;
        }

        if (saveBool == false)
        { return; }


        string saveNumber = "";

        //如果未找到存檔資料，預設為0
        if (mUnWalkEditor.dataManager.GetAllDataNumber().Length == 0)
        {
            saveNumber = "0";
        }
        else
        {
            //如果是新增新存檔，就將陣列數加1
            if (newSave == true)
            {
                //撈鎮陣列最後一個號碼，轉數字+1
                saveNumber =
                (Convert.ToInt16(
                    mUnWalkEditor.dataManager.GetAllDataNumber()
                    [mUnWalkEditor.dataManager.GetAllDataNumber().Length - 1]) + 1).ToString();
            }
            else
            {
                //覆蓋存檔就照輸入值
                saveNumber = num;
            }
        }

        //地圖長寬資料
        string terrainParam = mMapWidth + "," + mMapHeight;

        mUnWalkEditor.dataManager.SaveUnwalkArea(
            mTempFilePath + mSceneName + "_" + saveNumber + "_" + UnWalkFileName
         , terrainParam, mUnWalkEditor.useClientGrid);

        //新存檔，重製面板
        if (newSave == true)
        {
            MapToolInit();
        }
    }

    /// <summary>
    /// 刪除存檔
    /// </summary>
    /// <param name="num">檔案編號</param>
    private void WindowDeleteMapData(string num)
    {
        string path = mSaveFilePath + mSceneName + "_" + num + "_" + UnWalkFileName;
        File.Delete(path);
        MapToolInit();
    }

    /// <summary>
    /// 讀取存檔(這是讀檔列表專用，因為他是直接拿存檔列表編號)
    /// </summary>
    private void WindowLoadMapData()
    {
        if (mUnWalkEditor.dataManager.GetAllDataNumber().Length == 0)
        {
            return;
        }

        //讀存檔
        var outParam = "";
        if (mParentPoint != null)
        {
            //假如標記點(標記球)沒消失，刪除他的父物件
            if (mParentPoint.transform.childCount > 0)
            {
                GameObject.DestroyImmediate(mParentPoint);
                mParentPoint = new GameObject(ParentGameObjectName);
                mParentPoint.transform.position = mUnWalkGameObject.transform.position;
                mParentPoint.transform.parent = mUnWalkGameObject.transform;
            }

            //讀檔(用存檔列表抓編號)
            mUnWalkEditor.dataManager.LoadUnwalkArea(
                mTempFilePath + mSceneName + "_" + mUnWalkEditor.dataManager.GetAllDataNumber()[mUnWalkEditor.lastSelSaveData] + "_" + UnWalkFileName, mParentPoint,
             out outParam, out mUnWalkEditor.useClientGrid);
        }
        else
        {
            Debug.LogError("父節點不能為空");
        }
        mUnWalkEditor.selArea = 0;
        mUnWalkEditor.selPoint = 0;

        //讀取地圖長寬
        string[] _params = outParam.Split(',');
        mMapWidth = System.Convert.ToInt32(_params[0]);
        mMapHeight = System.Convert.ToInt32(_params[1]);
        if (mMapWidth == 0 || mMapHeight == 0)
        {
            if (mMapWidth == 0)
            {
                Debug.LogError("地圖長:為0");
            }
            if (mMapHeight == 0)
            {
                Debug.LogError("地圖寬:為0");
            }
            return;
        }
    }


    /// <summary>
    /// 輸出伺服器使用得導航文件
    /// </summary>
    private void ExportNavFile(int width, int height)
    {
        mUnWalkEditor.dataManager.ExportNavForServer(mSaveFilePath + mSceneName + ServerNavFileName, width, height, mUnWalkEditor.createMinimap, mUnWalkEditor.useClientGrid);
    }

    /// <summary>
    /// 輸出水域資料
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private void ExportWater(int width, int height, double deep)
    {
        mUnWalkEditor.dataManager.ExportWaterForServer(mSaveFilePath + mSceneName + SeverWater, width, height, deep);
    }
    /// <summary>
    /// 輸出安全區資料
    /// </summary>
    /// 
    private void ExportAnQuanQuFile(int width, int height)
    {
        mUnWalkEditor.dataManager.ExportSafeRegionForServer(mSaveFilePath + mSceneName + SeverSafeRegion, width, height, mUnWalkEditor.useClientGrid);
    }

    ///
    /// <summary>
    /// 導出 MapConfig 文件
    /// </summary>
    /// 
    private void ExportMapConfigFiles()
    {

        int width = mMapWidth;
        int height = mMapHeight;
        if (width == 0 || height == 0)
        {
            Debug.LogError("導出配置文件時需要設置地圖大小!");
            // GetWindow(typeof(ErrorWindow));
            return;
        }

        //水深
        if (string.IsNullOrEmpty(mDeepParam))
            mDeepParam = "0";
        double deep = System.Convert.ToDouble(mDeepParam);

        //導航障礙物
        ExportNavFile(width, height);
        //安全區
        ExportAnQuanQuFile(width, height);
        //水域
        ExportWater(width, height, deep);
    }

    /// <summary>
    /// 新增Area區域
    /// </summary>
    /// <param name="at">Area類型</param>
    private void CreateArea(AreanType at)
    {
        mUnWalkEditor.dataManager.AddNewArea(at);
        //select the last area
        mUnWalkEditor.selArea = mUnWalkEditor.dataManager.allAreas.Count - 1;

        // auto enter edit mode
        // EditArea();
    }

    /// <summary>
    /// 編輯Area區域
    /// </summary>
    private void EditArea()
    {
        if ((mPageNum == 0)//分頁號 0 =>地圖編輯
        && (mUnWalkEditor.selArea < 0 || mUnWalkEditor.dataManager.allAreas.Count == 0))
        {
            Debug.Log("請先選擇一個區域");
            return;
        }

        mEditState = EditState.StateEditArea;
    }

    /// <summary>
    /// 刪除Area區域
    /// </summary>
    private void DeleteArea()
    {
        mUnWalkEditor.dataManager.DelArea(mUnWalkEditor.selArea);
        mUnWalkEditor.selArea = mUnWalkEditor.dataManager.allAreas.Count - 1;
    }

    /// <summary>
    /// 完成Area區域
    /// </summary>
    private void FinishArea()
    {
        mEditState = EditState.StateFinishArea;
        if ((mPageNum == 0)//分頁號 0 =>地圖編輯
        && (!mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.selArea].CheckLineCross()))
        {
            Debug.LogError("當前區域含有不合法的點，請保證線段沒有交叉");
            // return;
        }
        // else
        // {
        //     mEditState = EditState.StateFinishArea;
        // }
    }

    /// <summary>
    /// 插入新的點
    /// </summary>
    private void InsertPoint()
    {
        if (CheckAllSelect())
        {
            // generate obj
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.selArea].points[mUnWalkEditor.selPoint].transform.position;
            point.transform.position += new Vector3(10, 0, 0);
            point.transform.parent = mParentPoint.transform;
            point.transform.localScale /= 5;

            mUnWalkEditor.dataManager.InsertPoint(mUnWalkEditor.selArea, mUnWalkEditor.selPoint, point);
        }
    }

    /// <summary>
    /// 檢查當前選中索引是否正確
    /// </summary>
    /// <returns></returns>
    private bool CheckAllSelect()
    {
        if (mUnWalkEditor.selArea >= 0 && mUnWalkEditor.selArea < mUnWalkEditor.dataManager.allAreas.Count &&
            mUnWalkEditor.selPoint >= 0 && mUnWalkEditor.selPoint < mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.selArea].points.Count)
            return true;
        return false;
    }

    /// <summary>
    /// 刪除點
    /// </summary>
    private void DeletePoint()
    {
        mUnWalkEditor.dataManager.DelPoint(mUnWalkEditor.selArea, mUnWalkEditor.selPoint);
        mUnWalkEditor.selPoint = mUnWalkEditor.dataManager.allAreas[mUnWalkEditor.selArea].points.Count - 1;
    }

    #endregion


}



/// <summary>
/// Scene存檔時將障礙物排除掉
/// 避免障礙物的GameObject存到Scene裡
/// </summary>
[InitializeOnLoad]
static class EditorSceneManagerSceneSaved
{
    private static bool sFindUnWalk = false;
    private static bool sSceneUnWalk = false;

    static EditorSceneManagerSceneSaved()
    {
        //Sene存檔完觸發
        UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;
        //Sene存檔中觸發
        UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnSceneSaving;
    }

    static void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
    {
        UnityEngine.Debug.LogFormat("Saving scene '{0}' to {1}", scene.name, path);
        Debug.Log("Scene 存檔中");

        //刪除物件
        GameObject obj = GameObject.Find("UnWalkEditor");
        sFindUnWalk = false;
        if (obj != null && obj.GetComponent<UnWalkEditor>() != null)
        {
            GameObject.DestroyImmediate(obj);
            sFindUnWalk = true;
        }

    }

    static void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
    {
        UnityEngine.Debug.LogFormat("Saving scene '{0}'", scene.name);
        Debug.Log("Scene 存檔完");

        //存檔完，把UnWalk重新叫回來
        if (sFindUnWalk == true)
        {
            UnWalkWindow.ShowWindow();
        }
    }
}
