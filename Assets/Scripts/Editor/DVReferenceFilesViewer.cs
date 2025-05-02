using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class DVReferenceFilesViewer : EditorWindow
{
    #region Variables
    private Vector2 _scrollPosition;

    private List<string> _files;
    #endregion

    #region Editor Functions
    public static void OpenWindow(List<string> files)
    {
        DVReferenceFilesViewer window = GetWindow<DVReferenceFilesViewer>("Reference Files Viewer");
        window._files = files;
        window.Show();
    }

    private void OnEnable()
    {
        
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

        SelectFileButtons();

        EditorGUILayout.EndScrollView();
    }
    #endregion

    #region GUI Functions
    private void SelectFileButtons() {
        GUILayout.Space(5f);

        foreach (var file in _files)
        {
            string fileName = Path.GetFileName(file);
            if (GUILayout.Button(fileName)) {
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(file);
                if (obj != null)
                    Selection.activeObject = obj;
            }
            GUILayout.Space(5f);
        }
    }
    #endregion
}
