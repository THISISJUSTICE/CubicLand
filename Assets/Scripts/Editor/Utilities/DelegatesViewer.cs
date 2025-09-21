using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Linq;

namespace CustomTIJI
{
    public class DelegatesViewer : EditorWindow
    {
        #region GUI Variables
        private Vector2 _scrollPosition;
        private bool[] _delFolds;
        private bool[] _ueFolds;
        private Dictionary<Delegate, bool> _delTargetFolds;
        private Dictionary<UnityEventBase, bool> _ueTargetFolds;
        #endregion

        #region Variables
        private bool _loaded = false;

        private MonoBehaviour[] _activeScripts;
        private Dictionary<UnityEngine.Object, GameObject> _monoObjects;
        private Dictionary<UnityEngine.Object, List<Delegate>> _delegates;
        private Dictionary<Delegate, List<MonoBehaviour>> _delegateTracks;
        private Dictionary<UnityEngine.Object, List<UnityEventBase>> _unityEvents;
        private Dictionary<UnityEventBase, List<MonoBehaviour>> _eventTracks;
        private Dictionary<UnityEventBase, string> _eventKeyTitles;

        #endregion

        #region Editor Functions
        [MenuItem("Custom Editor Utils/Delegates Viewer")]
        public static void OpenWindow()
        {
            DelegatesViewer window = (DelegatesViewer)EditorWindow.GetWindow(typeof(DelegatesViewer));
            window.Show();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;

            if (!EditorApplication.isPlaying)
                return;

            Refresh();
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
                return;

            if (GUILayout.Button("Refresh"))
            {
                Refresh();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 30f));

            if (!_loaded)
                return;

            GUILayout.Space(10f);

            DrawDelegates();
            DrawUnityEvents();

            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region GUI Functions
        private void DrawDelegates()
        {
            DrawList(ref _delegates, ref _delegateTracks, ref _delFolds, ref _delTargetFolds);
        }

        private void DrawUnityEvents()
        {
            GUILayout.Space(20f);
            GUILayout.Label("Unity Events", EditorStyles.boldLabel);
            DrawList(ref _unityEvents, ref _eventTracks, ref _ueFolds, ref _ueTargetFolds);
        }

