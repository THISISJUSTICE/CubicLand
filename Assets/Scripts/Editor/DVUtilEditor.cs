using UnityEngine;
using UnityEditor;
using System;

public class DVUtilEditor : EditorWindow
{
    #region Types
    private class SceneEditor {

        private DVUtilEditor _utilEditor;

        public SceneEditor(DVUtilEditor utilEditor)
        {
            _utilEditor = utilEditor;
        }

        public void OnGUIUpdate() {
            LogSceneLists();
        }

        private void LogSceneLists() {
            if (!GUILayout.Button("Log Scene Lists"))
                return;

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            string log = "";
            for (int i = 0; i < scenes.Length; i++) {
                log += $"{System.IO.Path.GetFileNameWithoutExtension(scenes[i].path)}\n";
            }

            Debug.Log(log);
        }
    }
    #endregion

    #region Variables
    private SceneEditor _sceneEditor;

    private int _selectedTab = 0;
    private string[] _tabLabels = new string[] { "Scene" };
    #endregion

    #region Editor Functions
    [MenuItem("Custom/Editor Utils/Util Editor")]
    public static void ShowWindow()
    {
        GetWindow<DVUtilEditor>("Util Editor");
    }

    private void OnEnable()
    {
        _sceneEditor = new SceneEditor(this);
    }

    private void OnGUI()
    {
        SelectTabTemplate(ref _selectedTab, ref _tabLabels, new Action[] {
            () => _sceneEditor.OnGUIUpdate(),
        });
    }
    #endregion

    #region GUI Functions
    #endregion

    #region
    public void SelectTabTemplate(ref int selectedTab, ref string[] tabLables, Action[] callbacks)
    {
        selectedTab = GUILayout.Toolbar(selectedTab, tabLables);
        GUILayout.Space(15);

        callbacks[selectedTab]();
    }
    #endregion
}
