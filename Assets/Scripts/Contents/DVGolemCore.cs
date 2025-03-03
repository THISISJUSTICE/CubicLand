using System;
using UnityEngine;

[RequireComponent(typeof(DVGolemCube))]
public class DVGolemCore : MonoBehaviour
{
    #region Variables
    [Header("Viewer")]
    [SerializeField] private DVGolemInfo _golemInfo;
    [SerializeField] private DVGolemCube _golemCube;
    [SerializeField] private DVGolemInfo _curGolemInfo;

    private Rigidbody _rb;
    #endregion

    #region Properties
    public DVGolemInfo GolemInfo { get => _golemInfo; }

    public DVGolemCube GolemCube { get => _golemCube; }

    public DVGolemInfo CurrentGolemInfo { get => _curGolemInfo; }

    public Rigidbody rb { get => _rb; }
    #endregion

    #region Unity Functions
    private void Awake()
    {
        _golemCube = GetComponent<DVGolemCube>();
    }

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
            return;

        if (collision.gameObject.tag == "Map")
        {

            return;
        }

        _rb.useGravity = true;
    }
    #endregion

    #region Public Functions
    public void Init() {
        _rb = GetComponent<Rigidbody>();
        _rb.Reset();
        _rb.useGravity = true;
        _rb.UseAngular(false);
        _rb.SetGolemMass(this);
    }

    public void RemoveCube(DVGolemCube golemCube)
    {
        _curGolemInfo.RemoveCube(golemCube.CubeInfo.ShapePosition);
        _rb.SetGolemMass(this);
    }

    public void SetGolemInfo(DVGolemInfo golemInfo) { 
        _golemInfo = golemInfo;
        _curGolemInfo = new DVGolemInfo(_golemInfo);
    }

    public void SetAttackMode(bool on) { 
        _golemCube.SetAttackMode(on);
    }

    public DVGolemCube FindCube(Vector3Int cubePos)
    {
        foreach (var child in GetComponentsInChildren<Transform>()) { 
            DVGolemCube cube = child.GetComponent<DVGolemCube>();
            if(cube == null)
                continue;

            if (cube.CubeInfo.ShapePosition == cubePos)
                return cube;
        }

        return null;
    }
    #endregion

    #region Utils

    #endregion
}
