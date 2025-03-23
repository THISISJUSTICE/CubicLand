using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// TODO: Creator 자체는 싱글톤으로 변경, 소환 정보 관련된 것만 맵 정보로서 저장
public class DVGolemCreator : MonoBehaviour
{
    #region Variables
    // TODO: Addressable Load로 변경
    [SerializeField] private GameObject _golemCube;
    [SerializeField] private GameObject _golemCore;
    [SerializeField] private GameObject _obstacleCube;

    private GameObject _obstacleParent;
    #endregion

    #region Unity Functions
    private void Start()
    {
        DVGolemInfo playerInfo = GetPlayerInfo();
        //GetMonsterInfo();

        const string parentName = "Obstacles";
        _obstacleParent = GameObject.Find(parentName);
        if (_obstacleParent == null)
        {
            _obstacleParent = new GameObject(parentName);
            _obstacleParent.transform.Reset();
        }

        StartCoroutine(TempFall());
    }

    private void OnDestroy()
    {
        DVObjectManager.Instance?.DeleteObject(_golemCube);
        // TODO: Addressable Load 해제
    }
    #endregion

    // TODO: 장애물 생성 방식
    private IEnumerator TempFall() {
        DVStatus status = new DVStatus(500, 5, 5);
        const float height = 30f;
        const float range = 30f;

        for (int i = 0; i < 10000; i++) {
            var obstacle = CreateObstacle(status);
            float x = Mathf.RoundToInt(Random.Range(-range, range));
            float z = Mathf.RoundToInt(Random.Range(-range, range));
            obstacle.transform.position = new Vector3(x, height, z);

            yield return DVHelper.In.YieldCache.GetWaitForSeconds(0.02f);
        }
    }

    #region Public Functions
    public DVObstacleCube CreateObstacle(DVStatus status) {
        DVCubeInfo cubeInfo = new DVCubeInfo(status, false, Vector3Int.zero);
        GameObject cube = DVObjectManager.Instance.InstanitateObject(_obstacleCube, instMat: true);
        DVObstacleCube obstacle = cube.GetComponent<DVObstacleCube>();
        obstacle.SetInit(cubeInfo);
        obstacle.transform.SetParent(_obstacleParent.transform);

        return obstacle;
    }
    #endregion

    #region Utils
    private DVGolemInfo GetPlayerInfo()
    {
        // TODO: 서버에서 받아온 데이터를 반환
        // TODO: 임시로 로컬 데이터를 반환
        DVStatus status = new DVStatus(1000, 50, 50);

        DVGolemInfo golemInfo = new DVGolemInfo(status, moveSpeedPoint:40);
        for (int i = 0; i < 10; i++)
            AddRandomGolemCube(golemInfo);

        GameObject player = CreateGolem(golemInfo, "Player", Vector3.one);
        DVObjectManager.Instance.AddComponent<DVPlayerController>(player);
        
        return golemInfo;
    }

    private DVGolemInfo GetMonsterInfo()
    {
        // TODO: 어드레서블 데이터를 통해 랜덤 구현 후 반환
        DVGolemInfo golemInfo = new DVGolemInfo();
        for (int i = 0; i < 30; i++)
            AddRandomGolemCube(golemInfo);

        GameObject monster = CreateGolem(golemInfo, "Monster", new Vector3(5f, DVConfigs.CubeBottomHeight, 5f));
        DVObjectManager.Instance.AddComponent<DVGolemController>(monster);

        return golemInfo;
    }

    private void AddRandomGolemCube(DVGolemInfo golemInfo) { 
        int rand = Random.Range(0, golemInfo.Shape.Count);
        Vector3Int parent = Vector3Int.zero;
        int index = 0;

        foreach (var shape in golemInfo.Shape) {
            if (index++ == rand) 
                parent = shape;
        }

        rand = Random.Range(0, DVUtil.Direction3DLength);
        for (int i = 0; i < DVUtil.Direction3DLength; i++) { 
            int dirIndex = Mathf.RoundToInt(Mathf.Repeat(i + rand, DVUtil.Direction3DLength));
            if (golemInfo.AddCube(parent, (DVEnums.Direction3D)dirIndex)) {
                return;
            }
        }

        AddRandomGolemCube(golemInfo);
    }

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
