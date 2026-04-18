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
            set => SetHP(value);
        }

        public int Armor
        {
            get => _armor;
            set => SetArmor(value);
        }

        public int Attack
        {
            get => _attack;
            set => SetAttack(value);
        }

        public void SetHP(int value) => _hp = Mathf.Max(value, 0);
        public void SetArmor(int value) => _armor = Mathf.Max(value, 0);
        public void SetAttack(int value) => _attack = Mathf.Max(value, 0);

        public StatusPoint(int hp, int armor, int attack)
        { 
            _hp = hp;
            _armor = armor;
            _attack = attack;
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

        internal StatusValue(StatusPoint statusPoint)
        {
            MaxHP = CubeConfig.Status.INIT_HP + statusPoint.HP * CubeConfig.Status.ADD_HP;
            HP = MaxHP;
            Armor = CubeConfig.Status.INIT_ARMOR + statusPoint.Armor * CubeConfig.Status.ADD_ARMOR;
            Attack = CubeConfig.Status.INIT_ATTACK + statusPoint.Attack * CubeConfig.Status.ADD_ATTACK;
        }

        public bool IsFullHP()
        { 
            return HP == MaxHP;
        }

        internal void EnhanceStatus(StatusPoint statusPoint)
        {
            int previousMaxHP = MaxHP;
            int previousHP = HP;
            this = new StatusValue(statusPoint);

            HP = Mathf.Min(MaxHP, previousHP + MaxHP - previousMaxHP);
        }

        internal void ApplyDamage(int damage)
        {
            HP = Mathf.Max(HP - damage, 0);
        }

        internal void Heal(int heal)
        {
            HP = Mathf.Min(MaxHP, HP + heal);
        }
    }
}