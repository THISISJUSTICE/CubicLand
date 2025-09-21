using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace CustomTIJI
{
    public static class EditorUtil
    {
        #region Vector Fields
        public static Vector3 LayoutVector3Field(string label, Vector3 vec3)
        {
            return LayoutVector3Field(label, vec3, out Rect labelRect);
        }

        public static Vector3 LayoutVector3Field(string label, Vector3 vec3, out Rect labelRect)
        {
            Vector3 res = vec3;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            labelRect = GUILayoutUtility.GetLastRect();

            res.x = DrawVectorElement("x", vec3.x);
            GUILayout.Space(20f);
            res.y = DrawVectorElement("y", vec3.y);
            GUILayout.Space(20f);
            res.z = DrawVectorElement("z", vec3.z);
            GUILayout.EndHorizontal();

            return res;
        }

        public static Vector3 LayoutVector3Field(Vector3 vec3)
        {
            Vector3 res = vec3;

            GUILayout.BeginHorizontal();
            res.x = DrawVectorElement("x", vec3.x);
            GUILayout.Space(20f);
            res.y = DrawVectorElement("y", vec3.y);
            GUILayout.Space(20f);
            res.z = DrawVectorElement("z", vec3.z);
            GUILayout.EndHorizontal();

            return res;
        }

        private static float DrawVectorElement(string label, float value)
        {
            float res = EditorGUILayout.FloatField(value);
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.width = 15f;
            rect.x -= rect.width;
            EditorGUI.LabelField(rect, label);
            OnHandleLabelDrag(rect, (delta) =>
            {
                res += delta.x * 0.01f;
            });

            return res;
        }
        #endregion

        #region LayerMask Field
        public static LayerMask LayerMaskField(string label, LayerMask selected)
        {
            var layerNames = new List<string>();
            var layerIndices = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string name = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(name))
                {
                    layerNames.Add(name);
                    layerIndices.Add(i);
                }
            }

            // 현재 LayerMask를 기반으로 체크된 상태 계산
            int shownMask = 0;
            for (int i = 0; i < layerIndices.Count; i++)
            {
                if (((1 << layerIndices[i]) & selected.value) != 0)
                {
                    shownMask |= (1 << i);
                }
            }

            // 팝업 UI
            shownMask = EditorGUILayout.MaskField(label, shownMask, layerNames.ToArray());

            // MaskField 결과를 실제 LayerMask 값으로 변환
            int finalMask = 0;
            for (int i = 0; i < layerIndices.Count; i++)
            {
                if ((shownMask & (1 << i)) != 0)
                {
                    finalMask |= (1 << layerIndices[i]);
                }
            }

            selected.value = finalMask;
            return selected;
        }
        #endregion

        public static void OnHandleLabelDrag(Rect rect, Action<Vector2> onDraggedCallback)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);
            int controlId = GUIUtility.GetControlID(FocusType.Passive, rect);

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (rect.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        onDraggedCallback?.Invoke(e.delta); // 드래그 감도 조정은 callback 함수에서 진행
                        e.Use();
                        GUI.changed = true;
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;
            }
        }

        #region Design
        public static void DrawHorizontalLine(Color color, float thickness = 1f, int padding = 10)
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding));
            rect.height = thickness;
            rect.y += padding / 2;
            EditorGUI.DrawRect(rect, color);
        }

        public static int DrawPopup(int index, string[] contents, Rect rect, float xOffset = 100f, float width = 150f)
        {
            rect.x = rect.x + xOffset;
            rect.width = width;

            GUIContent[] gContents = new GUIContent[contents.Length];
            for (int i = 0; i < gContents.Length; i++)
            {
                gContents[i] = new GUIContent(contents[i]);
            }

            return EditorGUI.Popup(rect, index, gContents);
        }

        public static int DrawPopup(int index, string[] contents, float xOffset = 100f, float width = 150f)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            return DrawPopup(index, contents, rect, xOffset, width);
        }
        #endregion

        #region Path
        public static string DrawSaveFilePanel(string title, string directory, string fileName, string extension)
        {
            string filePath = EditorUtility.SaveFilePanel(title, directory, fileName, extension);

            string temp = Path.GetDirectoryName(filePath);
            if (CheckValidDirectory(ref temp))
                directory = temp;

            temp = Path.GetFileNameWithoutExtension(filePath);
            if (!string.IsNullOrEmpty(temp))
                fileName = temp;

            return $"{directory}/{fileName}.{extension}";
        }

        public static string DrawOpenFolderPanel(string filePath)
        {
            string folderPath = EditorUtility.OpenFolderPanel("", filePath, "");

            if (CheckValidDirectory(ref folderPath))
                return folderPath;

            return filePath;
        }

        private static bool CheckValidDirectory(ref string filePath)
        {
            const string MainPath = "Assets";

            if (string.IsNullOrEmpty(filePath))
                return false;

            filePath = filePath.Replace("\\", "/");
            string projectPath = Directory.GetCurrentDirectory().Replace("\\", "/");

            if (filePath.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = filePath.Substring(projectPath.Length + 1);
                if (relativePath.StartsWith(MainPath))
                    filePath = relativePath;
            }

            return filePath.StartsWith(MainPath) && Directory.Exists(filePath);
        }

        public static List<UnityEngine.Object> LoadAllAssets(params string[] directories)
        {
            List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
            string[] assetGUIDs = AssetDatabase.FindAssets("", directories);

            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (obj != null)
                    assets.Add(obj);
            }

            return assets;
        }
        #endregion

        public static IList<T> DrawAssetList<T>(string label, ref bool foldout, IList<T> list, bool allowSceneObjects) where T : UnityEngine.Object
        {
            if (list == null)
                return null;

            foldout = EditorGUILayout.Foldout(foldout, $"{label}");

            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect rect = new Rect(label.Length * 10f, lastRect.y, 30f, lastRect.height);
            int count = EditorGUI.IntField(rect, list.Count);
            count = Mathf.Min(count, 999);

            if (count != list.Count)
            {
                if (count > list.Count)
                {
                    while (count > list.Count)
                        list.Add(null);
                }
                else
                {
                    while (count < list.Count)
                        list.RemoveAt(list.Count - 1);
                }

            }

            if (foldout)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i != 0)
                        GUILayout.Space(5f);
                    list[i] = EditorGUILayout.ObjectField(" ", list[i], typeof(T), allowSceneObjects) as T;
                }

                if (list.Count > 0)
                    GUILayout.Space(5f);
            }

            GUILayout.BeginHorizontal();

            float width = EditorGUIUtility.currentViewWidth;

            if (allowSceneObjects)
                GUILayout.Space(width * 0.6f);
            else
            {
                GUILayout.Space(width * 0.5f);
                if (GUILayout.Button("Folder"))
                {
                    HashSet<T> set = new HashSet<T>();
                    for (int i = 0; i < list.Count; i++)
                        set.Add(list[i]);

                    string directory = DrawOpenFolderPanel("Assets");
                    List<UnityEngine.Object> objects = LoadAllAssets(directory);
                    foreach (UnityEngine.Object obj in objects)
                    {
                        if (obj is T asset)
                            set.Add(asset);
                    }

                    list.Clear();
                    foreach (T item in set)
                        list.Add(item);
                }
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("+"))
                list.Add(null);
            GUILayout.Space(5f);
            if (GUILayout.Button("-") && list.Count > 0)
                list.RemoveAt(list.Count - 1);

            GUILayout.EndHorizontal();

            return list;
        }

        public static void SpaceHorizontalLayout(float space, Action draw)
        {
            if (draw == null)
                return;

            GUILayout.BeginHorizontal();
            GUILayout.Space(space);
            draw.Invoke();
            GUILayout.Space(space);
            GUILayout.EndHorizontal();
        }
    }
}