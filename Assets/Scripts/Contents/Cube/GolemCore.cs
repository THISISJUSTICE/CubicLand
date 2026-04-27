using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    [RequireComponent(typeof(CubeObject), typeof(Rigidbody))]
    public class GolemCore : MonoBehaviour
    {
        private ICubeCollisionResolver _cubeCollisionResolver;
        private IOrphanedCubeHandler _orphanedCubeHandler;
        private ICubeFactory _cubeFactory;

        private readonly Dictionary<Vector3Int, CubeObject> _cubes = new Dictionary<Vector3Int, CubeObject>();
        private readonly List<Vector3Int> _breakedCubePositions = new List<Vector3Int>();

        private bool _isAttackMode;

        public event Action onDamaged;
        public event Action<CubeObject> onHealed;

        internal Rigidbody Rigidbody { get; private set; }
        public GolemData GolemData { get; private set; }
        public Vector3 MoveVelocity { get; internal set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            _cubes[CubeConfig.CORE_POSITION] = GetComponent<CubeObject>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
                return;

            _cubeCollisionResolver.OnCollision(gameObject, collision);
            UpdateGolemMass();
        }

        internal void Initialize(GolemData golemData, IList<CubeObject> cubes, ICubeCollisionResolver cubeCollisionResolver, IOrphanedCubeHandler orphanedCubeHandler, ICubeFactory cubeFactory)
        {
            _cubeCollisionResolver = cubeCollisionResolver;
            _orphanedCubeHandler = orphanedCubeHandler;
            _cubeFactory = cubeFactory;
            Initialize(golemData, cubes);
        }

        public void Initialize(GolemData golemData, IList<CubeObject> cubes)
        {
            GolemData = golemData;

            _breakedCubePositions.Clear();
            foreach (CubeData cubeData in golemData.CubeDatas.Values)
            {
                if (cubeData.IsBreaked)
                    _breakedCubePositions.Add(cubeData.ShapePoisition);
            }
            SortBreakedCubeList();

            _cubes.Clear();
            foreach (CubeObject cube in cubes)
            {
                _cubes[cube.CubeData.ShapePoisition] = cube;
                cube.Initialize(golemData.CubeDatas[cube.CubeData.ShapePoisition]);
                cube.onCubeDestoried += OnParentCubeDestoried;
            }

            UpdateGolemMass();
        }

        public bool AddCube(Vector3Int parentPosition, Enums.Direction3D direction)
        {
            if (!GolemData.TryAddCube(parentPosition, direction, out Vector3Int position)
                || !GolemData.CubeDatas.TryGetValue(position, out CubeData cubeData))
                return false;

            if (GolemData.CubeDatas.TryGetValue(parentPosition, out CubeData parent)
                && parent.IsBreaked)
            {
                RecreateBreakedCube(parentPosition);
                _breakedCubePositions.Add(position);
                SortBreakedCubeList();
            }
            else
                _cubeFactory.CreateCube(cubeData);

            return true;
        }

        public CubeObject FindCube(Vector3Int position)
        {
            if (_cubes.TryGetValue(position, out CubeObject cube))
                return cube;
            return null;
        }

        public float CalculateMoveTime(float initTime, float minTime, float maxTime)
        {
            float weightFactor = Mathf.Pow(Rigidbody.mass, CubeConfig.WEIGHT_EXPONENT) / Mathf.Pow(CubeUtil.CalculateBasicCubeObjectMass(), CubeConfig.WEIGHT_EXPONENT);
            float speedFactor = Mathf.Pow((float)CubeConfig.Status.INIT_MOVE_SPEED / GolemData.MoveSpeed, CubeConfig.SPEED_EXPONENT);
            float moveTime = initTime * weightFactor * speedFactor;

            return Mathf.Clamp(moveTime, minTime, maxTime);
        }

        public void SetAttackMode(bool attackMode)
        {
            _isAttackMode = attackMode;

            foreach (CubeObject cube in _cubes.Values)
                cube.CubeData.IsAttackMode = _isAttackMode;
        }

        public CubeObject FindCollisionCube(Collision collision)
        {
            foreach (CubeObject cube in _cubes.Values)
            {
                if (cube.Collider == collision.collider)
                    return cube;
            }

            return null;
        }

        public void OnHealed(int heal)
        {
            if (!_cubes.TryGetValue(CubeConfig.CORE_POSITION, out CubeObject cube))
                return;

            heal = ApplyHeal(heal, cube);
            DistributeHeal(heal, cube);

            RecreateBreakedCubes();
            UpdateGolemMass();
        }

        public void EnhanceStatus(StatusPoint statusPoint)
        {
            if (!GolemData.CubeDatas.TryGetValue(CubeConfig.CORE_POSITION, out CubeData cubeData))
                return;

            cubeData.EnhanceStatus(statusPoint);
            EnhanceChildStatus(cubeData);

            UpdateGolemMass();
        }

        internal void RaiseOnDamaged()
        { 
            onDamaged?.Invoke();
        }

        private void OnParentCubeDestoried(CubeObject cube)
        {
            _cubes.Remove(cube.CubeData.ShapePoisition);
            cube.onCubeDestoried -= OnParentCubeDestoried;
            cube.CubeData.IsBreaked = true;
            _breakedCubePositions.Add(cube.CubeData.ShapePoisition);

            List<CubeData> children = GolemData.FindChildren(cube.CubeData.ShapePoisition);
            foreach (CubeData child in children)
            {
                if (!_cubes.TryGetValue(child.ShapePoisition, out CubeObject childCube))
                    continue;

                _orphanedCubeHandler?.HandleOrphanedCube(childCube);
                childCube.onCubeDestoried -= OnParentCubeDestoried;
                _cubes.Remove(child.ShapePoisition);
                childCube.CubeData.StatusValue.ApplyDamage(childCube.CubeData.StatusValue.MaxHP);
                childCube.CubeData.IsBreaked = true;
                _breakedCubePositions.Add(childCube.CubeData.ShapePoisition);
            }

            UpdateGolemMass();
            SortBreakedCubeList();
        }

        private int ApplyHeal(int heal, CubeObject cube)
        {
            StatusValue statusValue = cube.CubeData.StatusValue;
            if (!statusValue.IsFullHP() && heal > 0)
            {
                cube.CubeData.StatusValue.Heal(heal);
                heal -= cube.CubeData.StatusValue.HP - statusValue.HP;
                cube.UpdateCubeObject();
                onHealed?.Invoke(cube);

                if (heal <= 0)
                    return 0;
            }

            return heal;
        }

        private void DistributeHeal(int heal, params CubeObject[] cubes)
        {
            if (heal <= 0 || cubes == null || cubes.Length == 0)
                return;

            List<CubeObject> children = new List<CubeObject>();
            foreach (CubeObject cube in cubes)
            {
                List<CubeObject> validChildren = FindValidChildren(cube.CubeData.ShapePoisition);
                if (validChildren != null && validChildren.Count > 0)
                    children.AddRange(validChildren);
            }

            if (children.Count == 0)
                return;

            List<CubeObject> targets = new List<CubeObject>();
            targets.AddRange(children);

            while (heal > 0 && targets.Count > 0)
            {
                int baseShare = heal / targets.Count;
                int remainder = heal % targets.Count;

                for (int i = 0; i < targets.Count && heal > 0; i++)
                {
                    int offer = 1;
                    if (baseShare > 0)
                        offer = baseShare + (i < remainder ? 1 : 0);
                    heal += ApplyHeal(offer, targets[i]) - offer;
                }

                for (int i = targets.Count - 1; i >= 0; i--)
                {
                    if (targets[i].CubeData.StatusValue.IsFullHP())
                    {
                        if (targets[i].CubeData.IsBreaked)
                            targets[i].CubeData.IsBreaked = false;
                        targets.RemoveAt(i);
                    }
                }
            }

            DistributeHeal(heal, children.ToArray());
        }

        private List<CubeObject> FindValidChildren(Vector3Int position)
        {
            if (!GolemData.Children.TryGetValue(position, out List<Vector3Int> children) || children.Count == 0)
                return null;

            List<CubeObject> result = new List<CubeObject>();
            foreach (Vector3Int childPosition in children)
            {
                if (_cubes.TryGetValue(childPosition, out CubeObject cube))
                    result.Add(cube);
            }

            return result;
        }

        private void UpdateGolemMass()
        {
            float mass = 0f;
            foreach (CubeObject cube in _cubes.Values)
            {
                mass += cube.Mass;
            }
            Rigidbody.mass = mass;
        }

        private void RecreateBreakedCube(Vector3Int position)
        {
            if (!GolemData.Parents.TryGetValue(position, out Vector3Int parentPosition)
                && GolemData.CubeDatas.TryGetValue(parentPosition, out CubeData parent)
                && parent.IsBreaked)
            {
                RecreateBreakedCube(parentPosition);
                return;
            }

            if (!GolemData.CubeDatas.TryGetValue(position, out CubeData cubeData))
                return;

            cubeData.IsBreaked = false;
            cubeData.IsAttackMode = _isAttackMode;

            _cubeFactory.CreateCube(cubeData);
            _breakedCubePositions.Remove(position);
        }

        private void RecreateBreakedCubes()
        {
            if (_breakedCubePositions.Count == 0)
                return;

            for (int i = 0; i < _breakedCubePositions.Count; i++)
            {
                Vector3Int position = _breakedCubePositions[i];

                if (!GolemData.CubeDatas.TryGetValue(position, out CubeData cubeData)
                    || !cubeData.StatusValue.IsFullHP()
                    || !GolemData.Parents.TryGetValue(position, out Vector3Int parentPosition)
                    || !GolemData.CubeDatas.TryGetValue(parentPosition, out CubeData parent)
                    || parent.IsBreaked)
                    continue;

                cubeData.IsBreaked = false;
                cubeData.IsAttackMode = _isAttackMode;

                _cubeFactory.CreateCube(cubeData);
                _breakedCubePositions.RemoveAt(i--);
            }

        }

        private void SortBreakedCubeList()
        {
            if (_breakedCubePositions.Count <= 1)
                return;

            _breakedCubePositions.Sort((a, b) =>
            {
                if (!GolemData.ChildDepths.TryGetValue(a, out int depthA)
                 || !GolemData.ChildDepths.TryGetValue(b, out int depthB))
                    return 0;

                return depthB - depthA;
            });
        }

        private void EnhanceChildStatus(CubeData parent)
        {
            if (!GolemData.Children.TryGetValue(parent.ShapePoisition, out List<Vector3Int> childList))
                return;

            foreach (Vector3Int position in childList)
            {
                if (!GolemData.CubeDatas.TryGetValue(position, out CubeData cubeData))
                    continue;

                cubeData.EnhanceChildStatus(parent);
                EnhanceChildStatus(cubeData);
            }
        }
    }
}