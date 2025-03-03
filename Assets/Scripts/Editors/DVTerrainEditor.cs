using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class DVTerrainEditor : EditorWindow
{
    #region Variables
    private int BorderRange { get => 3; }
    private float MaxRaiseHeight { get => 0.5f; }

    private Terrain _terrain;

    private int _heightCount = 0;

    private int _maxRandCount = 0;

    private HashSet<int> _noDuplRandoms = new HashSet<int>();
    #endregion

    #region Editor Functions
    [MenuItem("Custom/Ingame Utils/Terrain Editor")]
    public static void ShowWindow()
    {
        GetWindow<DVTerrainEditor>("Terrain Editor");
    }

    private void OnEnable()
    {
        
    }

    private void OnGUI()
    {
        _terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", _terrain, typeof(Terrain), true);
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Height");
        Rect rect = GUILayoutUtility.GetLastRect();
        rect.x += 50f;
        rect.width = 50f;
        _heightCount = EditorGUI.IntField(rect, _heightCount);
        rect.x += 100f;
        rect.width = 120f;
        EditorGUI.LabelField(rect, "Max Random Count");
        rect.x += rect.width;
        rect.width = 50f;
        _maxRandCount = EditorGUI.IntField(rect, _maxRandCount);
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        ResetTerrain();
        GUILayout.Space(15);
        SetCopyTerrain();
        GUILayout.Space(15);
        SetTerrainFlattening();
        GUILayout.Space(15);
        SetTerrainBorder();
        GUILayout.Space(15);
        RaiseTerrainRandomly();
    }
    #endregion

    #region GUI Functions
    private void SetCopyTerrain() {
        if (!GUILayout.Button("Copy Terrain"))
            return;

        if (!CheckTerrain())
            return;

        Terrain terrain = CloneTerrain(_terrain);
        _terrain.gameObject.SetActive(false);
        _terrain = terrain;
    }

    private void ResetTerrain() {
        if (!GUILayout.Button("Reset Terrain"))
            return;

        if (!CheckTerrain())
            return;

        TerrainData tData = _terrain.terrainData;
        int resol = tData.heightmapResolution;
        tData.size = new Vector3(resol * DVConfigs.CUBE_BASE_LENGHT, DVConfigs.CUBE_BASE_LENGHT, resol * DVConfigs.CUBE_BASE_LENGHT);
        float[,] heights = tData.GetHeights(0, 0, resol, resol);

        for (int z = 0; z < resol; z++)
        {
            for (int x = 0; x < resol; x++)
            {
                heights[x, z] = 0f;
            }
        }

        tData.SetHeights(0, 0, heights);
    }

    private void SetTerrainFlattening() {
        if (!GUILayout.Button("Terrain Flattening"))
            return;

        if (!CheckTerrain())
            return;

        TerrainData tData = _terrain.terrainData;
        int resol = tData.heightmapResolution;
        float[,] heights = tData.GetHeights(0, 0, resol, resol);

        for (int z = 0; z < resol; z++)
        {
            for (int x = 0; x < resol; x++)
            {
                heights[x, z] = 0f;
            }
        }

        tData.SetHeights(0, 0, heights);
    }

    private void SetTerrainBorder() {
        if (!GUILayout.Button("Terrain Border"))
            return;

        if (!CheckTerrain())
            return;

        TerrainData tData = _terrain.terrainData;
        int resol = tData.heightmapResolution;
        tData.size = new Vector3(resol * DVConfigs.CUBE_BASE_LENGHT, (float)_heightCount * DVConfigs.CUBE_BASE_LENGHT, resol * DVConfigs.CUBE_BASE_LENGHT);
        float[,] heights = tData.GetHeights(0, 0, resol, resol);

        for (int z = 0; z < resol; z++)
        {
            for (int i = 0; i < BorderRange; i++)
            {
                heights[i, z] = 1f;
                heights[resol - i - 1, z] = 1f;
            }
        }
        for (int x = 0; x < resol; x++)
        {
            for (int i = 0; i < BorderRange; i++)
            {
                heights[x, i] = 1f;
                heights[x, resol - i - 1] = 1f;
            }
        }

        tData.SetHeights(0, 0, heights);
    }

    private void RaiseTerrainRandomly() {
        if (!GUILayout.Button("Terrain Random Raise"))
            return;

        if (!CheckTerrain())
            return;

        TerrainData tData = _terrain.terrainData;
        int resol = tData.heightmapResolution;
        int terrainRange = resol - BorderRange * 2;

        Random.InitState(System.DateTime.Now.Millisecond);

        if(_maxRandCount > resol / 2)
            _maxRandCount = resol / 2;

        _noDuplRandoms.Clear();
        while (_noDuplRandoms.Count < _maxRandCount) {
            _noDuplRandoms.Add(Random.Range(0, terrainRange * terrainRange));
        }

        foreach (int rand in _noDuplRandoms) {
            int x = rand / terrainRange + BorderRange;
            int z = rand % terrainRange + BorderRange;
        }
    }
    #endregion

    #region Utils
    private bool CheckTerrain() { 
        if(_terrain == null)
        {
            Debug.LogError($"Terrain is not assigned!!!");
            return false;
        }

        return true;
    }

    private Terrain CloneTerrain(Terrain terrain)
    {
        TerrainData cloneData = new TerrainData();
        int resol = terrain.terrainData.heightmapResolution;
        cloneData.heightmapResolution = terrain.terrainData.heightmapResolution;
        cloneData.size = terrain.terrainData.size;
        cloneData.SetHeights(0, 0, terrain.terrainData.GetHeights(0, 0, resol, resol));

        TerrainLayer[] cloneLayer = new TerrainLayer[terrain.terrainData.terrainLayers.Length];
        for (int i = 0; i < cloneLayer.Length; i++)
        {
            TerrainLayer layer = terrain.terrainData.terrainLayers[i];
            cloneLayer[i] = new TerrainLayer()
            {
                diffuseTexture = layer.diffuseTexture,
                normalMapTexture = layer.normalMapTexture,
                maskMapTexture = layer.maskMapTexture,
                tileSize = layer.tileSize,
                tileOffset = layer.tileOffset,
                specular = layer.specular,
                metallic = layer.metallic,
                smoothness = layer.smoothness
            };
        }
        cloneData.terrainLayers = cloneLayer;

        int alphaMapWidth = terrain.terrainData.alphamapWidth;
        int alphaMapHeight = terrain.terrainData.alphamapHeight;
        float[,,] cloneAlpha = terrain.terrainData.GetAlphamaps(0, 0, alphaMapWidth, alphaMapHeight);
        cloneData.alphamapResolution = terrain.terrainData.alphamapResolution;
        cloneData.SetAlphamaps(0, 0, cloneAlpha);

        GameObject clone = Instantiate(terrain.gameObject);
        clone.transform.SetParent(terrain.transform.parent);
        clone.name = $"{terrain.name}_clone";
        clone.transform.position = terrain.transform.position;
        clone.transform.rotation = terrain.transform.rotation;
        clone.transform.localScale = terrain.transform.localScale;
        clone.SetActive(true);

        Terrain cloneTerrain = clone.GetComponent<Terrain>();
        cloneTerrain.terrainData = cloneData;
        return cloneTerrain;
    }
    #endregion
}
