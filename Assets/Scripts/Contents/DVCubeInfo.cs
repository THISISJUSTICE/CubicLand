using UnityEngine;

[System.Serializable]
public struct DVCubeInfo
{
    public DVStatus Status;
    public bool AttackMode;

    private bool _isCore;
    public bool IsCore { get { return _isCore; } }
    private Vector3Int _shapePosition;
    public Vector3Int ShapePosition { get { return _shapePosition; } }

    public DVCubeInfo(DVStatus status, bool isCore, Vector3Int shapePosition)
    {
        Status = status;
        _isCore = isCore;
        _shapePosition = shapePosition;
        AttackMode = false;
    }

    public void DetachParent() {
        _shapePosition = Vector3Int.zero;
    }
}

[System.Serializable]
public struct DVStatus
{
    public int HP;
    public int Armor;
    public int Attack;
    public Color Color;

    public void SetInitValue() {
        HP = 10;
        Armor = 10;
        Attack = 10;
        Color = Color.white;
    }

    public void OnDamage(DVStatus attackCube, int maxHP) { 

    }
}
