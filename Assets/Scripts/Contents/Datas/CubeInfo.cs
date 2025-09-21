using UnityEngine;

namespace CustomTIJI.CubicLand
{
    [System.Serializable]
    public struct CubeInfo
    {
        [SerializeField] public Status Status;
        [SerializeField] public bool AttackMode;
        [SerializeField] public CurrentStatus CurrentStatus;

        [SerializeField] private bool _isCore;
        public bool IsCore { get { return _isCore; } }

        [SerializeField] private Vector3Int _shapePosition;
        public Vector3Int ShapePosition { get { return _shapePosition; } }

        public CubeInfo(Status status, bool isCore, Vector3Int shapePosition)
        {
            Status = status;
            _isCore = isCore;
            _shapePosition = shapePosition;
            AttackMode = false;
            CurrentStatus = new CurrentStatus();
            CurrentStatus.SetInitValue(status);
        }

        public void SetChildValue(Status status)
        {
            Status.SetChildValue(status);
            CurrentStatus.SetInitValue(Status);
        }

        public void EnhanceStatus(Status status)
        {
            Status = status;
            CurrentStatus.EnhanceStatus(status);
        }

        public void DetachParent()
        {
            _shapePosition = Vector3Int.zero;
        }
    }
}