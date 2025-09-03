using System.Collections.Generic;
using UnityEngine;

public class DVSkillGolemCube : DVCubeBase
{
    #region Variables
    [SerializeField] private DVSkillGolemCube _parent;
    [SerializeField] private List<DVSkillGolemCube> _childs;
    [SerializeField] private DVSkillGolemCore _core;

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
    public void SetGolemCubeInfo(DVCubeInfo cubeInfo, DVSkillGolemCube parent, DVSkillGolemCore core)
    {
        SetCubeInfo(cubeInfo);
        _parent = parent;
        _childs = new List<DVSkillGolemCube>();
        _core = core;

        _trail.enabled = true;
    }

    public override void SetCubeInfo(DVCubeInfo cubeInfo)
    {
        _cubeInfo = cubeInfo;
        SetCubeShader();
        cubeInfo.AttackMode = true;
        CubeMass = DVConfigs.ONE_CUBE_MASS;
    }

    public void AddGolemChild(DVSkillGolemCube childs)
    {
        _childs.Add(childs);
    }

    public override void OnDamaged(float selfMass, Vector3 impulse, DVCubeInfo? colCubeInfo, out float damageRate)
    {
        damageRate = 1f;
        OnCubeDestroied();
    }

    public void OnParentDestroied(List<DVSkillGolemCube> remove)
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

        // TODO: SKill Cube·Î ´ëÃ¼

        OnCubeDestroied(); // ÀÓ½Ã
    }

    public void OnChildDestroied(DVSkillGolemCube child)
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
                    child.OnParentDestroied(new List<DVSkillGolemCube>());
                }
                _childs.Clear();
            }

            _core = null;
        }

        _trail.enabled = false;

        // TODO: ÆÄ±« ÀÌÆåÆ®

        DVObjectManager.Instance.DestroyObject(gameObject);
    }
    #endregion
}