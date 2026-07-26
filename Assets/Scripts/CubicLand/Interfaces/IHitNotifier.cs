using System;
using UnityEngine;

namespace Commar.CubicLand.Cube
{
    public interface IHitNotifier
    {
        public event Action<GameObject, Collision> onHit;
    }
}