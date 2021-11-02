using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreatePrefabAndMaterialFromFbxWindow : EditorWindow
{
    private string mOutPath;
    private FbxList mFbxListAsset;
    private SerializedObject mSerializedObj;
    private SerializedProperty mFbxList;

    [MenuItem("美術工具/FBX轉Prefab")]
    public static void CreatePrefabAndMaterial()
    {
        var window = GetWindow(typeof(CreatePrefabAndMaterialFromFbxWindow));
    }

    private void OnEnable()
    {
        mFbxListAsset = CreateInstance<FbxList>();
        mSerializedObj = new SerializedObject(mFbxListAsset);
        mFbxList = mSerializedObj.FindProperty("fbxs");
    }

    private void OnGUI()
    {
        DropAreaGUI();
        EditorGUILayout.PropertyField(mFbxList);
        GUILayout.BeginHorizontal("box");
        {
            mOutPath = GUILayout.TextField(mOutPath);
            if (GUILayout.Button("選取輸出路徑", GUILayout.Width(80), GUILayout.Height(20)))
            {
                mOutPath = EditorUtility.OpenFolderPanel("輸出位置", "Assets", "");
                mOutPath = mOutPath.Substring(Application.dataPath.Length - "Assets".Length);
            }
        }
        GUILayout.EndHorizontal();
        if(GUILayout.Button("輸出"))
        {
            foreach (var fbx in mFbxListAsset.fbxs)
            {
                CreatePrefab(fbx, Shader.Find("J_Shard"));
                //CreatePrefab(fbx, Shader.Find("Kayac/Kamakura"));
                //CreatePrefab(fbx, Shader.Find("UnityChan/ToonShader"));
                //CreatePrefab(fbx, Shader.Find("UnityChanToonShader/Mobile/Toon_DoubleShadeWithFeather"));
            }
        }
    }

    private void CreatePrefab(GameObject fbx, Shader shader)
    {
        var instance = Instantiate(fbx);

        // 替換所有Material
        var renderers = instance.GetComponentsInChildren<Renderer>();
        foreach (var skm in renderers)
        {
            ReplaceMaterial(fbx, skm, shader);
        }

        //產生Prefab
        string postFix = shader.name.Replace("/", "_");
        PrefabUtility.SaveAsPrefabAsset(instance, mOutPath + $"/{fbx.name} {postFix}.prefab");
        DestroyImmediate(instance);
    }

    private void ReplaceMaterial(GameObject fbx, Renderer renderer, Shader shader)
    {
        var shaderName = shader.name.Replace("/", "_");
        var oldMaterials = renderer.sharedMaterials;
        List<Material> newMaterials = new List<Material>();
        for (int i = 0; i < oldMaterials.Length; i++)
        {
            var m = oldMaterials[i];
            if (m == null)
                continue;
            var newMaterial = new Material(m);
            newMaterial.shader = shader;
            var folder = mOutPath + $"/{fbx.name}Materials/{shaderName}";
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.CreateAsset(newMaterial, folder + $"/{m.name} {shaderName}.mat");
            newMaterials.Add(newMaterial);
        }
        AssetDatabase.Refresh();
        var arr = newMaterials.ToArray();
        renderer.sharedMaterials = arr;
    }

    public void DropAreaGUI()
    {
        Event evt = Event.current;
        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true));
        GUI.Box(drop_area, "fbx拖入此處");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObj in DragAndDrop.objectReferences)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(draggedObj);
                        if (assetPath.EndsWith(".fbx") || assetPath.EndsWith(".FBX"))
                        {
                            mFbxList.arraySize++;
                            var s = mFbxList.GetArrayElementAtIndex(mFbxList.arraySize - 1);
                            s.objectReferenceValue = draggedObj as GameObject;
                        }
                    }
                    mSerializedObj.ApplyModifiedProperties();
                }
                break;
        }
    }
}

public class FbxList : ScriptableObject
{
    public List<GameObject> fbxs = new List<GameObject>();
}