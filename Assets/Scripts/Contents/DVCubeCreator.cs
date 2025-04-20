using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class DVCubeCreator : SingletonMonoBehaviour<DVCubeCreator>, IIntroInitializable
{
    #region Variables
    private GameObject _golemCube;
    private GameObject _golemCore;
    private GameObject _obstacleCube;

    private Transform _obstacleParent;
    private Transform _monsterParent;

    private Dictionary<string, UnityEngine.Object> _cubes;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        _obstacleParent = new GameObject("Obstacle Parent").transform;
        _obstacleParent.SetParent(transform);
        _monsterParent = new GameObject("Monster Parent").transform;
        _monsterParent.SetParent(transform);
    }
    #endregion

    #region Events
    public void OnIntroInit()
    {
        DVResourceManager.Instance.TryGetAssetDictionary(DVAssets.AssetType.Cube, out _cubes);

        _golemCube = (GameObject)_cubes["GolemCube"];
        _golemCore = (GameObject)_cubes["GolemCore"];
        _obstacleCube = (GameObject)_cubes["ObstacleCube"];
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) {
        List<GameObject> cubes = new List<GameObject>();

        if (_obstacleParent != null)
        {
            foreach (Transform child in _obstacleParent.transform)
            {
                child.gameObject.SetActive(false);
                cubes.Add(child.gameObject);
            }
        }

        if (_monsterParent != null)
        {
            foreach (Transform child in _monsterParent.GetComponentsInChildren<Transform>())
            {
                if (child == _monsterParent)
                    continue;

                child.gameObject.SetActive(false);
                cubes.Add(child.gameObject);
            }
        }

        for (int i = 0; i < cubes.Count; i++)
        {
            DVObjectManager.Instance.DestroyObject(cubes[i]);
        }

    }
    #endregion

    #region Public Functions
    public DVGolemCore CreatePlayer(DVGolemInfo golemInfo) {
        // TODO: 자식 데이터는 로컬 데이터 베이스에서 파싱

        GameObject player = CreateGolem(golemInfo, "Player", Vector3.one);
        DVObjectManager.Instance.AddComponent<DVPlayerController>(player);

        player.transform.SetParent(transform);

        return player.GetComponent<DVGolemCore>();
    }

    public DVGolemCore CreateMonster(DVGolemInfo golemInfo, int addRandomCubeCount = 0)
    {
        for (int i = 0; i < addRandomCubeCount; i++)
            AddRandomGolemCube(golemInfo);

        GameObject monster = CreateGolem(golemInfo, "Monster", new Vector3(5f, DVConfigs.CubeBottomHeight, 5f));
        DVObjectManager.Instance.AddComponent<DVGolemController>(monster);
        monster.transform.SetParent(_monsterParent);

        return monster.GetComponent<DVGolemCore>();
    }

    public DVObstacleCube CreateObstacleCube(DVStatus status) {
        DVCubeInfo cubeInfo = new DVCubeInfo(status, false, Vector3Int.zero);
        GameObject cube = DVObjectManager.Instance.InstanitateObject(_obstacleCube, instMat: true);
        DVObstacleCube obstacle = cube.GetComponent<DVObstacleCube>();
        obstacle.SetInit(cubeInfo);        
        obstacle.transform.SetParent(_obstacleParent.transform);

        return obstacle;
    }

    public void AddRandomGolemCube(DVGolemInfo golemInfo)
    {
        int rand = Random.Range(0, golemInfo.Shape.Count);
        Vector3Int parent = Vector3Int.zero;
        int index = 0;

        foreach (var shape in golemInfo.Shape)
        {
            if (index++ == rand)
                parent = shape;
        }

        rand = Random.Range(0, DVUtil.Direction3DLength);
        for (int i = 0; i < DVUtil.Direction3DLength; i++)
        {
            int dirIndex = Mathf.RoundToInt(Mathf.Repeat(i + rand, DVUtil.Direction3DLength));
            if (golemInfo.AddCube(parent, (DVEnums.Direction3D)dirIndex))
            {
                return;
            }
        }

        AddRandomGolemCube(golemInfo);
    }
    #endregion

    #region Utils
    private GameObject CreateGolem(DVGolemInfo golemInfo, string golemName, Vector3 pos) {
        GameObject core = DVObjectManager.Instance.InstanitateObject(_golemCore, instMat: true);
        core.name = golemName;
        pos.y += (float)(golemInfo.GetDirectionSize(DVEnums.Direction3D.DOWN) - 1) * DVConfigs.CUBE_BASE_LENGHT;
        core.transform.position = pos;

        DVCubeInfo cubeInfo = new DVCubeInfo(golemInfo.Status, true, Vector3Int.zero);
        DVGolemCube golemCube = core.GetComponent<DVGolemCube>();
        DVGolemCore golemCore = core.GetComponent<DVGolemCore>();

        golemCube.SetGolemCubeInfo(cubeInfo, null, golemCore);
        golemCore.SetGolemInfo(golemInfo);

        MakeChildCube(ref golemInfo, cubeInfo, golemCube, golemCore);
        golemCore.SetInit();
        golemCore.SetupChilds();

        return core;
    }

    // TODO: 기록된 자식 정보가 있는 경우 기록된 것을 기준으로 적용하도록 수정
    private void MakeChildCube(ref DVGolemInfo golemInfo, DVCubeInfo pCubeInfo, DVGolemCube pGolemCube, DVGolemCore core)
    {
        if (!golemInfo.ChildMap.ContainsKey(pCubeInfo.ShapePosition) || golemInfo.ChildMap[pCubeInfo.ShapePosition].Count <= 0)
            return;

        DVStatus status = new DVStatus();
        status.SetChildValue(pCubeInfo.Status);

        List<DVGolemCube> childs = new List<DVGolemCube>();

        foreach (var shapePos in golemInfo.ChildMap[pCubeInfo.ShapePosition]) {
            GameObject cube = DVObjectManager.Instance.InstanitateObject(_golemCube, instMat: true);
            cube.name = $"Child_{shapePos.x}_{shapePos.y}_{shapePos.z}";
            cube.transform.SetParent(pGolemCube.transform);
            cube.transform.localPosition = (Vector3)(shapePos - pGolemCube.CubeInfo.ShapePosition) * DVConfigs.CUBE_BASE_LENGHT;

            DVCubeInfo cubeInfo = new DVCubeInfo(status, false, shapePos);
            DVGolemCube golemCube = cube.GetComponent<DVGolemCube>();
            golemCube.SetGolemCubeInfo(cubeInfo, pGolemCube, core);

            childs.Add(golemCube);

            MakeChildCube(ref golemInfo, cubeInfo, golemCube, core);
        }

        pGolemCube.SetGolemChild(childs.ToArray());
    }
    #endregion
}
