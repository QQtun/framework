using Core.Framework.Event.Property;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubscribableField<>))]
public class SubscribableFieldDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 3 + 3;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var target = GetTarget(property); 

        var screenW = Screen.width;
        var fieldNameWidth = EditorGUIUtility.labelWidth - (13 + (EditorGUI.indentLevel - 1) * 15);
        var fieldNameRect = new Rect(position.x, position.y, fieldNameWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(fieldNameRect, property.displayName);

        var valueNameWidth = EditorGUIUtility.labelWidth - (13 + (EditorGUI.indentLevel - 1 + 1) * 15);
        var valueNameRect = new Rect(position.x + 15,
            position.y + EditorGUIUtility.singleLineHeight, valueNameWidth, EditorGUIUtility.singleLineHeight);

        EditorGUI.LabelField(valueNameRect, "Value");

        var valueRect = new Rect(position.x + valueNameWidth + 15,
            position.y + EditorGUIUtility.singleLineHeight, screenW - valueNameWidth - position.x - 22, EditorGUIUtility.singleLineHeight);
        var valueField = property.FindPropertyRelative(SubscribableField.ValueFieldName);
        DrawProperty(valueRect, valueField, target, true);

        var lastValueNameRect = valueNameRect;
        lastValueNameRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(lastValueNameRect, "LastValue");

        var lastValueRect = valueRect;
        lastValueRect.y += EditorGUIUtility.singleLineHeight + 2;
        var lastValueField = property.FindPropertyRelative(SubscribableField.LastValueFieldName);
        var old = GUI.enabled;
        GUI.enabled = false;
        DrawProperty(lastValueRect, lastValueField, target, false);
        GUI.enabled = old;
    }

    private object GetTarget(SerializedProperty property)
    {
        var paths = property.propertyPath.Split('.');
        object parent = property.serializedObject.targetObject;
        object target = property.serializedObject.targetObject;
        for (int i = 0; i < paths.Length; i++)
        {
            var targetFeildInfo = parent.GetType().GetField(paths[i]);
            target = targetFeildInfo.GetValue(parent);
            parent = target;
        }
        return target;
    }

    private void DrawProperty(Rect valueRect, SerializedProperty valueField, object target, bool setValueWhenChanged)
    {
        var valuePropertyInfo = fieldInfo.FieldType.GetProperty("Value");

        if (valueField.propertyType == SerializedPropertyType.Integer)
        {
            var newValue = EditorGUI.IntField(valueRect, valueField.intValue);
            if(setValueWhenChanged && newValue != valueField.intValue)
            {
                valuePropertyInfo.SetValue(target, newValue);
            }
        }
        else if(valueField.propertyType == SerializedPropertyType.Float)
        {
            var newValue = EditorGUI.FloatField(valueRect, valueField.floatValue);
            if (setValueWhenChanged && newValue != valueField.floatValue)
            {
                valuePropertyInfo.SetValue(target, newValue);
            }
        }
        else if (valueField.propertyType == SerializedPropertyType.String)
        {
            var newValue = EditorGUI.TextField(valueRect, valueField.stringValue);
            if (setValueWhenChanged && newValue != valueField.stringValue)
            {
                valuePropertyInfo.SetValue(target, newValue);
            }
        }
        else if (valueField.propertyType == SerializedPropertyType.Boolean)
        {
            var newValue = EditorGUI.Toggle(valueRect, valueField.boolValue);
            if (setValueWhenChanged && newValue != valueField.boolValue)
            {
                valuePropertyInfo.SetValue(target, newValue);
            }
        }
        else if (valueField.propertyType == SerializedPropertyType.ObjectReference)
        {
            var subscribableType = target.GetType();
            var genericArgs = subscribableType.GetGenericArguments();
            System.Type pType = null;
            if (genericArgs != null && genericArgs.Length > 0)
                pType = genericArgs[0];
            else
                return;

            var newValue = EditorGUI.ObjectField(valueRect, valueField.objectReferenceValue, pType, true);
            if (setValueWhenChanged && newValue != valueField.objectReferenceValue)
            {
                valuePropertyInfo.SetValue(target, newValue);
            }
        }
        else if (valueField.propertyType == SerializedPropertyType.Enum)
        {
            var subscribableType = target.GetType();
            var genericArgs = subscribableType.GetGenericArguments();
            System.Type pType = null;
            if (genericArgs != null && genericArgs.Length > 0)
                pType = genericArgs[0];
            else
                return;

            var enumValues = pType.GetEnumValues();
            var oldValue = (System.Enum)enumValues.GetValue(valueField.enumValueIndex);
            var newValue = EditorGUI.EnumPopup(valueRect, oldValue);
            if (setValueWhenChanged && newValue != oldValue)
            {
                valuePropertyInfo.SetValue(target, newValue);
            }
        }
        else
        {
            // TODO
            // Debug.LogWarning("sorry, not supported");
        }
    }
}