using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DVMapEditor : EditorWindow
{
    #region Variables
    private const string PLANE_MATERIAL_PATH = "Assets/Resource/Materials/MapPlaneMaterial.mat";
    private const string BORDER_MATERIAL_PATH = "Assets/Resource/Materials/MapBorderMaterial.mat";
    private const float UNITY_PLANE_RATIO = 5f;

    private GameObject _plane;
    private GameObject[] _borders;

    private int _planeSize = 1;
    private int _borderHeight = 1;
    #endregion

    #region Editor Functions
    [MenuItem("Custom/Ingame Utils/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<DVMapEditor>("Map Editor");
    }

    private void OnEnable()
    {

    }

    private void OnGUI()
    {
        _plane = EditorGUILayout.ObjectField("Plane", _plane, typeof(GameObject), false) as GameObject;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Plane Size");
        _planeSize = Mathf.RoundToInt(Mathf.Abs(EditorGUILayout.IntField(_planeSize)));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Border Height");
        _borderHeight = Mathf.RoundToInt(Mathf.Abs(EditorGUILayout.IntField(_borderHeight)));
        GUILayout.EndHorizontal();

        GUILayout.Space(30f);
        CreateDefaultPlaneButton();
        GUILayout.Space(5f);
        ResetPlaneSettingButton();
        GUILayout.Space(5f);
        CreateBorderButton();
    }
    #endregion

    #region GUI Functions
    private void CreateDefaultPlaneButton() {
        if (!GUILayout.Button("Create Default Plane"))
            return;

        // TODO: 생성 오브젝트들은 프리팹으로 전환하기
        _plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        SetPlaneDefaultSetting(_plane);
    }

    private void ResetPlaneSettingButton() {
        if (!GUILayout.Button("Reset Plane"))
            return;

        if (!CheckPlane())
            return;

        SetPlaneDefaultSetting(_plane);
    }

    private void CreateBorderButton() {
        if (!GUILayout.Button("Create Border"))
            return;

        if (!CheckPlane())
            return;

        // TODO: 생성 오브젝트들은 프리팹으로 전환하기

        _borders = new GameObject[4];
        for (int i = 0; i < _borders.Length; i++) {
            _borders[i] = Instantiate(_plane);
            _borders[i].name = $"Border{i}";

            MeshRenderer meshRen = _borders[i].GetComponent<MeshRenderer>();
            if (meshRen == null)
                meshRen = _borders[i].AddComponent<MeshRenderer>();

            meshRen.sharedMaterial = AssetDatabase.LoadAssetAtPath(BORDER_MATERIAL_PATH, typeof(Material)) as Material;
        }

        for (int i = 0; i < _borders.Length; i++)
            _borders[i].transform.SetParent(_plane.transform);

        Vector3 oneCubeSize = GetOneCubePlaneSize(_plane);
        Vector3 scale = _borders[0].transform.localScale;
        scale.z = oneCubeSize.z * (float)_borderHeight;

        for (int i = 0; i < _borders.Length; i++)
            _borders[i].transform.localScale = scale;

        _borders[0].transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        _borders[0].transform.localPosition = new Vector3(0f, UNITY_PLANE_RATIO * scale.z, -UNITY_PLANE_RATIO);

        _borders[1].transform.localEulerAngles = new Vector3(90f, 90f, 0f);
        _borders[1].transform.localPosition = new Vector3(-UNITY_PLANE_RATIO, UNITY_PLANE_RATIO * scale.z, 0f);

        _borders[2].transform.localEulerAngles = new Vector3(90f, -90f, 0f);
        _borders[2].transform.localPosition = new Vector3(UNITY_PLANE_RATIO, UNITY_PLANE_RATIO * scale.z, 0f);

        _borders[3].transform.localEulerAngles = new Vector3(90f, 180f, 0f);
        _borders[3].transform.localPosition = new Vector3(0f, UNITY_PLANE_RATIO * scale.z, UNITY_PLANE_RATIO);
    }
    #endregion

    #region Utils
    private bool CheckPlane() {
        if (_plane == null) {
            Debug.LogError($"Plane is Null!!!");
            return false;
        }

        return true;
    }

    private void SetPlaneDefaultSetting(GameObject plane) {
        plane.transform.Reset();
        DeleteChilds(plane);
        SetPlaneSize(plane, Vector2.one);

        plane.name = "Map";
        plane.tag = "Map";
        plane.layer = LayerMask.NameToLayer("Map");

        MeshRenderer meshRen = plane.GetComponent<MeshRenderer>();
        if (meshRen == null)
            meshRen = plane.AddComponent<MeshRenderer>();

        meshRen.sharedMaterial = AssetDatabase.LoadAssetAtPath(PLANE_MATERIAL_PATH, typeof(Material)) as Material;
        //meshRen.sharedMaterial.SetFloatArray("_Tiling", new List<float>() { _planeSize, _planeSize });

        Collider collider = plane.GetComponent<Collider>();
        if (collider != null)
            DestroyImmediate(collider);
    }

    private void SetPlaneSize(GameObject plane, Vector2 ratio)
    {
        Vector3 size = GetOneCubePlaneSize(plane);
        plane.transform.localScale = new Vector3(size.x * (float)_planeSize * ratio.x,
            size.y, size.z * (float)_planeSize * ratio.y);

    }

    private Vector3 GetOneCubePlaneSize(GameObject plane) {
        MeshFilter meshFilter = plane.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        Vector3 planeSize = mesh.bounds.size;

        return new Vector3(DVConfigs.CUBE_BASE_LENGHT / planeSize.x, 1f, DVConfigs.CUBE_BASE_LENGHT / planeSize.z);
    }

    private void DeleteChilds(GameObject go) {
        DeleteChilds(go.transform);
    }

    private void DeleteChilds(Transform tf)
    {
        while (tf.childCount > 0) { 
            DestroyImmediate(tf.GetChild(0).gameObject);
        }
    }
    #endregion
}
