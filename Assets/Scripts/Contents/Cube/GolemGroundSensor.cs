using System;
using UnityEngine;

namespace Commar.CubicLand.Cube
{
    [RequireComponent(typeof(GolemCore))]
    public class GolemGroundSensor : MonoBehaviour, IGroundSensor
    {
        private IGolemObject _golemObject;

        public bool IsGrounded => throw new NotImplementedException();

        public float GroundDistance => throw new NotImplementedException();

        public Collider GroundedCollider => throw new NotImplementedException();

        public Rigidbody GroundedRigidbody => throw new NotImplementedException();

        public event Action OnGrounded;

        private void Awake()
        {
            _golemObject = GetComponent<GolemCore>();
        }

        public void NotifyAirborne()
        {
            throw new NotImplementedException();
        }
    }
}