using System;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomTIJI
{
    [Serializable]
    public class TypeSerializer<T> where T : class
    {
        [SerializeField] private UnityEngine.Object _object;
            
        public T Value
        {
            get
            {
                if (_value == null && _object != null)
                    _value = _object as T;

                return _value;
            }
        }

        private T _value;
    }

    namespace Editor
    {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(TypeSerializer<>))]
        internal class TypeSerializerDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                object boxedValue = property.boxedValue;
                Type type = boxedValue.GetType();
                FieldInfo objectField = type.GetField("_object", BindingFlags.Instance | BindingFlags.NonPublic);
                PropertyInfo prop = type.GetProperty("Value");

                UnityEngine.Object obj = EditorGUI.ObjectField(position, label,
                    (UnityEngine.Object)objectField.GetValue(boxedValue), prop.PropertyType, true);

                objectField.SetValue(boxedValue, obj);
                property.boxedValue = boxedValue;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}