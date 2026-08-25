using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Commar
{
    public class MonoBehaviourEventViewer : EditorWindow
    {
        private Vector2 _scrollPos;
        private bool[] _foldoutMethods;
        private bool[] _foldoutScripts;
        private GUIStyle _boldFoldoutStyle;
        GUIContent _selectButton;
        private string[] _tabNames = { "Script", "Event" };
        private int _tabIndex;

        private Dictionary<string, List<string>> _eventMethods = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _eventScripts = new Dictionary<string, List<string>>();
        private Dictionary<string, string> _scriptPaths = new Dictionary<string, string>();

        private static readonly string[] UnityEventMethods =
        {
        "Awake", "Start", "Update", "FixedUpdate", "LateUpdate",
        "OnEnable", "OnDisable", "OnDestroy", "OnApplicationQuit",
        "OnCollisionEnter", "OnCollisionExit", "OnTriggerEnter", "OnTriggerExit",
        "OnMouseDown", "OnMouseUp", "OnMouseDrag", "OnMouseEnter", "OnMouseExit"
    };

        [MenuItem(EditorUtil.MAIN_MENU + "/MonoBehaviour Event Viewer")]
        public static void ShowWindow()
        {
            GetWindow<MonoBehaviourEventViewer>("MonoBehaviour Event Viewer");
        }

        private void OnEnable()
        {
            ScanProjectForEvents();
            _selectButton = new GUIContent("Select");
        }

        private void OnGUI()
        {
            if (_boldFoldoutStyle == null)
            {
                _boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontSize = 15
                };
            }

            GUILayout.Space(5f);
            if (GUILayout.Button("Refresh", GUILayout.Height(25)))
            {
                ScanProjectForEvents();
            }

            EditorGUILayout.Space(10f);

            _tabIndex = GUILayout.Toolbar(_tabIndex, _tabNames);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_tabIndex)
            {
                case 0:
                    ShowEventMethods();
                    break;
                case 1:
                    ShowEventScripts();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void ShowEventMethods()
        {
            int index = 0;
            foreach (var entry in _eventMethods)
            {
                GUILayout.BeginHorizontal();
                _foldoutMethods[index] = EditorGUILayout.Foldout(_foldoutMethods[index], $"{entry.Key}", true, _boldFoldoutStyle);
                EditorGUILayout.LabelField("");
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.width = 50f;
                if (GUI.Button(rect, _selectButton))
                {
                    SelectScript(entry.Key);
                }

                GUILayout.EndHorizontal();

                if (_foldoutMethods[index])
                {
                    foreach (var method in entry.Value)
                    {
                        EditorGUILayout.LabelField($"   - {method}", EditorStyles.label);
                    }
                }
                EditorGUILayout.Space();

                index++;
            }
        }

        private void ShowEventScripts()
        {
            int index = 0;
            foreach (var key in UnityEventMethods)
            {
                if (!_eventScripts.ContainsKey(key))
                    continue;
                _foldoutScripts[index] = EditorGUILayout.Foldout(_foldoutScripts[index], $"{key}", true, _boldFoldoutStyle);

                if (_foldoutScripts[index])
                {
                    foreach (var scripts in _eventScripts[key])
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"   - {scripts}", EditorStyles.label);
                        EditorGUILayout.LabelField("");
                        Rect rect = GUILayoutUtility.GetLastRect();
                        rect.x -= 50f;
                        rect.width = 50f;
                        if (GUI.Button(rect, _selectButton))
                        {
                            SelectScript(scripts);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.Space();

                index++;
            }
        }

        private void ScanProjectForEvents()
        {
            _eventMethods.Clear();

            string[] scriptPaths = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            foreach (string scriptPath in scriptPaths)
            {
                string className = Path.GetFileNameWithoutExtension(scriptPath);
                string relativePath = "Assets" + scriptPath.Replace(Application.dataPath, string.Empty).Replace("\\", "/");

                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);
                if (monoScript == null) continue;

                Type classType = monoScript.GetClass();
                if (classType == null || !classType.IsSubclassOf(typeof(MonoBehaviour))) continue;

                List<string> methods = classType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Where(method => UnityEventMethods.Contains(method.Name))
                    .Select(method => method.Name)
                    .ToList();

                if (methods.Count > 0)
                {
                    _eventMethods[className] = methods;
                    _scriptPaths[className] = relativePath;
                    foreach (var method in methods)
                    {
                        if (!_eventScripts.ContainsKey(method))
                            _eventScripts[method] = new List<string>();
                        _eventScripts[method].Add(className);
                    }
                }
            }

            _foldoutMethods = new bool[_eventMethods.Count];
            _foldoutScripts = new bool[_eventScripts.Count];
        }

        private void SelectScript(string scriptName)
        {
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_scriptPaths[scriptName]);
            if (obj != null)
                Selection.activeObject = obj;
        }
    }
}