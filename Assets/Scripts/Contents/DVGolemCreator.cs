using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DVGolemCreator : MonoBehaviour
{
    #region Variables
    // TODO: Addressable Load로 변경
    [SerializeField] private GameObject _baseCube;
    #endregion

    #region Unity Functions
    private void Start()
    {
        DVGolemInfo playerInfo = GetPlayerInfo();
        //GetMonsterInfo();
    }

    private void OnDestroy()
    {
        DVObjectManager.Instance?.DeleteObject(_baseCube);
        // TODO: Addressable Load 해제
    }
    #endregion

    #region Utils
    private DVGolemInfo GetPlayerInfo()
    {
        // TODO: 서버에서 받아온 데이터를 반환
        // TODO: 임시로 로컬 데이터를 반환
        DVStatus status = new DVStatus(100, 0, 0);

        DVGolemInfo golemInfo = new DVGolemInfo(status);
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
        GameObject core = DVObjectManager.Instance.InstanitateObject(_baseCube, instMat: true);
        core.name = golemName;
        core.transform.Reset();
        pos.y += (float)(golemInfo.GetDirectionSize(DVEnums.Direction3D.DOWN) - 1) * DVConfigs.CUBE_BASE_LENGHT;
        core.transform.position = pos;

        DVCubeInfo cubeInfo = new DVCubeInfo(golemInfo.Status, true, Vector3Int.zero);
        DVGolemCube golemCube = core.GetComponent<DVGolemCube>();
        DVObjectManager.Instance.AddComponent<Rigidbody>(core);
        DVGolemCore golemCore = DVObjectManager.Instance.AddComponent<DVGolemCore>(core);

        golemCube.SetGolemCubeInfo(cubeInfo, null, golemCore);
        golemCore.SetGolemInfo(golemInfo);

        MakeChildCube(ref golemInfo, cubeInfo, golemCube, golemCore);
        golemCore.Init();

        return core;
    }

    private void MakeChildCube(ref DVGolemInfo golemInfo, DVCubeInfo pCubeInfo, DVGolemCube pGolemCube, DVGolemCore core)
    {
        if (!golemInfo.ChildMap.ContainsKey(pCubeInfo.ShapePosition) || golemInfo.ChildMap[pCubeInfo.ShapePosition].Count <= 0)
            return;

        DVStatus status = new DVStatus();
        status.SetChildValue(golemInfo.Status);

        List<DVGolemCube> childs = new List<DVGolemCube>();

        foreach (var shapePos in golemInfo.ChildMap[pCubeInfo.ShapePosition]) {
            GameObject cube = DVObjectManager.Instance.InstanitateObject(_baseCube, instMat: true);
            cube.name = $"Child_{shapePos.x}_{shapePos.y}_{shapePos.z}";
            cube.transform.Reset();
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
