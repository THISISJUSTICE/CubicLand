using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public class SkillGolemCube : CubeBase
    {
        #region Variables
        [SerializeField] private SkillGolemCube _parent;
        [SerializeField] private List<SkillGolemCube> _childs;
        [SerializeField] private SkillGolemCore _core;

        private TrailRenderer _trail;
        #endregion

        #region Unity Functions
        protected override void Awake()
        {
            _trail = GetComponent<TrailRenderer>();
            base.Awake();
        }
        #endregion

        #region Public Functions
        public void SetGolemCubeInfo(CubeInfo cubeInfo, SkillGolemCube parent, SkillGolemCore core)
        {
            SetCubeInfo(cubeInfo);
            _parent = parent;
            _childs = new List<SkillGolemCube>();
            _core = core;

            _trail.enabled = true;
        }

        public override void SetCubeInfo(CubeInfo cubeInfo)
        {
            _cubeInfo = cubeInfo;
            SetCubeShader();
            cubeInfo.AttackMode = true;
            CubeMass = Configs.ONE_CUBE_MASS;
        }

        public void AddGolemChild(SkillGolemCube childs)
        {
            _childs.Add(childs);
        }

        public override void OnDamaged(float selfMass, Vector3 impulse, CubeInfo? colCubeInfo, out float damageRate)
        {
            damageRate = 1f;
            OnCubeDestroied();
        }

        public void OnParentDestroied(List<SkillGolemCube> remove)
        {
            remove.Add(this);
            if (_childs.Count > 0)
            {
                foreach (var child in _childs)
                    child.OnParentDestroied(remove);
                _childs.Clear();
            }
            else
            {
                _core.RemoveCubes(remove);
            }

            _parent = null;
            _core = null;
            transform.SetParent(null);

            // TODO: SKill Cube로 대체

            OnCubeDestroied(); // 임시
        }

        public void OnChildDestroied(SkillGolemCube child)
        {
            _childs.Remove(child);
        }
        #endregion

        #region Utils
        protected override void SetCubeShader()
        {
            _meshRen.sharedMaterial.SetColor("_Color", _cubeInfo.Status.Color);
        }

        protected override void OnCubeDestroied()
        {
            if (_core != null)
            {
                _core.RemoveCube(this);

                if (_parent != null)
                {
                    _parent.OnChildDestroied(this);
                    _parent = null;
                }
                if (_childs.Count > 0)
                {
                    foreach (var child in _childs)
                    {
                        child.OnParentDestroied(new List<SkillGolemCube>());
                    }
                    _childs.Clear();
                }

                _core = null;
            }

            _trail.enabled = false;

            // TODO: 파괴 이펙트

            ObjectManager.Instance.DestroyGameObject(gameObject);
        }
        #endregion
    }
}