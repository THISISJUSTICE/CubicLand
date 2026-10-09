using UnityEngine;

namespace Commar.CubicLand.UI
{
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 240f;

        private void Update()
        {
            transform.Rotate(0f, 0f, -_rotationSpeed * Time.unscaledDeltaTime);
        }
    }
}