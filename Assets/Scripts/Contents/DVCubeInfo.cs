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

    private const int MIN_VALUE = 10;
    private const float CHILD_RATE = 0.9f;

    public void SetInitValue() {
        HP = MIN_VALUE;
        Armor = MIN_VALUE;
        Attack = MIN_VALUE;
        Color = Color.white;
    }

    public void SetChildValue(DVStatus status) { 
        HP = Mathf.RoundToInt(Mathf.Max(Mathf.RoundToInt(status.HP * CHILD_RATE), MIN_VALUE));
        Armor = Mathf.RoundToInt(Mathf.Max(Mathf.RoundToInt(status.Armor * CHILD_RATE), MIN_VALUE));
        Attack = Mathf.RoundToInt(Mathf.Max(Mathf.RoundToInt(status.Attack * CHILD_RATE), MIN_VALUE));
        Color = status.Color * (2f - CHILD_RATE);
        Color.Clamp(status.Color * (2f - CHILD_RATE), Color.white);
    }

    public void OnDamage(DVStatus attackCube, int maxHP) { 

    }
}
