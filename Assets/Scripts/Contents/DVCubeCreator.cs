using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class DVCubeCreator : SingletonMonoBehaviour<DVCubeCreator>
{
    private GameObject _golemCube;
    private GameObject _golemCore;
    private GameObject _obstacleCube;
    private GameObject _skillCube;
    private GameObject _skillGolemCore;
    private GameObject _skillGolemCube;

    private Transform _obstacleParent;
    private Transform _monsterParent;

    private Dictionary<string, UnityEngine.Object> _cubes;

    protected override async void Awake()
    {
        base.Awake();

        Init();

        await UniTask.WaitUntil(() => DVResourceManager.Instance.IsLoaded);
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

    private void Init()
    {
        DVResourceManager.Instance.TryGetAssetDictionary("Cubes", out _cubes);

        _golemCube = (GameObject)_cubes["GolemCube"];
        _golemCore = (GameObject)_cubes["GolemCore"];
        _obstacleCube = (GameObject)_cubes["ObstacleCube"];
        _skillCube = (GameObject)_cubes["SkillCube"];
        _skillGolemCore = (GameObject)_cubes["SkillGolemCore"];
        _skillGolemCube = (GameObject)_cubes["SkillGolemCube"];
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
        obstacle.SetCubeInfo(cubeInfo);        
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

    // TODO: Summon 함수는 생성 시 애니메이션 효과 추가
    public DVSkillGolemCore SummonSkillGolemCore(DVGolemController owner, DVGolemInfo golemInfo, string skillName, Vector3 pos)
    {
        GameObject core = DVObjectManager.Instance.InstanitateObject(_skillGolemCore, instMat: true);
        core.name = skillName;
        core.transform.position = pos;
        core.transform.rotation = owner.PlayerViewRotation;

        DVCubeInfo cubeInfo = new DVCubeInfo(golemInfo.Status, true, Vector3Int.zero);
        DVSkillGolemCore golemCore = core.GetComponent<DVSkillGolemCore>();

        DVSkillGolemCube childCube = core.GetComponent<DVSkillGolemCube>();
        childCube.SetGolemCubeInfo(cubeInfo, null, golemCore);

        golemCore.SetInit(golemInfo, owner);
        return golemCore;
    }

    public DVSkillGolemCube[] SummonSkillGolemChilds(DVSkillGolemCore golemCore, Vector3Int[] childs)
    {
        DVSkillGolemCube[] childCubes = new DVSkillGolemCube[childs.Length];
        
        for (int i = 0; i < childs.Length; i++)
        {
            GameObject cube = DVObjectManager.Instance.InstanitateObject(_skillGolemCube, instMat: true);
            Vector3Int parentPos = golemCore.GolemInfo.ParentMap[childs[i]];
            DVSkillGolemCube parentCube = golemCore.FindCube(parentPos);
            DVStatus status = parentCube.CubeInfo.Status.GetChildStatus();
            DVCubeInfo cubeInfo = new DVCubeInfo(status, false, childs[i]);

            DVSkillGolemCube childCube = cube.GetComponent<DVSkillGolemCube>();
            childCube.SetGolemCubeInfo(cubeInfo, parentCube, golemCore);
            SetChlidObject(childCube, parentCube);
            parentCube.AddGolemChild(childCube);
            golemCore.AddChild(childCube);

            childCubes[i] = childCube;            
        }

        return childCubes;
    }

    private GameObject CreateGolem(DVGolemInfo golemInfo, string golemName, Vector3 pos) {
        GameObject core = DVObjectManager.Instance.InstanitateObject(_golemCore, instMat: true);
        core.name = golemName;
        pos.y += (float)golemInfo.GetDirectionSize(DVEnums.Direction3D.Down) * DVConfigs.CUBE_BASE_LENGHT;
        core.transform.position = pos;

        DVCubeInfo cubeInfo = new DVCubeInfo(golemInfo.Status, true, Vector3Int.zero);
        DVGolemCube golemCube = core.GetComponent<DVGolemCube>();
        DVGolemCore golemCore = core.GetComponent<DVGolemCore>();

        golemCube.SetGolemCubeInfo(cubeInfo, null, golemCore);
        MakeChildCube(ref golemInfo, cubeInfo, golemCube, golemCore);
        golemCore.SetInit(golemInfo);

        return core;
    }

    // TODO: 기록된 자식 정보가 있는 경우 기록된 것을 기준으로 적용하도록 수정
    private void MakeChildCube(ref DVGolemInfo golemInfo, DVCubeInfo pCubeInfo, DVGolemCube pGolemCube, DVGolemCore core)
    {
        if (!golemInfo.ChildMap.ContainsKey(pCubeInfo.ShapePosition) || golemInfo.ChildMap[pCubeInfo.ShapePosition].Count <= 0)
            return;

        DVStatus status = pCubeInfo.Status.GetChildStatus();

        foreach (var shapePos in golemInfo.ChildMap[pCubeInfo.ShapePosition]) {
            GameObject cube = DVObjectManager.Instance.InstanitateObject(_golemCube, instMat: true);

            DVCubeInfo cubeInfo = new DVCubeInfo(status, false, shapePos);
            DVGolemCube golemCube = cube.GetComponent<DVGolemCube>();
            golemCube.SetGolemCubeInfo(cubeInfo, pGolemCube, core);
            SetChlidObject(golemCube, pGolemCube);

            pGolemCube.AddGolemChild(golemCube);

            MakeChildCube(ref golemInfo, cubeInfo, golemCube, core);
        }
    }

    private void SetChlidObject<T>(T childCube, T parentCube) where T : DVCubeBase
    {
        Vector3Int shapePos = childCube.CubeInfo.ShapePosition;

        childCube.name = $"Child_{shapePos.x}_{shapePos.y}_{shapePos.z}";
        childCube.transform.SetParent(parentCube.transform);
        childCube.transform.localPosition = (Vector3)(shapePos - parentCube.CubeInfo.ShapePosition) * DVConfigs.CUBE_BASE_LENGHT;
    }
}
