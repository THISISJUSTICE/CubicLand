using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Commar.CubicLand.Cube
{
    [RequireComponent(typeof(CubeObject), typeof(Rigidbody))]
    public class GolemCore : MonoBehaviour, IGolemObject
    {
        private ICubeCollisionResolver _cubeCollisionResolver;
        private IOrphanedCubeHandler _orphanedCubeHandler;
        private ICubeFactory _cubeFactory;
        private ICubeMotionAdjuster _motionAdjuster;

        private readonly HashSet<IOnEnablable> _onEnablables = new HashSet<IOnEnablable>();
        private readonly HashSet<IFixedUpdatable> _fixedUpdatables = new HashSet<IFixedUpdatable>();

        private readonly Dictionary<Vector3Int, GolemCubeHandle> _cubes = new Dictionary<Vector3Int, GolemCubeHandle>();
        private readonly List<Vector3Int> _breakedCubePositions = new List<Vector3Int>();

        private readonly List<GolemCubeData> _tempChildren = new List<GolemCubeData>();
        private readonly List<GolemCubeHandle> _tempHandles = new List<GolemCubeHandle>();

        private Rigidbody _rigidbody;
        private bool _isAttackMode;

        public event Action<CubeObject> onHealed;
        internal event Action<CubeObject> onReleased;

        Rigidbody IGolemObject.Rigidbody => _rigidbody;
        public GolemData GolemData { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            if (GolemData != null)
            {
                bool hasNull = false;

                foreach (IOnEnablable onEnalblable in _onEnablables)
                {
                    if (onEnalblable == null)
                    {
                        hasNull = true;
                        continue;
                    }

                    onEnalblable.OnEnable();
                }

                if (hasNull)
                    _onEnablables.Remove(null);
            }
        }

        private void OnDisable()
        {
            foreach (GolemCubeHandle handle in _cubes.Values)
            {
                if (handle.IsValid)
                    handle.cube.onCubeDestoried -= OnParentCubeDestoried;
            }

            _cubes.Clear();
            _breakedCubePositions.Clear();
            GolemData = null;
        }

        private void FixedUpdate()
        {
            bool hasNull = false;

            foreach (IFixedUpdatable fixedUpdatable in _fixedUpdatables)
            {
                if (fixedUpdatable == null)
                {
                    hasNull = true;
                    continue;
                }

                fixedUpdatable.FixedUpdate();
            }

            if (hasNull)
                _fixedUpdatables.Remove(null);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
                return;

            foreach (ContactPoint contact in collision.contacts)
                _cubeCollisionResolver.OnCollision(contact.thisCollider.gameObject, _motionAdjuster, collision, true);
            UpdateGolemMass();
        }

        internal void Initialize(ICubeCollisionResolver cubeCollisionResolver, IOrphanedCubeHandler orphanedCubeHandler, ICubeFactory cubeFactory, ICubeMotionAdjuster motionAdjuster)
        {
            _cubeCollisionResolver = cubeCollisionResolver;
            _orphanedCubeHandler = orphanedCubeHandler;
            _cubeFactory = cubeFactory;
            _motionAdjuster = motionAdjuster;
        }

        public void Initialize(GolemData golemData, IList<GolemCubeHandle> handles)
        {
            GolemData = golemData;

            _breakedCubePositions.Clear();
            foreach (GolemCubeData cubeData in golemData.CubeDatas.Values)
            {
                if (cubeData.IsBreaked)
                    _breakedCubePositions.Add(cubeData.shapePoisition);
            }

            SortBreakedCubeList();
            _cubes.Clear();

            foreach (GolemCubeHandle handle in handles)
            {
                if (!handle.IsValid)
                    continue;

                _cubes.Add(handle.data.shapePoisition, handle);
                handle.cube.onCubeDestoried += OnParentCubeDestoried;
            }

            SetAttackMode(false);
            UpdateGolemMass();

            OnEnable();
        }

        void IGolemObject.AddUnityRoutine(IOnEnablable onEnablable)
        {
            _onEnablables.Add(onEnablable);
        }

        void IGolemObject.AddUnityRoutine(IFixedUpdatable fixedUpdatable)
        {
            _fixedUpdatables.Add(fixedUpdatable);
        }

        void IGolemObject.SetAttackMode(bool attackMode)
        {
            SetAttackMode(attackMode);
        }

        Coroutine IGolemObject.StartCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        void IGolemObject.StopCoroutine(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }

        public float CalculateMoveTime(float initTime, float minTime, float maxTime)
        {
            float weightFactor = Mathf.Pow(_rigidbody.mass, CubeConfig.WEIGHT_EXPONENT) / Mathf.Pow(CubeUtil.CalculateBasicCubeObjectMass(), CubeConfig.WEIGHT_EXPONENT);
            float speedFactor = Mathf.Pow((float)CubeConfig.Status.INIT_MOVE_SPEED / GolemData.MoveSpeed, CubeConfig.SPEED_EXPONENT);
            float moveTime = initTime * weightFactor * speedFactor;

            return Mathf.Clamp(moveTime, minTime, maxTime);
        }

        public CubeObject FindCube(Vector3Int position)
        {
            if (GolemData == null)
                return null;

            if (_cubes.TryGetValue(position, out GolemCubeHandle handle))
                return handle.cube;
            return null;
        }

        public static bool AttachCube(GolemCubeHandle parentHandle, GolemCubeHandle childHandle)
        {
            if (!parentHandle.IsValid || !childHandle.IsValid)
                return false;

            Vector3Int childPosition = childHandle.data.shapePoisition;
            childHandle.cube.transform.SetParent(parentHandle.cube.transform);
            childHandle.cube.transform.localPosition = (Vector3)(childPosition - parentHandle.data.shapePoisition) * CubeConfig.CUBE_BASE_LENGHT;
            childHandle.cube.transform.localRotation = Quaternion.identity;
            childHandle.cube.transform.localScale = Vector3.one;

            return true;
        }

        internal bool AddCube(Vector3Int parentPosition, Enums.Direction3D direction)
        {
            if (!GolemData.TryAddCube(parentPosition, direction, out Vector3Int position)
                || !GolemData.CubeDatas.TryGetValue(position, out GolemCubeData cubeData))
                return false;

            if (GolemData.CubeDatas.TryGetValue(parentPosition, out GolemCubeData parent)
                && parent.IsBreaked)
            {
                RecreateBreakedCube(parentPosition);
                cubeData.IsBreaked = true;
                _breakedCubePositions.Add(position);
                SortBreakedCubeList();
            }
            else if (!CreateCube(cubeData, CubeSpawnOptions.ANIMATED))
            {
                cubeData.IsBreaked = true;
                _breakedCubePositions.Add(position);
                SortBreakedCubeList();

                Debug.LogError($"Failed to create cube at position({position}).");
                return false;
            }

            UpdateGolemMass();

            return true;
        }

        internal void OnHealed(int heal)
        {
            if (!_cubes.TryGetValue(CubeConfig.CORE_POSITION, out GolemCubeHandle handle))
                return;

            heal = ApplyHeal(heal, handle);
            DistributeHeal(heal, new GolemCubeHandle[] { handle });

            RecreateBreakedCubes();
            UpdateGolemMass();
        }

        internal void EnhanceStatus(StatusPoint statusPoint)
        {
            if (GolemData == null || !GolemData.CubeDatas.TryGetValue(CubeConfig.CORE_POSITION, out GolemCubeData cubeData))
                return;

            cubeData.EnhanceStatus(statusPoint);
            EnhanceChildStatus(cubeData);

            UpdateGolemMass();
        }

        private void OnParentCubeDestoried(CubeObject cube)
        {
            GolemCubeHandle handle = new GolemCubeHandle(cube);
            cube.onCubeDestoried -= OnParentCubeDestoried;

            if (!handle.IsValid)
                return;

            _cubes.Remove(handle.data.shapePoisition);
            handle.data.IsBreaked = true;
            _breakedCubePositions.Add(handle.data.shapePoisition);

            GolemData.FindChildren(handle.data.shapePoisition, _tempChildren);
            foreach (GolemCubeData child in _tempChildren)
            {
                if (!_cubes.TryGetValue(child.shapePoisition, out GolemCubeHandle childHandle))
                    continue;

                _orphanedCubeHandler?.HandleOrphanedCube(childHandle.cube);
                _cubeFactory.DestoryCube(childHandle.cube);
                childHandle.cube.onCubeDestoried -= OnParentCubeDestoried;
                _cubes.Remove(child.shapePoisition);
                childHandle.data.StatusValue.ApplyDamage(childHandle.data.StatusValue.MaxHP);
                childHandle.data.IsBreaked = true;
                _breakedCubePositions.Add(childHandle.data.shapePoisition);
            }

            UpdateGolemMass();
            SortBreakedCubeList();

            if (handle.data.shapePoisition == CubeConfig.CORE_POSITION)
                onReleased?.Invoke(cube);
        }

        private int ApplyHeal(int heal, GolemCubeHandle handle)
        {
            StatusValue statusValue = handle.data.StatusValue;
            if (!statusValue.IsFullHP() && heal > 0)
            {
                heal = statusValue.Heal(heal);
                handle.cube.UpdateCubeObject();
                onHealed?.Invoke(handle.cube);

                if (heal <= 0)
                    return 0;
            }

            return heal;
        }

        private void DistributeHeal(int heal, IList<GolemCubeHandle> handles)
        {
            if (heal <= 0 || handles == null || handles.Count == 0)
                return;

            List<GolemCubeHandle> children = new List<GolemCubeHandle>();

            foreach (GolemCubeHandle handle in handles)
                FindValidChildren(handle.data.shapePoisition, children);

            if (children.Count == 0)
                return;

            List<GolemCubeHandle> targets = _tempHandles;
            targets.Clear();
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
                    if (targets[i].data.StatusValue.IsFullHP())
                    {
                        if (targets[i].data.IsBreaked)
                            targets[i].data.IsBreaked = false;
                        targets.RemoveAt(i);
                    }
                }
            }

            DistributeHeal(heal, children);
        }

        private void FindValidChildren(Vector3Int position, IList<GolemCubeHandle> handles)
        {
            if (handles == null || !GolemData.Children.TryGetValue(position, out List<Vector3Int> children) || children.Count == 0)
                return;

            foreach (Vector3Int childPosition in children)
            {
                if (_cubes.TryGetValue(childPosition, out GolemCubeHandle handle))
                    handles.Add(handle);
            }
        }

        private void UpdateGolemMass()
        {
            float mass = 0f;
            foreach (GolemCubeHandle handle in _cubes.Values)
            {
                mass += handle.cube.Mass;
            }
            _rigidbody.mass = mass;
        }

        private void RecreateBreakedCube(Vector3Int position)
        {
            if (GolemData.Parents.TryGetValue(position, out Vector3Int parentPosition)
                && GolemData.CubeDatas.TryGetValue(parentPosition, out GolemCubeData parent)
                && parent.IsBreaked)
            {
                RecreateBreakedCube(parentPosition);
                return;
            }

            if (!GolemData.CubeDatas.TryGetValue(position, out GolemCubeData cubeData))
                return;

            cubeData.IsBreaked = false;
            cubeData.IsAttackMode = _isAttackMode;

            if (CreateCube(cubeData, CubeSpawnOptions.ANIMATED))
                _breakedCubePositions.Remove(position);
            else
                cubeData.IsBreaked = true;
        }

        private void RecreateBreakedCubes()
        {
            if (_breakedCubePositions.Count == 0)
                return;

            for (int i = 0; i < _breakedCubePositions.Count; i++)
            {
                Vector3Int position = _breakedCubePositions[i];

                if (!GolemData.CubeDatas.TryGetValue(position, out GolemCubeData cubeData)
                    || !cubeData.StatusValue.IsFullHP()
                    || !GolemData.Parents.TryGetValue(position, out Vector3Int parentPosition)
                    || !GolemData.CubeDatas.TryGetValue(parentPosition, out GolemCubeData parent)
                    || parent.IsBreaked)
                    continue;

                cubeData.IsBreaked = false;
                cubeData.IsAttackMode = _isAttackMode;

                if (CreateCube(cubeData, CubeSpawnOptions.ANIMATED))
                    _breakedCubePositions.RemoveAt(i--);
                else
                    cubeData.IsBreaked = true;
            }
        }

        private bool CreateCube(GolemCubeData cubeData, CubeSpawnOptions options)
        {
            Vector3Int position = cubeData.shapePoisition;
            if (_cubes.ContainsKey(position)
                || !GolemData.Parents.TryGetValue(position, out Vector3Int parentPosition)
                || !_cubes.TryGetValue(parentPosition, out GolemCubeHandle parentHandle))
                return false;

            GolemCubeHandle handle = new GolemCubeHandle(_cubeFactory.CreateCube(cubeData, options));
            if (!handle.IsValid)
                return false;

            AttachCube(parentHandle, handle);

            _cubes.Add(position, handle);
            handle.cube.onCubeDestoried += OnParentCubeDestoried;
            return true;
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

        private void EnhanceChildStatus(GolemCubeData parent)
        {
            if (!GolemData.Children.TryGetValue(parent.shapePoisition, out List<Vector3Int> childList))
                return;

            foreach (Vector3Int position in childList)
            {
                if (!GolemData.CubeDatas.TryGetValue(position, out GolemCubeData cubeData))
                    continue;

                cubeData.EnhanceChildStatus(parent);
                EnhanceChildStatus(cubeData);
            }
        }

        private void SetAttackMode(bool attackMode)
        {
            _isAttackMode = attackMode;

            foreach (GolemCubeHandle handle in _cubes.Values)
                handle.data.IsAttackMode = _isAttackMode;
        }
    }
}