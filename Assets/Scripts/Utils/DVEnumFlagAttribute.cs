using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Target Type: int
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DVEnumFlagAttribute : PropertyAttribute
{
    public Type EnumType { get; }

    public DVEnumFlagAttribute(Type enumType) => EnumType = enumType;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(DVEnumFlagAttribute))]
public class DVEnumFlagAttributeEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DVEnumFlagAttribute enumFlag = (DVEnumFlagAttribute)attribute;

        if (property.propertyType != SerializedPropertyType.Integer
            || (enumFlag.EnumType == null || !enumFlag.EnumType.IsEnum))
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        string[] enumNames = Enum.GetNames(enumFlag.EnumType);
        Array enumValues = Enum.GetValues(enumFlag.EnumType);
        int[] maskValues = new int[enumNames.Length];

        for (int i = 0; i < enumNames.Length; i++)
            maskValues[i] = 1 << (int)enumValues.GetValue(i);

        property.intValue = EditorGUI.MaskField(position, label, property.intValue, enumNames);
    }
}
#endif