using UnityEngine;

namespace CustomTIJI.CubicLand
{
    [System.Serializable]
    public struct Status
    {
        [SerializeField] public int Point_HP;
        [SerializeField] public int Point_Armor;
        [SerializeField] public int Point_Attack;
        [SerializeField] private Color _color;

        public Color Color { get => _color; }

        public Status(int point_hp = 0, int point_armor = 0, int point_attack = 0)
        {
            Point_HP = Mathf.Max(point_hp, 0);
            Point_Armor = Mathf.Max(point_armor, 0);
            Point_Attack = Mathf.Max(point_attack, 0);
            _color = Color.white;
        }

        public void SetInitValue()
        {
            Point_HP = 0;
            Point_Armor = 0;
            Point_Attack = 0;
            _color = Color.white;
        }

        public void SetChildValue(Status status)
        {
            Point_HP = Mathf.Max(status.Point_HP - 1, 0);
            Point_Armor = Mathf.Max(status.Point_Armor - 1, 0);
            Point_Attack = Mathf.Max(status.Point_Attack - 1, 0);
            _color = status._color * StatusConfig.COLOR_CHILD_RATE;
            _color.Clamp(status._color * StatusConfig.COLOR_CHILD_RATE, Color.white);
        }

        public Status GetChildStatus()
        {
            Status newStatus = new Status(0, 0, 0);
            newStatus.SetChildValue(this);

            return newStatus;
        }
    }

    [System.Serializable]
    public struct CurrentStatus
    {
        [SerializeField] private int _hp;
        [SerializeField] private int _armor;
        [SerializeField] private int _attack;
        [SerializeField] private int _pointHP;

        public int HP { get => _hp; }
        public int MaxHP { get => StatusConfig.INIT_HP + _pointHP * StatusConfig.ADD_HP; }
        public int Armor { get => _armor; }
        public int Attack { get => _attack; }

        public void SetInitValue(Status status)
        {
            _pointHP = status.Point_HP;
            _hp = MaxHP;
            _armor = StatusConfig.INIT_ARMOR + status.Point_Armor * StatusConfig.ADD_ARMOR;
            _attack = StatusConfig.INIT_ATTACK + status.Point_Attack * StatusConfig.ADD_ATTACK;
        }

        public void EnhanceStatus(Status status)
        {
            int prevMaxHP = MaxHP;
            _pointHP = status.Point_HP;
            _hp += MaxHP - prevMaxHP;
            _armor = StatusConfig.INIT_ARMOR + status.Point_Armor * StatusConfig.ADD_ARMOR;
            _attack = StatusConfig.INIT_ATTACK + status.Point_Attack * StatusConfig.ADD_ATTACK;
        }

        public void OnDamaged(float selfMass, Vector3 impulse, CubeInfo colCubeInfo)
        {
            float cubeHP = colCubeInfo.CurrentStatus.HP;
            float cubeMaxHP = colCubeInfo.CurrentStatus.MaxHP;
            float cubeArmor = colCubeInfo.CurrentStatus.Armor;
            float cubeAttack = colCubeInfo.AttackMode ? colCubeInfo.CurrentStatus.Attack : 0f;
            int damage = Mathf.RoundToInt(cubeHP / cubeMaxHP * cubeArmor + cubeAttack);

            OnDamaged(damage, selfMass, impulse);
        }

        public void OnDamaged(float selfMass, Vector3 impulse)
        {
            OnDamaged(1, selfMass, impulse);
        }

        private void OnDamaged(int damage, float selfMass, Vector3 impulse)
        {
            const float damageImpulseRate = 6f;
            float calDamage = damage;
            calDamage = Mathf.RoundToInt(calDamage * Mathf.Clamp01(impulse.magnitude / damageImpulseRate)); // 충격량에 따라 데미지 비율 변경

            float impulseDamage = impulse.magnitude / selfMass * Configs.IMPULSE_DAMAGE_RATE; // 물리 충격량 반영
            calDamage = calDamage + Mathf.Min(calDamage * 2f, impulseDamage); // 물리 충격량이 데미지의 2배를 넘지 못하도록 제한
            calDamage = calDamage * Mathf.Exp((float)-_armor / StatusConfig.DAMAGE_ARMOR_RATE); // 방어 스텟 반영
            calDamage = Mathf.Max(calDamage, 1f); // 데미지 최솟값 제한

            damage = Mathf.RoundToInt(calDamage);
            _hp = Mathf.Max(_hp - damage, 0); // 데미지 반영
        }

        public void SetAttackOff()
        {
            _attack = 0;
        }

        // TODO: 남은 힐 량은 자식에게 전이
        public int OnHealed(int heal)
        {
            _hp = Mathf.Max(_hp + heal, MaxHP);
            int rest = heal - _hp;
            return Mathf.Max(rest, 0);
        }
    }
}