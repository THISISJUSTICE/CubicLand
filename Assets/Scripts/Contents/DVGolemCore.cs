using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DVGolemCube))]
public class DVGolemCore : MonoBehaviour
{
    #region Variables
    [Header("Viewer")]
    [SerializeField] private DVGolemInfo _golemInfo;
    [SerializeField] private DVGolemCube _golemCube;
    [SerializeField] private DVGolemInfo _curGolemInfo;

    private Dictionary<Vector3Int, DVGolemCube> _childs = new Dictionary<Vector3Int, DVGolemCube>();

    private Rigidbody _rb;
    private DVGolemController _golemController;
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

        OnCoreCollision(collision);
    }
    #endregion

    #region Public Functions
    public void SetInit() {
        _rb = GetComponent<Rigidbody>();
        _rb.Reset();
        _rb.UseAngular(false);

        SetGolemMass();
    }

    public void RemoveCube(DVGolemCube golemCube)
    {
        _curGolemInfo.RemoveCube(golemCube.CubeInfo.ShapePosition);
        
        var key = golemCube.CubeInfo.ShapePosition;
        if (_childs.ContainsKey(key)) {
            _childs.Remove(key);
        }

        SetGolemMass();
    }

    public void SetGolemInfo(DVGolemInfo golemInfo) { 
        _golemInfo = golemInfo;
        _curGolemInfo = new DVGolemInfo(_golemInfo);
    }

    public void SetupChilds() {
        _childs.Clear();

        foreach (var child in GetComponentsInChildren<Transform>()) { 
            var golemCube = child.GetComponent<DVGolemCube>();
            if (golemCube == null) {
                Debug.LogError($"{child.name} doesn't have DVGolemCube");
                continue;
            }

            _childs[golemCube.CubeInfo.ShapePosition] = golemCube;
        }
        SetGolemMass();
    }

    public void SetGolemController(DVGolemController golemController) {
        _golemController = golemController;
    }

    public void SetAttackMode(bool on) { 
        _golemCube.SetAttackMode(on);
    }

    public DVGolemCube FindCube(Vector3Int cubePos)
    {
        if(_childs.TryGetValue(cubePos, out var child))
            return child;
        return null;
    }
    #endregion

    #region Utils
    private void OnCoreCollision(Collision collision) {
        var core = collision.gameObject.GetComponent<DVGolemCore>();
        var obstacle = collision.gameObject.GetComponent<DVObstacleCube>();

        Vector3 normalAVG = Vector3.zero;
        foreach (var contact in collision.contacts)
            normalAVG += contact.normal;
        normalAVG.Normalize();

        // TODO: Skill Collision

        if (core != null) // Golem Collision (다른 곳에서 호출, 먼저 호출하면 다른 쪽 호출은 무시)
        { 

        }
        else if (obstacle != null) // Obstacle Collision
        {
            float maxDamageRate = 0f;
            ActOnChildCollsion(collision, (child) =>
            {
                Vector3 impulse = DVUtil.EstimateImpulse(_golemController.MoveVelocity, _rb.mass, obstacle.Velocity, obstacle.CubeMass, normalAVG);
                if(impulse.magnitude <= collision.impulse.magnitude)
                    impulse = -collision.impulse;

                DVHelper.Instance.WaitFrameAct(1, () =>
                {
                    if (child.Usable())
                    {
                        child.OnDamaged(_rb.mass, -impulse, obstacle.CubeInfo, out float damageRate);
                        maxDamageRate = Mathf.Max(maxDamageRate, damageRate);
                    }
                });

                DVHelper.Instance.WaitFrameAct(1, () =>
                {
                    if (obstacle.Usable())
                    {
                        obstacle.OnDamaged(impulse, child.CubeInfo);
                    }
                });
            });

            Vector3 impulse = collision.impulse + collision.impulse.normalized * maxDamageRate;
            _golemController.OnImpulse(impulse);
        }

        DVHelper.Instance.WaitFrameAct(1, () => 
        { 
            if(this.Usable())
                SetGolemMass(); 
        });
    }

    private void ActOnChildCollsion(Collision collision, Action<DVGolemCube> onCollisionCallback) {
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
        }
    }

    private void SetGolemMass() {
        float mass = 0f;
        foreach (var child in _childs.Values) {
            mass += child.CubeMass;
        }
        _rb.mass = mass;
    }
    #endregion
}
