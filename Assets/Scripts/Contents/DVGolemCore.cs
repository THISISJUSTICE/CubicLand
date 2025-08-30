using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DVGolemCube))]
public class DVGolemCore : DVCubeCore<DVGolemCube>
{
    #region Variables
    private DVGolemController _golemController;

    protected override Vector3 MoveVelocity => _golemController.MoveVelocity;
    #endregion

    #region Unity Functions
    protected override void Awake()
    {
        base.Awake();
        _golemCube = GetComponent<DVGolemCube>();
    }
    #endregion

    #region Public Functions
    public void SetInit(DVGolemInfo golemInfo)
    {
        SetupChilds();
        SetGolemInfo(golemInfo);

        _rb.Reset();
        _rb.UseAngular(false);
    }

    public void SetGolemController(DVGolemController golemController) {
        _golemController = golemController;
    }

    public void SetAttackMode(bool on) { 
        _golemCube.SetAttackMode(on);
    }
    #endregion

    #region Utils
    protected override void OnImpulse(Vector3 impulse)
    {
        _golemController.OnImpulse(impulse);
    }
    #endregion
}
