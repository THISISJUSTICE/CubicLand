using UnityEditor;
using UnityEngine;

namespace Commar
{
    public abstract class TabEditorWindow : EditorWindow
    {
        protected abstract class EditorOption
        {
            public abstract string TabName { get; }

            public abstract void DrawOption();
        }

        private Vector2 _scrollPosition;
        private int _tabIndex;
        private string[] _tabNames;
        protected abstract EditorOption[] EditorOptions { get; }

        protected virtual void OnGUI()
        {
            _tabIndex = GUILayout.Toolbar(_tabIndex, _tabNames);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 30f));

            GUILayout.Space(20);
            EditorOptions[_tabIndex].DrawOption();

            EditorGUILayout.EndScrollView();
        }

        protected static void SetTabs(TabEditorWindow window)
        {
            window._tabNames = new string[window.EditorOptions.Length];

            for (int i = 0; i < window._tabNames.Length; i++)
            {
                window._tabNames[i] = window.EditorOptions[i].TabName;
            }
        }

        protected static bool IsUpper(char ch)
        {
            return ch >= 'A' && ch <= 'Z';
        }

        protected static bool IsNumber(char ch)
        {
            return ch >= '0' && ch <= '9';
        }
    }
}