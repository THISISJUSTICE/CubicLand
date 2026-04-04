using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomTIJI
{
    /// <summary>
    /// Target Type: int
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumFlagAttribute : PropertyAttribute
    {
        public Type EnumType { get; }

        public EnumFlagAttribute(Type enumType) => EnumType = enumType;
    }

    namespace Editor
    {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(EnumFlagAttribute))]
        internal class EnumFlagAttributeDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EnumFlagAttribute enumFlag = (EnumFlagAttribute)attribute;

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

                EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
                int value = EditorGUI.MaskField(position, label, property.intValue, enumNames);
                EditorGUI.showMixedValue = false;

                if (!property.hasMultipleDifferentValues)
                    property.intValue = value;
            }
        }
#endif
    }
}