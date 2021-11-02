using Core.Framework.Nav;
using Core.Framework.Nav.AStarEx;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestNav))]
public class NavEditor : Editor
{
    private TestNav mTestNav;

    public override void OnInspectorGUI()
    {
        mTestNav = (TestNav)target;

        GUILayout.Label("Nav測試需在Unity Play下才能進行");
        GUILayout.Label("Nav測試工具是需要TestMapPlayer在場景上才能使用");

        mTestNav.nSearchLimit = EditorGUILayout.IntField("SearchLimit", mTestNav.nSearchLimit);

        mTestNav.detectNum = EditorGUILayout.IntField("檢測精度", mTestNav.detectNum);

        EditorGUILayout.Space();

        mTestNav.isBall = EditorGUILayout.Toggle("是否放置球", mTestNav.isBall, GUILayout.Height(30));

        if (GUILayout.Button("測地圖全部Nav點", GUILayout.Height(50)))
        {
            mTestNav.TestMapNav();
        }

        EditorGUILayout.Space();

        mTestNav.detectRadius = EditorGUILayout.IntField("測試半徑", mTestNav.detectRadius);

        if (GUILayout.Button("測角色半徑Nav點", GUILayout.Height(50)))
        {
            mTestNav.TestPlayerNav();
        }

        EditorGUILayout.Space();

        GUILayout.Label("LineRenderer 物件");
        mTestNav.line = (LineRenderer)EditorGUILayout.ObjectField(mTestNav.line, typeof(LineRenderer));
        GUILayout.Label("Line起點");
        mTestNav.gameStart = (Transform)EditorGUILayout.ObjectField(mTestNav.gameStart, typeof(Transform));
        GUILayout.Label("Line終點");
        mTestNav.gameEnd = (Transform)EditorGUILayout.ObjectField(mTestNav.gameEnd, typeof(Transform));

        mTestNav.isLineUpdate = EditorGUILayout.Toggle("是否使用Update", mTestNav.isLineUpdate, GUILayout.Height(30));

        if (GUILayout.Button("測Nav Line", GUILayout.Height(50)))
        {
            mTestNav.TestNavLine();
        }
    }
}
