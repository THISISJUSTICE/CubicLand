using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public struct StatusPoint
    {
        private int _hp;
        private int _armor;
        private int _attack;

        public int HP
        {
            get => _hp;
            set => _hp = Mathf.Max(value, 0);
        }

        public int Armor
        {
            get => _armor;
            set => _armor = Mathf.Max(value, 0);
        }

        public int Attack
        {
            get => _attack;
            set => _attack = Mathf.Max(value, 0);
        }

        public void Initialize()
        {
            HP = 0;
            Armor = 0;
            Attack = 0;
        }

        public StatusPoint MakeChildStatus()
        {
            StatusPoint childStatusPoint = new StatusPoint();
            childStatusPoint.HP = HP - 1;
            childStatusPoint.Armor = Armor - 1;
            childStatusPoint.Attack = Attack - 1;

            return childStatusPoint;
        }
    }

    public struct StatusValue
    {
        public int MaxHP { get; private set; }
        public int HP { get; private set; }
        public int Armor { get; private set; }
        public int Attack { get; private set; }

        internal void Initialize(StatusPoint statusPoint)
        {
            MaxHP = CubeConfig.Status.INIT_HP + statusPoint.HP * CubeConfig.Status.ADD_HP;
            HP = MaxHP;
            Armor = CubeConfig.Status.INIT_ARMOR + statusPoint.Armor * CubeConfig.Status.ADD_ARMOR;
            Attack = CubeConfig.Status.INIT_ATTACK + statusPoint.Attack * CubeConfig.Status.ADD_ATTACK;
        }

        internal void EnhanceStatus(StatusPoint statusPoint)
        {
            int previousMaxHP = MaxHP;
            int previousHP = HP;
            Initialize(statusPoint);

            HP = Mathf.Min(MaxHP, previousHP + MaxHP - previousMaxHP);
        }

        internal void OnDamaged(int rawDamage, float selfMass, Vector3 impulse)
        {
            // 충격량에 따라 데미지 비율 변경
            float damage = Mathf.Round(rawDamage * 
                Mathf.Clamp01(impulse.magnitude / CubeConfig.DAMAGE_IMPULSE_RATE));

            // 물리 충격량에 따른 추가 데미지 반영
            // 기본 데미지의 2배를 넘지 못하도록 제한
            damage += Mathf.Min(rawDamage * 2f, 
                impulse.magnitude / selfMass * CubeConfig.IMPULSE_DAMAGE_RATE);

            // 방어 스텟에 따른 데미지 감소
            damage *= Mathf.Exp(-Armor / CubeConfig.Status.DAMAGE_ARMOR_RATE);

            // 최소 데미지 제한
            damage = Mathf.Max(damage, 1f);

            // 데미지 반영
            HP = Mathf.Max(HP - Mathf.RoundToInt(damage), 0);
        }

        internal int OnHealed(int heal)
        {
            if (HP + heal < MaxHP)
            {
                HP += heal;
                return 0;
            }

            int rest = heal - (MaxHP - HP);
            HP = MaxHP;
            return rest;
        }
    }
}