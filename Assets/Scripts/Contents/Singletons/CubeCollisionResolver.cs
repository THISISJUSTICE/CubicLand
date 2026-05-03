using CustomTIJI.CubicLand.Cube;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand.Singletons
{
    public class CubeCollisionResolver : ICubeCollisionResolver, IHitNotifier, IFixedUpdatable
    {
        private readonly HashSet<(GameObject, GameObject)> _hitPairs = new HashSet<(GameObject, GameObject)>();

        private Action _defferedCollisionAction;
        public event Action<GameObject, Collision> onHit;

        public void FixedUpdate()
        {
            _defferedCollisionAction?.Invoke();
            _defferedCollisionAction = null;
            _hitPairs.Clear();
        }

        public void OnCollision(GameObject gameObject, ICubeMotionAdjuster motionAdjuster, Collision collision, bool onlyCubeCollision)
        {
            CubeObject cubeObject = gameObject.GetComponent<CubeObject>();
            if (cubeObject == null)
                return;

            Vector3 impulse = -collision.impulse;
            CubeObject collisionCube = collision.gameObject.GetComponent<CubeObject>();

            if (collisionCube == null)
            {
                if (!onlyCubeCollision)
                {
                    _defferedCollisionAction += () =>
                    {
                        cubeObject.OnDamaged(cubeObject.Mass, impulse, null);
                        motionAdjuster.ApplyKnockback(impulse);
                    };
                }
            }
            else
            {
                _defferedCollisionAction += () =>
                {
                    cubeObject.OnDamaged(cubeObject.Mass, impulse, collisionCube.CubeData);
                    motionAdjuster.ApplyKnockback(impulse);
                };
            }

            _defferedCollisionAction += () =>
            {
                (GameObject, GameObject) pair = MakePair(gameObject, collision.gameObject);
                if (!_hitPairs.Contains(pair))
                {
                    _hitPairs.Add(pair);
                    onHit?.Invoke(gameObject, collision);
                }
            };
        }

        private (GameObject, GameObject) MakePair(GameObject a, GameObject b)
        {
            if (a.GetInstanceID().CompareTo(b.GetInstanceID()) < 0)
                return (a, b);
            else
                return (b, a);
        }
    }
}