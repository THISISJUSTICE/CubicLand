using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace CustomTIJI.CubicLand
{
    public class CubeCreator : SingletonMonoBehaviour<CubeCreator>, IIntroInitializer
    {
        private GameObject _golemCube;
        private GameObject _golemCore;
        private GameObject _obstacleCube;
        private GameObject _skillCube;
        private GameObject _skillGolemCore;
        private GameObject _skillGolemCube;

        private bool _isLoaded = false;
        public bool IsLoaded => _isLoaded;

        public async UniTask Initialize()
        {
            _golemCube = await AddresableManager.Instance.LoadAsset<GameObject>("GolemCube");
            _golemCore = await AddresableManager.Instance.LoadAsset<GameObject>("GolemCore");
            _obstacleCube = await AddresableManager.Instance.LoadAsset<GameObject>("ObstacleCube");
            _skillCube = await AddresableManager.Instance.LoadAsset<GameObject>("SkillCube");
            _skillGolemCore = await AddresableManager.Instance.LoadAsset<GameObject>("SkillGolemCore");
            _skillGolemCube = await AddresableManager.Instance.LoadAsset<GameObject>("SkillGolemCube");

            _isLoaded = true;
        }

        public GolemCore CreatePlayer(GolemInfo golemInfo)
        {
            // TODO: 자식 데이터는 로컬 데이터 베이스에서 파싱

            GameObjectInstance player = CreateGolem(golemInfo, "Player", Vector3.one);
            player.AddComponent<PlayerController>();

            return player.GetComponent<GolemCore>();
        }

        public GolemCore CreateMonster(GolemInfo golemInfo, int addRandomCubeCount = 0)
        {
            for (int i = 0; i < addRandomCubeCount; i++)
                AddRandomGolemCube(golemInfo);

            GameObjectInstance monster = CreateGolem(golemInfo, "Monster", new Vector3(5f, Configs.CubeBottomHeight, 5f));
            monster.AddComponent<GolemController>();

            return monster.GetComponent<GolemCore>();
        }

        public ObstacleCube CreateObstacleCube(Status status)
        {
            CubeInfo cubeInfo = new CubeInfo(status, false, Vector3Int.zero);
            GameObjectInstance cube = ObjectManager.Instance.InstanitateGameObject(_obstacleCube, useInstanceMaterial: true);
            ObstacleCube obstacle = cube.GetComponent<ObstacleCube>();
            obstacle.SetCubeInfo(cubeInfo);

            return obstacle;
        }

        public void AddRandomGolemCube(GolemInfo golemInfo)
        {
            int rand = Random.Range(0, golemInfo.Shape.Count);
            Vector3Int parent = Vector3Int.zero;
            int index = 0;

            foreach (var shape in golemInfo.Shape)
            {
                if (index++ == rand)
                    parent = shape;
            }

            rand = Random.Range(0, Utils.Direction3DLength);
            for (int i = 0; i < Utils.Direction3DLength; i++)
            {
                int dirIndex = Mathf.RoundToInt(Mathf.Repeat(i + rand, Utils.Direction3DLength));
                if (golemInfo.AddCube(parent, (Enums.Direction3D)dirIndex))
                {
                    return;
                }
            }

            AddRandomGolemCube(golemInfo);
        }

        // TODO: Summon 함수는 생성 시 애니메이션 효과 추가
        public SkillGolemCore SummonSkillGolemCore(GolemController owner, GolemInfo golemInfo, string skillName, Vector3 pos)
        {
            GameObjectInstance core = ObjectManager.Instance.InstanitateGameObject(_skillGolemCore, useInstanceMaterial: true);
            core.Name = skillName;
            core.Position = pos;
            core.Rotation = owner.PlayerViewRotation;

            CubeInfo cubeInfo = new CubeInfo(golemInfo.Status, true, Vector3Int.zero);
            SkillGolemCore golemCore = core.GetComponent<SkillGolemCore>();

            SkillGolemCube childCube = core.GetComponent<SkillGolemCube>();
            childCube.SetGolemCubeInfo(cubeInfo, null, golemCore);

            golemCore.SetInit(golemInfo, owner);
            return golemCore;
        }

        public SkillGolemCube[] SummonSkillGolemChilds(SkillGolemCore golemCore, Vector3Int[] childs)
        {
            SkillGolemCube[] childCubes = new SkillGolemCube[childs.Length];

            for (int i = 0; i < childs.Length; i++)
            {
                GameObjectInstance cube = ObjectManager.Instance.InstanitateGameObject(_skillGolemCube, useInstanceMaterial: true);
                Vector3Int parentPos = golemCore.GolemInfo.ParentMap[childs[i]];
                SkillGolemCube parentCube = golemCore.FindCube(parentPos);
                Status status = parentCube.CubeInfo.Status.GetChildStatus();
                CubeInfo cubeInfo = new CubeInfo(status, false, childs[i]);

                SkillGolemCube childCube = cube.GetComponent<SkillGolemCube>();
                childCube.SetGolemCubeInfo(cubeInfo, parentCube, golemCore);
                SetChlidObject(childCube, parentCube);
                parentCube.AddGolemChild(childCube);
                golemCore.AddChild(childCube);

                childCubes[i] = childCube;
            }

            return childCubes;
        }

        private GameObjectInstance CreateGolem(GolemInfo golemInfo, string golemName, Vector3 pos)
        {
            GameObjectInstance core = ObjectManager.Instance.InstanitateGameObject(_golemCore, transform, useInstanceMaterial: true);
            core.Name = golemName;
            pos.y += (float)golemInfo.GetDirectionSize(Enums.Direction3D.Down) * Configs.CUBE_BASE_LENGHT;
            core.Position = pos;

            CubeInfo cubeInfo = new CubeInfo(golemInfo.Status, true, Vector3Int.zero);
            GolemCube golemCube = core.GetComponent<GolemCube>();
            GolemCore golemCore = core.GetComponent<GolemCore>();

            golemCube.SetGolemCubeInfo(cubeInfo, null, golemCore);
            MakeChildCube(ref golemInfo, cubeInfo, golemCube, golemCore);
            golemCore.SetInit(golemInfo);

            return core;
        }

        // TODO: 기록된 자식 정보가 있는 경우 기록된 것을 기준으로 적용하도록 수정
        private void MakeChildCube(ref GolemInfo golemInfo, CubeInfo pCubeInfo, GolemCube pGolemCube, GolemCore core)
        {
            if (!golemInfo.ChildMap.ContainsKey(pCubeInfo.ShapePosition) || golemInfo.ChildMap[pCubeInfo.ShapePosition].Count <= 0)
                return;

            Status status = pCubeInfo.Status.GetChildStatus();

            foreach (var shapePos in golemInfo.ChildMap[pCubeInfo.ShapePosition])
            {
                GameObjectInstance cube = ObjectManager.Instance.InstanitateGameObject(_golemCube, useInstanceMaterial: true);

                CubeInfo cubeInfo = new CubeInfo(status, false, shapePos);
                GolemCube golemCube = cube.GetComponent<GolemCube>();
                golemCube.SetGolemCubeInfo(cubeInfo, pGolemCube, core);
                SetChlidObject(golemCube, pGolemCube);

                pGolemCube.AddGolemChild(golemCube);

                MakeChildCube(ref golemInfo, cubeInfo, golemCube, core);
            }
        }

        private void SetChlidObject<T>(T childCube, T parentCube) where T : CubeBase
        {
            Vector3Int shapePos = childCube.CubeInfo.ShapePosition;

            childCube.name = $"Child_{shapePos.x}_{shapePos.y}_{shapePos.z}";
            childCube.transform.SetParent(parentCube.transform);
            childCube.transform.localPosition = (Vector3)(shapePos - parentCube.CubeInfo.ShapePosition) * Configs.CUBE_BASE_LENGHT;
        }
    }
}