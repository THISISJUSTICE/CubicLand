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

        private Transform _obstacleParent;
        private Transform _monsterParent;

        private Dictionary<string, UnityEngine.Object> _cubes;

        private bool _isLoaded = false;
        public bool IsLoaded => _isLoaded;

        protected override void Awake()
        {
            base.Awake();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            _obstacleParent = new GameObject("Obstacle Parent").transform;
            _obstacleParent.SetParent(transform);
            _monsterParent = new GameObject("Monster Parent").transform;
            _monsterParent.SetParent(transform);
        }

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

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
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
                ObjectManager.Instance.DestroyObject(cubes[i]);
            }

        }

        public GolemCore CreatePlayer(GolemInfo golemInfo)
        {
            // TODO: 자식 데이터는 로컬 데이터 베이스에서 파싱

            GameObject player = CreateGolem(golemInfo, "Player", Vector3.one);
            ObjectManager.Instance.AddComponent<PlayerController>(player);

            player.transform.SetParent(transform);

            return player.GetComponent<GolemCore>();
        }

        public GolemCore CreateMonster(GolemInfo golemInfo, int addRandomCubeCount = 0)
        {
            for (int i = 0; i < addRandomCubeCount; i++)
                AddRandomGolemCube(golemInfo);

            GameObject monster = CreateGolem(golemInfo, "Monster", new Vector3(5f, Configs.CubeBottomHeight, 5f));
            ObjectManager.Instance.AddComponent<GolemController>(monster);
            monster.transform.SetParent(_monsterParent);

            return monster.GetComponent<GolemCore>();
        }

        public ObstacleCube CreateObstacleCube(Status status)
        {
            CubeInfo cubeInfo = new CubeInfo(status, false, Vector3Int.zero);
            GameObject cube = ObjectManager.Instance.InstanitateObject(_obstacleCube, instMat: true);
            ObstacleCube obstacle = cube.GetComponent<ObstacleCube>();
            obstacle.SetCubeInfo(cubeInfo);
            obstacle.transform.SetParent(_obstacleParent.transform);

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
            GameObject core = ObjectManager.Instance.InstanitateObject(_skillGolemCore, instMat: true);
            core.name = skillName;
            core.transform.position = pos;
            core.transform.rotation = owner.PlayerViewRotation;

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
                GameObject cube = ObjectManager.Instance.InstanitateObject(_skillGolemCube, instMat: true);
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

        private GameObject CreateGolem(GolemInfo golemInfo, string golemName, Vector3 pos)
        {
            GameObject core = ObjectManager.Instance.InstanitateObject(_golemCore, instMat: true);
            core.name = golemName;
            pos.y += (float)golemInfo.GetDirectionSize(Enums.Direction3D.Down) * Configs.CUBE_BASE_LENGHT;
            core.transform.position = pos;

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
                GameObject cube = ObjectManager.Instance.InstanitateObject(_golemCube, instMat: true);

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