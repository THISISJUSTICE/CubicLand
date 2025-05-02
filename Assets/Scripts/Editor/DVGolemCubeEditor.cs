using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DVGolemCube))]
public class DVGolemCubeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
