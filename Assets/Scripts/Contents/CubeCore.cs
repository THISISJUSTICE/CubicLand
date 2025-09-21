using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public abstract class CubeCore<T> : MonoBehaviour where T : CubeBase
    {
        #region Variables
        [Header("Viewer")]
        [SerializeField] protected GolemInfo _golemInfo;
        [SerializeField] protected T _golemCube;
        [SerializeField] protected GolemInfo _curGolemInfo;

        protected Dictionary<Vector3Int, T> _childs = new Dictionary<Vector3Int, T>();

        protected Rigidbody _rb;

        public GolemInfo GolemInfo { get => _golemInfo; }

        public T GolemCube { get => _golemCube; }

        public GolemInfo CurrentGolemInfo { get => _curGolemInfo; }

        public Rigidbody rb { get => _rb; }

        protected abstract Vector3 MoveVelocity { get; }
        #endregion

        #region Unity Functions
        protected virtual void Awake()
        {
            _golemCube = GetComponent<T>();
            _rb = GetComponent<Rigidbody>();
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
                return;

            if (collision.gameObject.CompareTag("Map"))
            {
                return;
            }

            OnCoreCollision(collision);
        }
        #endregion

        #region Public Functions
        public void AddChild(T child)
        {
            _childs[child.CubeInfo.ShapePosition] = child;
            SetGolemMass();
        }

        public void RemoveCube(T golemCube)
        {
            _curGolemInfo.RemoveCube(golemCube.CubeInfo.ShapePosition);

            var key = golemCube.CubeInfo.ShapePosition;
            if (_childs.ContainsKey(key))
            {
                _childs.Remove(key);
            }

            SetGolemMass();
        }

        public void RemoveCubes(List<T> golemCubes)
        {
            foreach (T golemCube in golemCubes)
            {
                _curGolemInfo.RemoveCube(golemCube.CubeInfo.ShapePosition);

                Vector3Int key = golemCube.CubeInfo.ShapePosition;
                if (_childs.ContainsKey(key))
                {
                    _childs.Remove(key);
                }
            }

            SetGolemMass();
        }

        public void SetGolemInfo(GolemInfo golemInfo)
        {
            _golemInfo = golemInfo;
            _curGolemInfo = new GolemInfo(_golemInfo);
            _golemCube.CubeInfo.EnhanceStatus(golemInfo.Status);

            //_golemCube.SetCubeInfo

            foreach (Vector3Int childPos in _curGolemInfo.GetChilds(Vector3Int.zero))
            {
                _childs[childPos].CubeInfo.
                    EnhanceStatus(_childs[_curGolemInfo.ParentMap[childPos]].CubeInfo.Status.GetChildStatus());
            }
        }

        public T FindCube(Vector3Int cubePos)
        {
            if (_childs.TryGetValue(cubePos, out var child))
                return child;
            return null;
        }

        public float CalculateMoveTime(float initTime, float minTime, float maxTime)
        {
            float weightFactor = Mathf.Pow(_rb.mass, 0.2f) / Mathf.Pow(Configs.INIT_CUBE_MASS, 0.2f);
            float speedFactor = Mathf.Pow((float)StatusConfig.INIT_MOVE_SPEED / (float)_golemInfo.MoveSpeed, 0.3f);
            float moveTime = initTime * weightFactor * speedFactor;

            return Mathf.Clamp(moveTime, minTime, maxTime);
        }
        #endregion

        #region Utils
        protected void SetupChilds()
        {
            _childs.Clear();

            foreach (var child in GetComponentsInChildren<Transform>())
            {
                var golemCube = child.GetComponent<T>();
                if (golemCube == null)
                {
                    Debug.LogError($"{child.name} doesn't have DVGolemCube");
                    continue;
                }

                _childs[golemCube.CubeInfo.ShapePosition] = golemCube;
            }
            SetGolemMass();
        }

        protected void OnCoreCollision(Collision collision)
        {
            // TODO: 확인
            var core = collision.gameObject.GetComponent<CubeCore<CubeBase>>();
            var obstacle = collision.gameObject.GetComponent<ObstacleCube>();

            Vector3 normalAVG = Vector3.zero;
            foreach (var contact in collision.contacts)
                normalAVG += contact.normal;
            normalAVG.Normalize();

            // TODO: Skill Collision
            // TODO: Skill Golem Collision

            if (core != null) // Golem Collision (다른 곳에서 호출, 먼저 호출하면 다른 쪽 호출은 무시)
            {

            }
            else if (obstacle != null) // Obstacle Collision
            {
                float maxDamageRate = 0f;
                ActOnChildCollsion(collision, (child) =>
                {
                    Vector3 impulse = Utils.EstimateImpulse(MoveVelocity, _rb.mass, obstacle.Velocity, obstacle.CubeMass, normalAVG);
                    if (impulse.magnitude <= collision.impulse.magnitude)
                        impulse = -collision.impulse;

                    this.WaitFrameAct(1, () =>
                    {
                        if (child.Usable())
                        {
                            child.OnDamaged(_rb.mass, -impulse, obstacle.CubeInfo, out float damageRate);
                            maxDamageRate = Mathf.Max(maxDamageRate, damageRate);
                        }
                    });

                    this.WaitFrameAct(1, () =>
                    {
                        if (obstacle.Usable())
                        {
                            obstacle.OnDamaged(impulse, child.CubeInfo);
                        }
                    });
                });

                Vector3 impulse = collision.impulse + collision.impulse.normalized * maxDamageRate;
                OnImpulse(impulse);
            }

            this.WaitFrameAct(1, () =>
            {
                if (this.Usable())
                    SetGolemMass();
            });
        }

        protected virtual void OnImpulse(Vector3 impulse) { }

        protected void ActOnChildCollsion(Collision collision, Action<T> onCollisionCallback)
        {
            Vector3 center;
            Quaternion rotation = transform.rotation;
            Vector3 size;
            foreach (var child in _childs.Values)
            {
                center = child.transform.position;
                size = child.Collider.bounds.size;
                Collider[] colliders = Physics.OverlapBox(center, size / 2f, rotation);

                foreach (var collider in colliders)
                {
                    if (collider == collision.collider)
                    {
                        onCollisionCallback?.Invoke(child);
                        break;
                    }
                }

                if (_childs == null || _childs.Count <= 0)
                    break;
            }
        }

        protected void SetGolemMass()
        {
            float mass = 0f;
            foreach (var child in _childs.Values)
            {
                mass += child.CubeMass;
            }
            _rb.mass = mass;
        }
        #endregion
    }
}