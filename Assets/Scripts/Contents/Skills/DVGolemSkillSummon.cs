using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class DVGolemSkillSummon : DVGolemSkill
{
    public const float MAX_MOVE_TIME = 0.1f;
    public const float MIN_MOVE_TIME = 0.01f;
    public const float INIT_MOVE_TIME = 0.05f;

    public DVGolemSkillSummon(DVGolemController owner) : base(owner)
    { 

    }
}
