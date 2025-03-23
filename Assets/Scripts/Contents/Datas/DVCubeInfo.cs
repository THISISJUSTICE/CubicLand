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
        Point_HP = Mathf.Max(status.Point_HP - 1, 0);
        Point_Armor = Mathf.Max(status.Point_Armor - 1, 0);
        Point_Attack = Mathf.Max(status.Point_Attack - 1, 0);
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

    public void OnDamaged(float selfMass, Vector3 impulse, DVCubeInfo colCubeInfo) { 
        float cubeHP = colCubeInfo.Status.CurrentStatus.HP;
        float cubeMaxHP = colCubeInfo.Status.CurrentStatus.MaxHP;
        float cubeArmor = colCubeInfo.Status.CurrentStatus.Armor;
        float cubeAttack = colCubeInfo.AttackMode ? colCubeInfo.Status.CurrentStatus.Attack : 0f;
        int damage = Mathf.RoundToInt(cubeHP / cubeMaxHP * cubeArmor + cubeAttack);

        OnDamaged(damage, selfMass, impulse);
    }

    public void OnDamaged(float selfMass, Vector3 impulse)
    {
        OnDamaged(1, selfMass, impulse);
    }

    private void OnDamaged(int damage, float selfMass, Vector3 impulse) {
        const float damageImpulseRate = 6f;
        float calDamage = damage;
        calDamage = Mathf.RoundToInt(calDamage * Mathf.Clamp01(impulse.magnitude / damageImpulseRate)); // 충격량에 따라 데미지 비율 변경

        float impulseDamage = impulse.magnitude / selfMass * DVConfigs.IMPULSE_DAMAGE_RATE; // 물리 충격량 반영
        calDamage = calDamage + Mathf.Min(calDamage * 2f, impulseDamage); // 물리 충격량이 데미지의 2배를 넘지 못하도록 제한
        calDamage = calDamage * Mathf.Exp((float)-_armor / DVStatusConfig.DAMAGE_ARMOR_RATE); // 방어 스텟 반영
        calDamage = Mathf.Max(calDamage, 1f); // 데미지 최솟값 제한

        damage = Mathf.RoundToInt(calDamage);
        _hp = Mathf.Max(_hp - damage, 0); // 데미지 반영
    }

    public void SetAttackOff() {
        _attack = 0;
    }

    // TODO: 남은 힐 량은 자식에게 전이
    public int OnHealed(int heal) {
        _hp = Mathf.Max(_hp + heal, MaxHP);
        int rest = heal - _hp;
        return Mathf.Max(rest, 0);
    }
}
