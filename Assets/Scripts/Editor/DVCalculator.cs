using UnityEngine;
using UnityEditor;
using System;

public class DVCalculator : EditorWindow
{
    #region Types
    private class Vector3Calculator {
        private DVCalculator _calculator;

        private int _selectedTab = 0;
        private string[] _tabLabels = new string[] { "Display Couple Vector3s Info" };

        private Vector3[] _couples = new Vector3[2];

        public Vector3Calculator(DVCalculator calculator)
        {
            _calculator = calculator;
        }

        public void OnGUIUpdate() {
            _calculator.SelectTabTemplate(ref _selectedTab, ref _tabLabels, new Action[]
            {
                () => DisplayCoupleVector3sInfo()
            });
        }

        private void DisplayCoupleVector3sInfo() { 
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
            if (GUILayout.Button("Log Infos")) {
                Debug.Log($"{distance}, {angle}\n" +
                    $"{direction}, {cross}\n" +
                    $"{magnitude}, {dot}");
            }
        }
    }
    #endregion

    #region Variables
    private Vector3Calculator _vec3Cal;

    #region GUI Variables
    private int _selectedTab = 0;
    private string[] _tabLabels = new string[] { "Vector3" }; 

    #endregion
    #endregion

    #region Editor Functions
    [MenuItem("Custom/Editor Utils/Calculator")]
    public static void ShowWindow()
    {
        GetWindow<DVCalculator>("Calculator");
    }

    private void OnEnable()
    {
        _vec3Cal = new Vector3Calculator(this);
    }

    private void OnGUI()
    {
        SelectTabTemplate(ref _selectedTab, ref _tabLabels, new Action[] {
            () => _vec3Cal.OnGUIUpdate(),
        });
    }
    #endregion

    #region GUI Functions

    #endregion

    #region Utils
    public void SelectTabTemplate(ref int selectedTab, ref string[] tabLables, Action[] callbacks)
    {
        selectedTab = GUILayout.Toolbar(selectedTab, tabLables);
        GUILayout.Space(15);

        callbacks[selectedTab]();
    }

    public void SetVector3s(ref Vector3[] vec3s) {
        for (int i = 0; i < vec3s.Length; i++) {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Vector3  {i+1} :");
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.x += 100f;
            rect.width = 50f;
            EditorGUI.LabelField(rect, "x :");
            rect.x += 30f;
            vec3s[i].x = EditorGUI.FloatField(rect, vec3s[i].x);

            rect.x += 60f;
            EditorGUI.LabelField(rect, "y :");
            rect.x += 30f;
            vec3s[i].y = EditorGUI.FloatField(rect, vec3s[i].y);

            rect.x += 60f;
            EditorGUI.LabelField(rect, "z :");
            rect.x += 30f;
            vec3s[i].z = EditorGUI.FloatField(rect, vec3s[i].z);

            GUILayout.EndHorizontal();
            if (i < vec3s.Length - 1)
                GUILayout.Space(5);
        }
    }
    #endregion
}
