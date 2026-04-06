using System;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public interface IHitNotifier
    {
        public event Action<GameObject, Collision> onHit;
    }
}