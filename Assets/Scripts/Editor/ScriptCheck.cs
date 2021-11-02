using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ScriptCheck
{
    /// <summary>
    /// 删除对象中丢失或者有错误的脚本
    /// </summary>
    [MenuItem("检查工具/Cleanup Missing Scripts")]
    public static void CleanupMissingScripts()
    {
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {        
            var gameObject = Selection.gameObjects[i];
            List<GameObject> gosWithMissingScript = GetGameObjectHaveMissingScript(gameObject);
            for (int j = 0; j < gosWithMissingScript.Count; j++)
            {
                GameObject missingScriptGo = gosWithMissingScript[j];
                var components = missingScriptGo.GetComponentsInChildren<Component>(true);
                var serializedObject = new SerializedObject(missingScriptGo);
                var prop = serializedObject.FindProperty("m_Component");
                int r = 0;
                for (int k = 0; k < components.Length; k++)
                {
                    if (components[k] == null)
                    {
                        prop.DeleteArrayElementAtIndex(k - r);
                        r++;
                    }
                }
                serializedObject.ApplyModifiedProperties();

                string s = missingScriptGo.name;
                Transform t = missingScriptGo.transform;
                while (t.parent != null)
                {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }
                Debug.Log("remvoe missing script:" + s, missingScriptGo);
            }
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("提示", "修改完成！", "关闭");
    }

    [MenuItem("检查工具/Find Script Missing")]
    public static void FindInSelected()
    {
        GameObject[] go = Selection.gameObjects;
        foreach (GameObject g in go)
        {
            FindInGO(g);
        }
        EditorUtility.DisplayDialog("提示", "查找完成！", "关闭");
    }

    private static void FindInGO(GameObject g)
    {
        Component[] components = g.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                string s = g.name;
                Transform t = g.transform;
                while (t.parent != null)
                {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }
                Debug.Log(s + " has an empty script attached in position: " + i, g);
            }
        }
        foreach (Transform childT in g.transform)
        {
            FindInGO(childT.gameObject);
        }
    }

    static List<GameObject> GetGameObjectHaveMissingScript(GameObject go)
    {
        Transform[] ts = go.GetComponentsInChildren<Transform>(true);
        List<GameObject> objs = new List<GameObject>();
        foreach (Transform t in ts)
        {
            Component[] cs = t.gameObject.GetComponents<Component>();
            foreach (Component c in cs)
            {
                if (c == null)
                {
                    objs.Add(t.gameObject);
                }
            }
        }
        return objs;
    }
}