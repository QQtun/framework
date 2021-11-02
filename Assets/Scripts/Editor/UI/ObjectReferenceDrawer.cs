using Core.Framework.Utility;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectReference))]
public class ObjectReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var nameProp = property.FindPropertyRelative("name");
        var objProp = property.FindPropertyRelative("obj");

        var rect = position;
        rect.height *= 0.95f;
        rect.width = position.width * 0.49f;
        nameProp.stringValue = EditorGUI.TextField(rect, nameProp.stringValue);

        rect.x += position.width * 0.52f;
        rect.width = position.width * 0.49f;
        EditorGUI.BeginChangeCheck();
        objProp.objectReferenceValue = EditorGUI.ObjectField(rect, objProp.objectReferenceValue, typeof(GameObject), true);
        if(EditorGUI.EndChangeCheck())
        {
            if (objProp.objectReferenceValue != null)
                nameProp.stringValue = objProp.objectReferenceValue.name;
            else
                nameProp.stringValue = string.Empty;
        }
    }
}
