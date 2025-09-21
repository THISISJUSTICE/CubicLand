using System;

namespace CustomTIJI.CubicLand
{
    [Serializable]
    public abstract class GolemSkill
    {
        protected GolemController _owner;
        protected GolemCore _ownerCore;

        public abstract float DelayTime { get; }

        public GolemSkill(GolemController owner)
        {
            _owner = owner;
            _ownerCore = owner.GetComponent<GolemCore>();
        }

        public abstract GolemSkill Clone();

        public abstract void KeyDown();

        public abstract void KeyUp();

        public abstract void Cancel();
    }
}