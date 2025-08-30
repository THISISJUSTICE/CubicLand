using System;
using UnityEngine;

[Serializable]
public abstract class DVGolemSkill
{
    protected DVGolemController _owner;
    protected DVGolemCore _ownerCore;

    public abstract float DelayTime { get;}

    public DVGolemSkill(DVGolemController owner)
    {
        _owner = owner;
        _ownerCore = owner.GetComponent<DVGolemCore>();
    }

    public abstract DVGolemSkill Clone();

    public abstract void KeyDown();

    public abstract void KeyUp();

    public abstract void Cancel();
}
