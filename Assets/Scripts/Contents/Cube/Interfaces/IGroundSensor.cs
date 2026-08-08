using System;
using UnityEngine;

namespace Commar.CubicLand.Cube
{
    public interface IGroundSensor
    {
        public bool IsGrounded { get; }
        public float GroundDistance { get; }

        public Collider GroundedCollider { get; }
        public Rigidbody GroundedRigidbody { get; }

        public event Action OnGrounded;

        public void NotifyAirborne();
    }
}