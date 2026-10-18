using Commar.CubicLand.Cube;
using UnityEngine;

namespace Commar.CubicLand.Game
{
    // TODO: 추후 보완
    // Golem 스킬, 행동 등에 따라 카메라 혹은 Follow 로직 변경
    // 벽 등 장애물에 가려지거나 카메라가 이동하면 안 되는 부분 보완
    public class GolemFollower : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        private GameObject _target;
        private IGolemGeometryProvider _golemGeometry;

        public void Initialize(GameObject target, IGolemGeometryProvider golemGeometry)
        {
            _target = target;
            _golemGeometry = golemGeometry;
        }

        private void LateUpdate()
        {
            if (_target == null || _golemGeometry == null)
                return;

            transform.rotation = _golemGeometry.ViewRotation;

            Vector3 backPos = CalculateBackPosition();
            Vector3 upPos = CalculateUpPosition();
            Vector3 targetPos = backPos + upPos * 0.8f - transform.forward * CalculateFollowDistance();

            transform.position = targetPos;
            transform.LookAt(backPos + upPos * 0.5f);
        }

        private Vector3 CalculateBackPosition()
        {
            if (_golemGeometry == null)
                return Vector3.zero;

            float backDist = Utils.GetDistanceXZ(_golemGeometry.BackEdgeCube.transform.position, transform.position);
            float upDist = Utils.GetDistanceXZ(_golemGeometry.UpEdgeCube.transform.position, transform.position);

            Vector3 position = backDist < upDist ? _golemGeometry.BackEdgeCube.transform.position : _golemGeometry.UpEdgeCube.transform.position;
            position.y = _golemGeometry.RotateAxisCube.transform.position.y;

            return position;
        }

        private Vector3 CalculateUpPosition()
        {
            if (_golemGeometry == null)
                return Vector3.zero;

            return Vector3.up * _golemGeometry.GolemHeight;
        }

        private float CalculateFollowDistance()
        {
            if (_golemGeometry == null)
                return 0f;

            float maxWidth = Mathf.Max(_golemGeometry.GolemHeight, _golemGeometry.GolemWidth);
            float distance = Mathf.Max(Utils.GetBaseLineHA(maxWidth, _camera.fieldOfView), _golemGeometry.GolemBack + 1f);

            return distance * 1.5f + 1f;
        }
    }
}