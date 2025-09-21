using System;

namespace CustomTIJI.CubicLand
{
    [Serializable]
    public abstract class GolemSkillSummon : GolemSkill
    {
        public const float MAX_MOVE_TIME = 0.1f;
        public const float MIN_MOVE_TIME = 0.01f;
        public const float INIT_MOVE_TIME = 0.05f;

        public GolemSkillSummon(GolemController owner) : base(owner)
        {

        }
    }
}