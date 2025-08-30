using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DVGolemCube : DVCubeBase
{
    #region Variables
    [SerializeField] private DVGolemCube _parent;
    [SerializeField] private List<DVGolemCube> _childs;
    [SerializeField] private DVGolemCore _core;
    #endregion

    #region Properties
    public DVGolemCore Core { get => _core; }
    #endregion

    #region Unity Functions
    #endregion

    #region Public Functions
    public void SetGolemCubeInfo(DVCubeInfo cubeInfo, DVGolemCube parent, DVGolemCore core)
    {
        SetCubeInfo(cubeInfo);
        _parent = parent;
        _childs = new List<DVGolemCube>();
        _core = core;
    }

    public void AddGolemChild(DVGolemCube childs)
    {
        _childs.Add(childs);
    }

    public void OnParentDestroied(List<DVGolemCube> remove)
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

        var obstacle = GameObject.FindAnyObjectByType<DVCubeCreator>().CreateObstacleCube(_cubeInfo.Status);
        obstacle.transform.position = transform.position;
        obstacle.transform.rotation = transform.rotation;
        obstacle.NormalizeTransform();
        DVObjectManager.Instance.DestroyObject(gameObject);
    }

    public void OnChildDestroied(DVGolemCube child)
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
                    child.OnParentDestroied(new List<DVGolemCube>());
                }
                _childs.Clear();
            }

            _core = null;
        }

        base.OnCubeDestroied();
    }
    #endregion
}