        private void DrawList<V>(ref Dictionary<UnityEngine.Object, List<V>> events, ref Dictionary<V, List<MonoBehaviour>> tracks,
            ref bool[] folds, ref Dictionary<V, bool> tFolds)
        {
            int index = 0;
            float offsetX = 20f;
            List<KeyValuePair<UnityEngine.Object, List<V>>> eventList = events.ToList();
            eventList.Sort((a, b) => a.Key.name.CompareTo(b.Key.name));

            foreach (var evt in eventList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(offsetX + 5f);

                string objName = $"{evt.Key.name}";
                if (_monoObjects.ContainsKey(evt.Key))
                    objName = $"{_monoObjects[evt.Key].name}({evt.Key.GetType()})";

                folds[index] = EditorGUILayout.Foldout(folds[index], objName);
                Rect rect = GUILayoutUtility.GetLastRect();
                GUILayout.EndHorizontal();

                if (folds[index])
                {
                    List<string> restKeys = new List<string>();
                    foreach (var key in evt.Value)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(offsetX + 15f);
                        bool isFold = tFolds.ContainsKey(key);

                        string keyTitle = key.ToString();
                        if (key is UnityEventBase ue && _eventKeyTitles.ContainsKey(ue))
                        {
                            keyTitle = _eventKeyTitles[ue];
                        }

                        if (isFold)
                            tFolds[key] = EditorGUILayout.Foldout(tFolds[key], $"{keyTitle}");
                        else
                        {
                            GUILayout.Space(13f);
                            restKeys.Add($"{key}");
                            //EditorGUILayout.LabelField($"{key}");
                        }
                        GUILayout.EndHorizontal();
                        if (isFold && tFolds[key])
                        {
                            foreach (var target in tracks[key])
                            {
                                if (target == null)
                                    continue;

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(offsetX + 30f);
                                if (GUILayout.Button($"{target.name} ({target.GetType()})"))
                                {
                                    Selection.activeObject = target;
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                    }

                    foreach (var restKey in restKeys)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(offsetX + 28f);
                        EditorGUILayout.LabelField($"{restKey}");
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(3f);
                }
                rect.x -= offsetX;
                rect.width = rect.height;

                if (GUI.Button(rect, "◯"))
                {
                    Selection.activeObject = evt.Key;
                }

                GUILayout.Space(5f);
                index++;
            }
        }
        #endregion

        #region Utils
        private void Refresh()
        {
            _loaded = false;
            SetupViewer();
            SetGUIOptions();
            _loaded = true;
        }

        private void SetupViewer()
        {
            _activeScripts = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            _monoObjects = new Dictionary<UnityEngine.Object, GameObject>();
            _delegates = new Dictionary<UnityEngine.Object, List<Delegate>>();
            _delegateTracks = new Dictionary<Delegate, List<MonoBehaviour>>();

            _unityEvents = new Dictionary<UnityEngine.Object, List<UnityEventBase>>();
            _eventTracks = new Dictionary<UnityEventBase, List<MonoBehaviour>>();
            _eventKeyTitles = new Dictionary<UnityEventBase, string>();

            foreach (var script in _activeScripts)
            {
                Type type = script.GetType();
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                foreach (var field in fields)
                {
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                    {
                        Delegate del = field.GetValue(script) as Delegate;
                        if (del != null)
                        {
                            TrackDelegate(script, del);
                            _monoObjects[script] = script.gameObject;
                        }
                    }

                    if (typeof(UnityEvent).IsAssignableFrom(field.FieldType))
                    {
                        UnityEventBase unityEvent = field.GetValue(script) as UnityEventBase;
                        if (unityEvent != null)
                        {
                            TrackUnityEvent(script, unityEvent);
                            _monoObjects[script] = script.gameObject;
                        }
                    }
                }
            }

            var monoScriptGUIDs = AssetDatabase.FindAssets("t:MonoScript");
            foreach (var guid in monoScriptGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (monoScript == null)
                    continue;

                Type scriptType = monoScript.GetClass();
                if (scriptType == null || !scriptType.IsClass || !scriptType.IsAbstract)
                    continue;
                FieldInfo[] fields = scriptType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var field in fields)
                {
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                    {
                        Delegate del = field.GetValue(null) as Delegate;

                        if (del != null)
                            TrackDelegate(monoScript, del);
                    }

                    if (typeof(UnityEvent).IsAssignableFrom(field.FieldType))
                    {
                        UnityEventBase unityEvent = field.GetValue(null) as UnityEventBase;

                        if (unityEvent != null)
                            TrackUnityEvent(monoScript, unityEvent);
                    }
                }
            }
        }

        private void SetGUIOptions()
        {
            _delFolds = new bool[_delegates.Count];
            _ueFolds = new bool[_unityEvents.Count];

            _delTargetFolds = new Dictionary<Delegate, bool>();
            foreach (var track in _delegateTracks)
            {
                if (track.Value.Count > 0)
                    _delTargetFolds[track.Key] = false;
            }

            _ueTargetFolds = new Dictionary<UnityEventBase, bool>();
            foreach (var evt in _eventTracks)
            {
                _ueTargetFolds[evt.Key] = false;
            }
        }

        private void TrackDelegate(UnityEngine.Object source, Delegate del)
        {
            if (!_delegates.ContainsKey(source))
                _delegates[source] = new List<Delegate>();
            _delegates[source].Add(del);

            var invocationList = del.GetInvocationList();
            foreach (var method in invocationList)
            {
                foreach (var script in _activeScripts)
                {
                    if (script != (UnityEngine.Object)method.Target)
                        continue;

                    if (!_delegateTracks.ContainsKey(del))
                        _delegateTracks[del] = new List<MonoBehaviour>();
                    _delegateTracks[del].Add(script);
                }
            }
        }

        private void TrackUnityEvent(UnityEngine.Object source, UnityEventBase unityEvent)
        {
            if (!_unityEvents.ContainsKey(source))
                _unityEvents[source] = new List<UnityEventBase>();
            _unityEvents[source].Add(unityEvent);

            for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            {
                var target = unityEvent.GetPersistentTarget(i);

                if (target == null)
                    continue;

                foreach (var script in _activeScripts)
                {
                    if (script != target)
                        continue;

                    if (!_eventTracks.ContainsKey(unityEvent))
                        _eventTracks[unityEvent] = new List<MonoBehaviour>();
                    _eventTracks[unityEvent].Add(script);
                }
            }

            var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
            var callGroup = field.GetValue(unityEvent);
            var callsField = callGroup.GetType().GetField("m_RuntimeCalls", BindingFlags.NonPublic | BindingFlags.Instance);
            var runtimeCalls = callsField.GetValue(callGroup) as System.Collections.IList;
            foreach (var call in runtimeCalls)
            {
                var delegateField = call.GetType().GetField("Delegate", BindingFlags.NonPublic | BindingFlags.Instance);
                var del = delegateField.GetValue(call) as Delegate;
                var target = del.Target;

                foreach (var script in _activeScripts)
                {
                    if (script != (UnityEngine.Object)target)
                        continue;

                    if (!_eventTracks.ContainsKey(unityEvent))
                        _eventTracks[unityEvent] = new List<MonoBehaviour>();
                    _eventTracks[unityEvent].Add(script);
                }
            }
        }

        private void HandleActiveSceneChanged(Scene current, Scene next)
        {
            if (!EditorApplication.isPlaying)
                return;

            Refresh();
        }
        #endregion
    }
}