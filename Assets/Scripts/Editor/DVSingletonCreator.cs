using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class DVSingletonCreator
{
    private const string SINGLETON_PATH = "Assets/Prefabs/InScene/Singletons";

    [MenuItem("Custom Editor Utils/Singleton/Create Singletons")]
    public static void CreateSingletons()
    {
        if (!Directory.Exists(SINGLETON_PATH))
            Directory.CreateDirectory(SINGLETON_PATH);

        List<Type> singletonTypes = FindSingletonTypes();

        foreach (Type type in singletonTypes)
        {
            GameObject prefab = LoadPrefab(type);

            if (prefab.GetComponent(type) == null)
                prefab.AddComponent(type);

            AssetDatabase.Refresh();
        }

        AssetDatabase.SaveAssets();
    }

    [MenuItem("Custom Editor Utils/Singleton/Load Singletons to Scene")]
    public static void LoadSingletons()
    {
        SceneSelector.LoadScene("Assets/Scenes/Intro.unity");

        List<Type> singletonTypes = FindSingletonTypes();
        MethodInfo mainMethod = typeof(DVSingletonCreator).
            GetMethod(nameof(LoadSingleton), BindingFlags.Static | BindingFlags.Public);

        foreach (Type type in singletonTypes)
        {
            MethodInfo method = mainMethod.MakeGenericMethod(type);
            method.Invoke(null, null);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    public static T LoadSingleton<T>() where T : SingletonMonoBehaviour<T>
    {
        T singleton = GameObject.FindFirstObjectByType<T>();
        if (singleton != null)
            return singleton;

        GameObject prefab = LoadPrefab(typeof(T));

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        SingletonMonoBehaviour<T>.SetSingletonParent(go.transform);

        singleton = go.GetComponent<T>();

        return singleton;
    }

    private static List<Type> FindSingletonTypes()
    {
        string[] guids = AssetDatabase.FindAssets("t:MonoScript", new string[] { "Assets/Scripts" });
        List<Type> singletonTypes = new List<Type>();

        foreach (string guid in guids)
        {
            string scriptPath = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            Type type = script.GetClass();

            if (ValidateSingletonType(type))
                singletonTypes.Add(type);
        }

        return singletonTypes;
    }

    private static bool ValidateSingletonType(Type type)
    {
        if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type))
            return false;

        while (type != null && type != typeof(MonoBehaviour))
        {
            Type generic = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (generic == typeof(SingletonMonoBehaviour<>))
                return true;

            type = type.BaseType;
        }

        return false;
    }

    private static GameObject LoadPrefab(Type type)
    {
        string prefabName = GetPrefabName(type);
        string prefabPath = $"{SINGLETON_PATH}/{prefabName}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            GameObject go = new GameObject(prefabName);
            go.transform.Reset();

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            GameObject.DestroyImmediate(go);

            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        return prefab;
    }

    private static string GetPrefabName(Type type) => DVUtil.GetTypeName(type);
}
