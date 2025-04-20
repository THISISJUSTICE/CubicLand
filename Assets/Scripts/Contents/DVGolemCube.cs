using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DVGolemCube : DVCubeBase
{
    #region Variables
    [Header("Viewer")]
    [SerializeField] private DVGolemCube _parent;
    [SerializeField] private DVGolemCube[] _childs;
    [SerializeField] private DVGolemCore _core;
    #endregion

    #region Properties
    public DVGolemCore Core { get => _core; }
    public BoxCollider Collider { get => _collider; }
    #endregion

    #region Unity Functions
    #endregion

    #region Public Functions
    public void SetGolemCubeInfo(DVCubeInfo cubeInfo, DVGolemCube parent, DVGolemCore core) {
        SetInit(cubeInfo);
        _parent = parent;
        _childs = null;
        _core = core;
    }

    public void SetGolemChild(DVGolemCube[] childs) { 
        _childs = childs;
    }

    public void OnParentDestroied() {
        _core.RemoveCube(this);
        _parent = null;
        _core = null;
        transform.SetParent(null);

        _cubeInfo.AttackMode = false;
        _cubeInfo.Status.CurrentStatus.SetAttackOff();

        if (_childs != null)
        {
            foreach (var child in _childs)
                child.OnParentDestroied();
            _childs = null;
        }

        var obstacle = GameObject.FindAnyObjectByType<DVCubeCreator>().CreateObstacleCube(_cubeInfo.Status);
        obstacle.transform.position = transform.position;
        obstacle.transform.rotation = transform.rotation;
        obstacle.NormalizeTransform();
        DVObjectManager.Instance.DestroyObject(gameObject);
    }

    public void OnChildDestroied(DVGolemCube child) { 
        List<DVGolemCube> childs = new List<DVGolemCube>();
        foreach (var prevChild in _childs) { 
            if(prevChild != child)
                childs.Add(prevChild);
        }
        if(childs.Count > 0)
            _childs = childs.ToArray();
        else
            _childs = null;
    }

    public void SetAttackMode(bool on) { 
        _cubeInfo.AttackMode = on;
        if (_childs != null)
        {
            foreach (var child in _childs)
                child.SetAttackMode(on);
        }
    }
    #endregion

    #region Utils
    protected override void OnCubeDestroied() {
        _core.RemoveCube(this);

        if (_parent != null)
        {
            _parent.OnChildDestroied(this);
            _parent = null;
        }
        if (_childs != null)
        {
            foreach (var child in _childs)
            {
                child.OnParentDestroied();
            }
            _childs = null;
        }

        if (_core != null)
        {
            _core = null;
        }

        base.OnCubeDestroied();
    }
    #endregion
}
