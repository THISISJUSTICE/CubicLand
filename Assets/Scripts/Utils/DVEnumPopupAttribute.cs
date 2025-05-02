using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DVEnumPopupAttribute : PropertyAttribute
{
    public Type EnumType { get; }

    public DVEnumPopupAttribute(Type enumType) => EnumType = enumType;
}
