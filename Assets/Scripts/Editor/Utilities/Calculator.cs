using UnityEngine;
using UnityEditor;
using System;

namespace Commar
{
    public class Calculator : EditorWindow
    {
        private abstract class BaseCalculator
        {
            protected Calculator _calculator;
            protected int _selectedTab = 0;
            protected string[] _tabLabels;


            public BaseCalculator(Calculator calculator)
            {
                _calculator = calculator;
            }

            public abstract void OnGUIUpdate();
        }

        private class Vector3Calculator : BaseCalculator
        {
            private Vector3[] _couples = new Vector3[2];

            public Vector3Calculator(Calculator calculator) : base(calculator)
            {
                _tabLabels = new string[] { "Display Couple Vector3s Info" };
            }

            public override void OnGUIUpdate()
            {
                _calculator.SelectTabTemplate(ref _selectedTab, ref _tabLabels, new Action[]
                {
                () => DisplayCoupleVector3sInfo()
                });
            }

            private void DisplayCoupleVector3sInfo()
            {
                _calculator.SetVector3s(ref _couples);

                string distance = $"Distance: {Vector3.Distance(_couples[0], _couples[1])}";
                string angle = $"Angle: {Vector3.Angle(_couples[0], _couples[1])}";
                string direction = $"Direction: {(_couples[1] - _couples[0]).normalized}";
                string cross = $"Cross: {Vector3.Cross(_couples[0], _couples[1])}";
                string magnitude = $"Magnitude: 1: {Vector3.Magnitude(_couples[0])}, 2: {Vector3.Magnitude(_couples[1])}";
                string dot = $"Dot: {Vector3.Dot(_couples[0], _couples[1])}";

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(distance);
                EditorGUILayout.LabelField(angle);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(direction);
                EditorGUILayout.LabelField(cross);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(magnitude);
                EditorGUILayout.LabelField(dot);
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
                if (GUILayout.Button("Log Infos"))
                {
                    Debug.Log($"{distance}, {angle}\n" +
                        $"{direction}, {cross}\n" +
                        $"{magnitude}, {dot}");
                }
            }
        }

        private class ResolutionCalculator : BaseCalculator
        {
            private Vector2Int _resolution;

            public ResolutionCalculator(Calculator calculator) : base(calculator)
            {
                _tabLabels = new string[] { "Display Resolution Info" };
            }

            public override void OnGUIUpdate()
            {
                _calculator.SelectTabTemplate(ref _selectedTab, ref _tabLabels, new Action[]
                {
                () => DisplayResolutionInfo()
                });
            }

            private void DisplayResolutionInfo()
            {
                _calculator.SetResolutions(ref _resolution);

                if (_resolution.x * _resolution.y <= 0)
                    return;

                GUILayout.Space(10);
                int gcd = Utils.GetGCD(_resolution.x, _resolution.y);

                string resolRate = $"Resolution Rate: {_resolution.x / gcd} x {_resolution.y / gcd}";
                EditorGUILayout.LabelField(resolRate);
            }
        }

        private BaseCalculator[] _calculators;

        private int _selectedTab = 0;
        private string[] _tabLabels = new string[] { "Vector3" };

        private Action[] _guiActions;

        [MenuItem("Commar/Calculator")]
        public static void ShowWindow()
        {
            GetWindow<Calculator>("Calculator");
        }

        private void OnEnable()
        {
            _calculators = new BaseCalculator[] {
            new Vector3Calculator(this),
            new ResolutionCalculator(this),
        };

            _guiActions = new Action[_calculators.Length];
            for (int i = 0; i < _guiActions.Length; i++)
            {
                _guiActions[i] = _calculators[i].OnGUIUpdate;
            }

            _tabLabels = new string[_calculators.Length];
            for (int i = 0; i < _tabLabels.Length; i++)
            {
                _tabLabels[i] = _calculators[i].GetType().Name.Replace("Calculator", "");
            }
        }

        private void OnGUI()
        {
            SelectTabTemplate(ref _selectedTab, ref _tabLabels, _guiActions);
        }

        public void SelectTabTemplate(ref int selectedTab, ref string[] tabLables, Action[] callbacks)
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabLables);
            GUILayout.Space(15);

            callbacks[selectedTab]();
        }

        public void SetVector3s(ref Vector3[] vec3s)
        {
            float labelSpace = 20f;
            float fieldSpace = 70f;

            for (int i = 0; i < vec3s.Length; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Vector3 {i + 1} :");
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += 80f;
                rect.width = 50f;
                EditorGUI.LabelField(rect, "x :");
                rect.x += labelSpace;
                vec3s[i].x = EditorGUI.FloatField(rect, vec3s[i].x);

                rect.x += fieldSpace;
                EditorGUI.LabelField(rect, "y :");
                rect.x += labelSpace;
                vec3s[i].y = EditorGUI.FloatField(rect, vec3s[i].y);

                rect.x += fieldSpace;
                EditorGUI.LabelField(rect, "z :");
                rect.x += labelSpace;
                vec3s[i].z = EditorGUI.FloatField(rect, vec3s[i].z);

                GUILayout.EndHorizontal();
                if (i < vec3s.Length - 1)
                    GUILayout.Space(5);
            }
        }

        public void SetResolutions(ref Vector2Int resolution)
        {
            float labelSpace = 40f;
            float fieldSpace = 70f;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Resolution :");
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.x += 100f;
            rect.width = 50f;
            EditorGUI.LabelField(rect, "width :");
            rect.x += labelSpace;
            resolution.x = EditorGUI.IntField(rect, resolution.x);

            rect.x += fieldSpace;
            EditorGUI.LabelField(rect, "height :");
            rect.x += labelSpace;
            resolution.y = EditorGUI.IntField(rect, resolution.y);
            GUILayout.EndHorizontal();
        }
    }
}