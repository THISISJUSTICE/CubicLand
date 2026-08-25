using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Commar
{
    public class AssetOpener : EditorWindow
    {
        private const string MAIN_PATH = "Assets";

        private Vector2 _scrollPosition;
        private bool _foldout = false;
        private string _filePathText = "";
        private UnityEngine.Object _fileObject;

        private HashSet<string> _filePaths = new HashSet<string>();

        [MenuItem(EditorUtil.MAIN_MENU + "/Asset Opener")]
        private static void ShowWindow()
        {
            GetWindow<AssetOpener>("Asset Opener");
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

            GUILayout.Space(5f);
            _filePathText = EditorGUILayout.TextField("Asset Path", _filePathText);

            GUILayout.Space(5f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Folder"))
            {
                string filePath = GetPathToOpenFolder();
                if (!string.IsNullOrEmpty(filePath))
                {
                    _filePathText = filePath;
                    OnGUI();
                }
            }

            GUILayout.Space(15f);
            if (GUILayout.Button("Add"))
            {
                if (CheckValidFile(_filePathText))
                    _filePaths.Add(_filePathText);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            _fileObject = EditorGUILayout.ObjectField("Asset", _fileObject, typeof(UnityEngine.Object), false);

            GUILayout.Space(10f);
            if (GUILayout.Button("Add"))
            {
                if (_fileObject != null)
                    _filePaths.Add(AssetDatabase.GetAssetPath(_fileObject));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(40f);
            _foldout = EditorGUILayout.Foldout(_foldout, "Files");

            string deletePath = "";
            if (_foldout)
            {
                foreach (var filePath in _filePaths)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    string fileName = Path.GetFileName(filePath);
                    if (GUILayout.Button(fileName))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
                    }
                    GUILayout.Space(10f);
                    if (GUILayout.Button("Delete"))
                    {
                        deletePath = filePath;
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(deletePath))
                _filePaths.Remove(deletePath);
        }

        private string GetPathToOpenFolder()
        {
            string filePath = EditorUtility.OpenFilePanel("", MAIN_PATH, "");

            if (string.IsNullOrEmpty(filePath))
                return "";

            string projectPath = Directory.GetCurrentDirectory().Replace("\\", "/");
            filePath = filePath.Replace("\\", "/");

            if (filePath.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = filePath.Substring(projectPath.Length + 1);
                if (relativePath.StartsWith(MAIN_PATH))
                    filePath = relativePath;
            }

            if (filePath.StartsWith(MAIN_PATH) && !filePath.EndsWith(".meta") && File.Exists(filePath))
            {
                return filePath;
            }

            return "";
        }

        private bool CheckValidFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            if (filePath.StartsWith(MAIN_PATH) && !filePath.EndsWith(".meta") && File.Exists(filePath))
            {
                return true;
            }

            return false;
        }
    }
}