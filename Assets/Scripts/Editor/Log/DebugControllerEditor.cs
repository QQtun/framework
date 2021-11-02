using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Game.Log;
using UnityEditor;
using UnityEngine;
using LogUtil;

[CustomEditor(typeof(DebugController))]
public class DebugControllerEditor : Editor
{
    public class PropertyActionBind<T> where T : class
    {
        public T target;
        public string propertyPath;
        public SerializedProperty property;
        public Action<T> action;

        public PropertyActionBind(SerializedObject so, string path, Action<T> action)
        {
            target = so.targetObject as T;
            propertyPath = path;
            property = so.FindProperty(path);
            this.action = action;
        }
    }

    private List<DebugController.LogSetting> _debugTagTypes = new List<DebugController.LogSetting>();
    private List<PropertyActionBind<DebugController>> _actionBinding = new List<PropertyActionBind<DebugController>>();

    private SerializedProperty _list;

    private void OnEnable()
    {
        _debugTagTypes.Clear();

        List<string> logTypes = new List<string>();
        Type[] checkTypes = new Type[] { typeof(LogTag), typeof(LogTagEx) };

        foreach (var type in checkTypes)
        {
            logTypes.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.FieldType.IsSubclassOf(typeof(LogTag)) || x.FieldType == typeof(LogTag)).Select(x => (x.GetValue(type) as LogTag).ToString()));
        }

        foreach (string t in logTypes)
        {
            _debugTagTypes.Add(new DebugController.LogSetting()
            {
                tagName = t,
                visible = VisiblePack.All,
                tagColor = false,
                color = Color.black,
            });
        }

        DebugController controller = target as DebugController;

        if (controller.TagList.Count != _debugTagTypes.Count)
        {
            var list = controller.TagList;
            // update taglist
            var backup = list.ToList();
            controller.TagList.Clear();

            controller.TagList.AddRange(_debugTagTypes);

            foreach (var tag in backup)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].tagName == tag.tagName)
                    {
                        // restore values
                        list[i] = tag;
                    }
                }
            }
            EditorUtility.SetDirty(controller);
        }

        // property action bind
        _actionBinding.Add(new PropertyActionBind<DebugController>(serializedObject, "mIsShowLog", c => c.ShowLogChange()));
        _actionBinding.Add(new PropertyActionBind<DebugController>(serializedObject, "mIsShowTime", c => c.SetShowTime()));
        _actionBinding.Add(new PropertyActionBind<DebugController>(serializedObject, "mIsShowTagString", c => c.SetShowTagString()));

        _list = serializedObject.FindProperty("mList");
    }

    public override void OnInspectorGUI()
    {
        bool isTagVisibleChanged = false;
        bool isTagColorChanged = false;

        serializedObject.Update();

        foreach (var bind in _actionBinding)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(bind.property, true);
            if (EditorGUI.EndChangeCheck())
            {
                if (bind.action != null && EditorApplication.isPlaying)
                {
                    bind.action(bind.target);
                }
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Visible[ I W E ] / Tag Name / Tag Color / Color");
        for (int i = 0; i < _list.arraySize; i++)
        {
            var item = _list.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(item.FindPropertyRelative("visible.Info"), GUIContent.none, GUILayout.Width(30));
            EditorGUILayout.PropertyField(item.FindPropertyRelative("visible.Warning"), GUIContent.none, GUILayout.Width(30));
            EditorGUILayout.PropertyField(item.FindPropertyRelative("visible.Error"), GUIContent.none, GUILayout.Width(30));
            if (EditorGUI.EndChangeCheck()) { isTagVisibleChanged = true; }

            EditorGUILayout.LabelField(item.FindPropertyRelative("tagName").stringValue, GUILayout.Width(100));

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(item.FindPropertyRelative("tagColor"), GUIContent.none, GUILayout.Width(30));
            EditorGUILayout.PropertyField(item.FindPropertyRelative("color"), GUIContent.none, GUILayout.Width(120));
            if (EditorGUI.EndChangeCheck()) { isTagColorChanged = true; }

            if (GUILayout.Button("All ON", GUILayout.Width(50)))
            {
                item.FindPropertyRelative("visible.Info").boolValue = true;
                item.FindPropertyRelative("visible.Warning").boolValue = true;
                item.FindPropertyRelative("visible.Error").boolValue = true;
                item.FindPropertyRelative("visible.Exception").boolValue = true;
                isTagVisibleChanged = true;
            }
            else if (GUILayout.Button("All OFF", GUILayout.Width(50)))
            {
                item.FindPropertyRelative("visible.Info").boolValue = false;
                item.FindPropertyRelative("visible.Warning").boolValue = false;
                item.FindPropertyRelative("visible.Error").boolValue = false;
                item.FindPropertyRelative("visible.Exception").boolValue = false;
                isTagVisibleChanged = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();

        DebugController controller = target as DebugController;

        if (controller)
        {
            if (isTagVisibleChanged && EditorApplication.isPlaying)
            {
                controller.SetVisibility();
            }
            if (isTagColorChanged && EditorApplication.isPlaying)
            {
                controller.SetTagColor();
            }
        }
    }
}