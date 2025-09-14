#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;

public class DVExternableEditor
{
    public static MethodInfo FindStaticMethod(string className, string methodName)
    {
        const string editorPath = "Assets/Scripts/Editor";

        MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>($"{editorPath}/{className}.cs");
        
        return script.GetClass().GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
    }
}
#endif