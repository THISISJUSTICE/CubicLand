using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public interface ICubeCollisionResolver
    {
        public void OnCollision(GameObject gameObject, Collision collision);
    }
}