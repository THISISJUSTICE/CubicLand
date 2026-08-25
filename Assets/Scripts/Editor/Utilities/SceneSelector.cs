using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Commar
{
    public class SceneSelector : EditorWindow
    {
        private GUIStyle _buttonStyle;
        private GUIStyle _numberStyle;
        private int _hoveredIndex = -1;
        private int _selectedIndex = -1;

        [MenuItem(EditorUtil.MAIN_MENU + "/Scene Selector")]
        public static void ShowWindow()
        {
            GetWindow<SceneSelector>("Scene Selector");
        }

        private void OnGUI()
        {
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.alignment = TextAnchor.MiddleLeft;
                _buttonStyle.padding = new RectOffset(30, 10, 5, 5);

                _numberStyle = new GUIStyle(GUI.skin.label);
                _numberStyle.alignment = TextAnchor.MiddleLeft;
                _numberStyle.margin = new RectOffset(5, 5, 5, 5);
            }

            GUILayout.Label("Select a Scene to Load", EditorStyles.boldLabel);

            ShowScenesInBuildSettings();
        }

        private void ShowScenesInBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            for (int i = 0; i < scenes.Length; i++)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
                if (SceneManager.GetActiveScene().name == sceneName)
                {
                    _selectedIndex = i;
                    break;
                }
            }

            for (int i = 0; i < scenes.Length; i++)
            {
                string scenePath = scenes[i].path;
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(sceneName), _buttonStyle);
                if (buttonRect.Contains(Event.current.mousePosition))
                {
                    _hoveredIndex = i;
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        LoadScene(scenePath);
                    }
                }

                if (i == _selectedIndex)
                    GUI.backgroundColor = _hoveredIndex == i ? new Color(1, 0, 1, 1) : Color.red;
                else
                    GUI.backgroundColor = _hoveredIndex == i ? Color.blue : Color.white;

                GUI.Box(buttonRect, GUIContent.none);

                GUI.Label(new Rect(buttonRect.x + 5, buttonRect.y, 20, buttonRect.height), (i + 1).ToString(), _numberStyle);
                GUI.Button(buttonRect, sceneName, _buttonStyle);

                if (Event.current.type == EventType.Repaint)
                {
                    GUI.backgroundColor = Color.white;
                }
            }
        }

        public static void LoadScene(string scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }
    }
}