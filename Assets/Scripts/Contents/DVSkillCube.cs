using UnityEngine;

public class DVSkillCube : DVCubeBase
{
    #region Variables
    private Rigidbody _rb;

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
    public override void SetCubeInfo(DVCubeInfo cubeInfo)
    {
        _cubeInfo = cubeInfo;
        SetCubeShader();
        cubeInfo.AttackMode = true;
        CubeMass = DVConfigs.ONE_CUBE_MASS;
    }
    #endregion

    #region Utils
    protected override void SetCubeShader()
    {
        
    }

    protected override void OnCubeDestroied()
    {
        // TODO: ÆÄ±« ÀÌÆåÆ®

        DVObjectManager.Instance.DestroyObject(gameObject);
    }
    #endregion
}
