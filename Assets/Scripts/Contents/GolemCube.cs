using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public class GolemCube : CubeBase
    {
        #region Variables
        [SerializeField] private GolemCube _parent;
        [SerializeField] private List<GolemCube> _childs;
        [SerializeField] private GolemCore _core;
        #endregion

        #region Properties
        public GolemCore Core { get => _core; }
        #endregion

        #region Unity Functions
        #endregion

        #region Public Functions
        public void SetGolemCubeInfo(CubeInfo cubeInfo, GolemCube parent, GolemCore core)
        {
            SetCubeInfo(cubeInfo);
            _parent = parent;
            _childs = new List<GolemCube>();
            _core = core;
        }

        public void AddGolemChild(GolemCube childs)
        {
            _childs.Add(childs);
        }

        public void OnParentDestroied(List<GolemCube> remove)
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

            _cubeInfo.AttackMode = false;
            _cubeInfo.CurrentStatus.SetAttackOff();

            var obstacle = GameObject.FindAnyObjectByType<CubeCreator>().CreateObstacleCube(_cubeInfo.Status);
            obstacle.transform.position = transform.position;
            obstacle.transform.rotation = transform.rotation;
            obstacle.NormalizeTransform();
            ObjectManager.Instance.DestroyObject(gameObject);
        }

        public void OnChildDestroied(GolemCube child)
        {
            _childs.Remove(child);
        }

        public void SetAttackMode(bool on)
        {
            _cubeInfo.AttackMode = on;
            if (_childs.Count > 0)
            {
                foreach (var child in _childs)
                    child.SetAttackMode(on);
            }
        }
        #endregion

        #region Utils
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
                        child.OnParentDestroied(new List<GolemCube>());
                    }
                    _childs.Clear();
                }

                _core = null;
            }

            base.OnCubeDestroied();
        }
        #endregion
    }
}