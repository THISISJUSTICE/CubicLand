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
    public int Point_HP;
    public int Point_Armor;
    public int Point_Attack;
    private Color _color;
    public Color Color { get => _color; }
    public DVCurrentStatus CurrentStatus;

    public DVStatus(int point_hp = 0, int point_armor = 0, int point_attack = 0) {
        Point_HP = Mathf.Max(point_hp, 0);
        Point_Armor = Mathf.Max(point_armor, 0);
        Point_Attack = Mathf.Max(point_attack, 0);
        _color = Color.white;
        CurrentStatus = new DVCurrentStatus();
        CurrentStatus.SetInitValue(this);
    }

    public void SetInitValue() { 
        Point_HP = 0;
        Point_Armor = 0;
        Point_Attack = 0;
        _color = Color.white;
        CurrentStatus = new DVCurrentStatus();
        CurrentStatus.SetInitValue(this);
    }

    public void SetChildValue(DVStatus status) { 
        Point_HP = Mathf.Max(Point_HP - 1, 0);
        Point_Armor = Mathf.Max(Point_Armor - 1, 0);
        Point_Attack = Mathf.Max(Point_Attack - 1, 0);
        _color = status._color * DVStatusConfig.COLOR_CHILD_RATE;
        _color.Clamp(status._color * DVStatusConfig.COLOR_CHILD_RATE, Color.white);
        CurrentStatus.SetInitValue(this);
    }
}

[System.Serializable]
public struct DVCurrentStatus {
    [SerializeField] private int _hp;
    [SerializeField] private int _armor;
    [SerializeField] private int _attack;
    [SerializeField] private int _pointHP;

    public int HP { get => _hp; }
    public int MaxHP { get => DVStatusConfig.INIT_HP + _pointHP * DVStatusConfig.ADD_HP; }
    public int Armor { get => _armor; }
    public int Attack { get => _attack; }

    public void SetInitValue(DVStatus status) {
        _pointHP = status.Point_HP;
        _hp = MaxHP;
        _armor = DVStatusConfig.INIT_ARMOR + status.Point_Armor * DVStatusConfig.ADD_ARMOR;
        _attack = DVStatusConfig.INIT_ATTACK + status.Point_Attack * DVStatusConfig.ADD_ATTACK;
    }

    public void OnDamaged(DVCubeInfo cubeInfo) { 
        // TODO: 물리, 속도 계산 추가

        int cubeHP = cubeInfo.Status.CurrentStatus.HP;
        int cubeMaxHP = cubeInfo.Status.CurrentStatus.MaxHP;
        int cubeArmor = cubeInfo.Status.CurrentStatus.Armor;
        int cubeAttack = cubeInfo.AttackMode ? cubeInfo.Status.CurrentStatus.Attack : 0;
        int damage = Mathf.RoundToInt((float)cubeHP / (float)cubeMaxHP * (float)cubeArmor) + cubeAttack;
        damage = Mathf.RoundToInt((float)damage * Mathf.Exp((float)-_armor / DVStatusConfig.DAMAGE_ARMOR_RATE));
        damage = Mathf.RoundToInt(Mathf.Max(damage, 1f));

        _hp = Mathf.RoundToInt(Mathf.Max(_hp - damage, 0f));
    }

    public void SetAttackOff() {
        _attack = 0;
    }

    public void OnHealed(int heal) {
        _hp = Mathf.RoundToInt(Mathf.Max(_hp + heal, MaxHP));
    }
}
