using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomTIJI
{
    /// <summary>
    /// Target Type: string
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumPopupAttribute : PropertyAttribute
    {
        public Type EnumType { get; }

        public EnumPopupAttribute(Type enumType) => EnumType = enumType;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnumPopupAttribute))]
    public class EnumPopupAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumPopupAttribute dvEnum = (EnumPopupAttribute)attribute;

            // 여기서 타입 검사
            if (property.propertyType != SerializedPropertyType.String
                || (dvEnum.EnumType == null || !dvEnum.EnumType.IsEnum))
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string[] enumNames = System.Enum.GetNames(dvEnum.EnumType);
            int currentIndex = Mathf.Max(0, System.Array.IndexOf(enumNames, property.stringValue));
            int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, enumNames);

            property.stringValue = enumNames[selectedIndex];
        }
    }
#endif
}