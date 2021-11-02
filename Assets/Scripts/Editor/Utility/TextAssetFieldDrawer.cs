using Core.Framework.Utility;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TextAssetField))]
public class TextAssetFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var pathProp = property.FindPropertyRelative("path");
        var guidProp = property.FindPropertyRelative("guid");

        TextAsset asset = null;
        if(!string.IsNullOrEmpty(guidProp.stringValue))
        {
            var newPath = AssetDatabase.GUIDToAssetPath(guidProp.stringValue);
            if (newPath != pathProp.stringValue)
                pathProp.stringValue = newPath;
            asset = AssetDatabase.LoadAssetAtPath<TextAsset>(pathProp.stringValue);
        }

        EditorGUI.BeginChangeCheck();
        asset = (TextAsset)EditorGUI.ObjectField(position, "TextAsset", asset, typeof(TextAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            if(asset != null)
            {
                pathProp.stringValue = AssetDatabase.GetAssetPath(asset);
                var guid = AssetDatabase.GUIDFromAssetPath(pathProp.stringValue);
                guidProp.stringValue = guid.ToString();
            }
            else
            {
                pathProp.stringValue = string.Empty;
                guidProp.stringValue = string.Empty;
            }
        }
    }
}
