using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Commar.CubicLand.Cube
{
    internal class GolemMotionMotor : IOnEnablable, IGolemGeometryProvider
    {
        private readonly IGolemObject _golemObject;
        private GolemMoveDirection _moveDirection;

        private float _chargedHeight = 0f;

        private readonly Vector3[] _objectDirections = new Vector3[6];

        private readonly List<Vector3Int> _tempList = new List<Vector3Int>();
        private readonly Dictionary<Enums.Direction3D, Vector3Int> _tempDictionary = new Dictionary<Enums.Direction3D, Vector3Int>();
        private readonly List<CubeData> _edgeCubes = new List<CubeData>();

        private static readonly Enums.Direction3D[] PRIORITY_CANDIDATES = new Enums.Direction3D[]
        {
            Enums.Direction3D.Front,
            Enums.Direction3D.Right,
            Enums.Direction3D.Back,
            Enums.Direction3D.Left
        };
        private static readonly Enums.Direction3D[] REVERSE_PRIORITY_CANDIDATES = new Enums.Direction3D[]
        {
            Enums.Direction3D.Back,
            Enums.Direction3D.Left,
            Enums.Direction3D.Right,
            Enums.Direction3D.Front
        };

        public CubeObject RotateAxisCube { get; private set; }
        public CubeObject UpEdgeCube { get; private set; }
        public CubeObject BackEdgeCube { get; private set; }
        public CubeObject FrontEdgeCube { get; private set; }

        public int GolemWidth { get; private set; }
        public int GolemHeight { get; private set; }
        public int GolemBack { get; private set; }

        public Quaternion ViewRotation { get; private set; }

        private Transform Transform => _golemObject.Rigidbody.transform;

        internal GolemMotionMotor(IGolemObject golemObject)
        {
            _golemObject = golemObject;
        }

        public void OnEnable()
        {
            _chargedHeight = 0;

            _moveDirection = new GolemMoveDirection(Transform.forward, Transform.right, Transform.up);
            UpdateGeometryData();
        }

        public void FindEdgeCubeDatas(Enums.Direction3D direction, IList<CubeData> cubeDatas)
        {
            _golemObject.GolemData.FindEdgeCubes(ConvertObjectDirection(direction), cubeDatas);
        }

        public bool ReleaseJumpForce()
        {
            if (_chargedHeight > 0f && !_golemObject.Rigidbody.IsUnderGravity())
            {
                _golemObject.Rigidbody.AddForce(Vector3.up * CubeUtil.CalculateLiftForce(_golemObject.Rigidbody, _chargedHeight), ForceMode.Impulse);
                _chargedHeight = 0f;
                return true;
            }

            return false;
        }

        public IEnumerator MoveWithRoll(Enums.Direction direction, float duration)
        {
            Vector3 moveDirection = _moveDirection.GetDirection(direction).normalized;
            float halfLine = CubeConfig.CUBE_BASE_LENGHT / 2f;

            int currentDownLength = GetGolemLength(Enums.Direction3D.Down);
            if (currentDownLength == 0)
                currentDownLength = 1;
            int nextDownLength = GetGolemLength(DirectionEnumUtils.ConvertDirection2DTo3D(direction));

            float currentHeight = (currentDownLength * 2f + 1f) * halfLine;
            float nextHeight = (nextDownLength * 2f + 1f) * halfLine;
            float rollAxisAngle = Utils.GetAngleBH(nextHeight, currentHeight);
            float rollHypot = Utils.GetHypotenuseBH(nextHeight, currentHeight);
            float previousAngle = 0f;

            Quaternion startRotation = Transform.rotation;
            Quaternion targetRotation = Quaternion.FromToRotation(moveDirection, _moveDirection.Down) * startRotation;
            float time = 0f;

            bool once = true;

            while (time < duration)
            {
                _golemObject.Rigidbody.MoveRotation(Quaternion.Slerp(startRotation, targetRotation, time / duration));
                float angle = Quaternion.Angle(startRotation, Transform.rotation);

                Transform.position += Vector3.up * (Utils.GetHeightLineHyA(rollHypot, angle + rollAxisAngle)
                    - Utils.GetHeightLineHyA(rollHypot, previousAngle + rollAxisAngle));
                Transform.position += -moveDirection * (Utils.GetBaseLineHyA(rollHypot, angle + rollAxisAngle)
                    - Utils.GetBaseLineHyA(rollHypot, previousAngle + rollAxisAngle));

                if (angle > 50f && once)
                {
                    once = false;
                    UpdateGeometryData();
                }

                time += Time.fixedDeltaTime;
                yield return YieldCache.WaitForFixedUpdate;
            }

            _golemObject.Rigidbody.MovePosition(CubeUtil.GetNormalizedPosition(Transform.position));
            _golemObject.Rigidbody.MoveRotation(CubeUtil.GetNormalizedRotation(Transform.rotation));
        }

        public IEnumerator Tumble(Enums.Direction direction, float duration)
        { 
            // TODO: 구현
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 바닥에 붙었을 때만 정상 동작함
        /// </summary>
        public IEnumerator MoveWithJump(Enums.Direction direction, float duration)
        {
            yield return MoveWithJumpCoroutine(direction, duration, CubeConfig.CUBE_BASE_LENGHT * 0.5f);
        }

        public IEnumerator Move(Enums.Direction direction, float duration)
        {
            yield return MoveWithJumpCoroutine(direction, duration, 0f);
        }

        public IEnumerator Rotate(Enums.Direction direction, float duration)
        {
            RotateAxisCube = _golemObject.FindCube(FindRotateAxis(direction));

            Quaternion startRotation = Transform.rotation;
            Vector3 moveDirection = _moveDirection.GetDirection(direction);
            Vector3 axisPosition = RotateAxisCube.transform.position;
            axisPosition.y = 0f;
            Quaternion moveRotation = Quaternion.FromToRotation(_moveDirection.Front, moveDirection);
            Quaternion targetRotation = startRotation * moveRotation;

            Quaternion startViewRotation = ViewRotation;
            Quaternion targetViewRotation = startViewRotation * moveRotation;

            float time = 0f;
            bool once = true;

            while (time < duration)
            {
                time += Time.fixedDeltaTime;

                float ratio = Mathf.Min(1f, time / duration);

                _golemObject.Rigidbody.MoveRotation(Quaternion.Slerp(startRotation, targetRotation, ratio));
                ViewRotation = Quaternion.Slerp(startViewRotation, targetViewRotation, ratio);

                if (ratio > 0.55f && once)
                {
                    once = false;
                    _moveDirection.Rotate(moveRotation.eulerAngles);
                }
                
                Vector3 positionOffset = axisPosition - RotateAxisCube.transform.position;
                positionOffset.y = 0f;
                Transform.position += positionOffset;

                yield return YieldCache.WaitForFixedUpdate;
            }

            ViewRotation = _moveDirection.GetRotation();
        }

        public IEnumerator ChargeJump()
        {
            if (_chargedHeight > 0f)
                yield break;

            Utils.GetClosestAxisVector(Transform.GetDirections(), Vector3.up, out int index);
            index /= 2;
            Vector3 scaleDirection = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward }[index];

            float golemHeight = GolemHeight * CubeConfig.CUBE_BASE_LENGHT;
            float maxHeight = CubeConfig.CUBE_BASE_LENGHT / 2f + (GolemHeight - 1) * CubeConfig.CUBE_BASE_LENGHT;
            Vector3 previousScale = Transform.localScale;

            float duration = GolemHeight * CubeConfig.GOLEM_JUMP_CHARGE_TIME;
            float time = 0f;

            while (time < duration)
            {
                float ratio = Mathf.Min(1f, time / duration);

                Transform.localScale = Vector3.one - scaleDirection * ratio / 2f;
                Transform.position += (Transform.localScale[index] - previousScale[index]) * golemHeight / 2f * Vector3.up;
                _chargedHeight = Mathf.Max(maxHeight * ratio, CubeConfig.CUBE_BASE_LENGHT / 2f);

                previousScale = Transform.localScale;

                time += Time.fixedDeltaTime;
                yield return YieldCache.WaitForFixedUpdate;
            }
        }

        public IEnumerator RestoreScale()
        {
            if (Transform.localScale == Vector3.one)
                yield break;

            Vector3 startScale = Transform.localScale;
            Utils.GetClosestAxisVector(Transform.GetDirections(), Vector3.up, out int index);
            index /= 2;
            float golemHeight = GolemHeight * CubeConfig.CUBE_BASE_LENGHT;

            float scaleRate = (1f - startScale[index]) / 0.5f;
            float duration = CubeConfig.GOLEM_SIZE_UP_TIME * scaleRate;
            float time = 0f;
            float previousAxisScale = startScale[index];

            while (time < duration)
            {
                time += Time.fixedDeltaTime;
                float ratio = Mathf.Min(1f, time / duration);
                Vector3 scale = Vector3.Lerp(startScale, Vector3.one, ratio);

                Transform.position += (scale[index] - previousAxisScale) * golemHeight / 2f * Vector3.up;
                Transform.localScale = scale;
                previousAxisScale = scale[index];

                yield return YieldCache.WaitForFixedUpdate;
            }

            Transform.position += (1f - previousAxisScale) * golemHeight / 2f * Vector3.up;
            Transform.localScale = Vector3.one;
        }

        private IEnumerator MoveWithJumpCoroutine(Enums.Direction direction, float duration, float moveHeight)
        {
            Vector3 moveDirection = _moveDirection.GetDirection(direction).normalized;
            Vector3 startPosition = Transform.position;
            Vector3 targetPosition = startPosition + moveDirection * CubeConfig.CUBE_BASE_LENGHT;

            float time = 0f;

            while (time < duration)
            {
                time += Time.fixedDeltaTime;
                float ratio = Mathf.Min(1f, time / duration);
                float heightRatio = ratio < 0.5f ? ratio * 2f : (1f - ratio) * 2f;

                Vector3 position = Vector3.Lerp(startPosition, targetPosition, ratio);
                position.y += moveHeight * heightRatio;
                _golemObject.Rigidbody.MovePosition(position);

                yield return YieldCache.WaitForFixedUpdate;
            }

            _golemObject.Rigidbody.MovePosition(CubeUtil.GetNormalizedPosition(targetPosition));
        }

        private void UpdateGeometryData()
        {
            RotateAxisCube = _golemObject.FindCube(FindRotateAxis(Enums.Direction.Right));
            UpEdgeCube = _golemObject.FindCube(FindAxisCandidates(Enums.Direction3D.Up)[0]);
            BackEdgeCube = _golemObject.FindCube(FindAxisCandidates(Enums.Direction3D.Back)[0]);
            FrontEdgeCube = _golemObject.FindCube(FindAxisCandidates(Enums.Direction3D.Front)[0]);

            GolemWidth = GetGolemLength(Enums.Direction3D.Left) + GetGolemLength(Enums.Direction3D.Right) + 1;
            GolemHeight = GetGolemLength(Enums.Direction3D.Down) + GetGolemLength(Enums.Direction3D.Up) + 1;
            GolemBack = GetGolemLength(Enums.Direction3D.Back) + 1;

            ViewRotation = _moveDirection.GetRotation();
        }

        private Vector3[] GetObjectDirections()
        {
            int index = 0;
            _objectDirections[index++] = Transform.right;
            _objectDirections[index++] = -Transform.right;
            _objectDirections[index++] = Transform.up;
            _objectDirections[index++] = -Transform.up;
            _objectDirections[index++] = Transform.forward;
            _objectDirections[index++] = -Transform.forward;

            return _objectDirections;
        }

        private Enums.Direction3D ConvertObjectDirection(Enums.Direction3D direction)
        {
            Vector3[] objectDirections = GetObjectDirections();
            Vector3 moveDirection = _moveDirection.GetDirection(direction);

            return DirectionEnumUtils.ConvertDirection(objectDirections, moveDirection);
        }

        private int GetGolemLength(Enums.Direction3D direction)
        {
            return _golemObject.GolemData.GetDirectionLength(ConvertObjectDirection(direction));
        }

        private List<Vector3Int> FindAxisCandidates(Enums.Direction3D moveDirection)
        {
            _tempList.Clear();

            FindEdgeCubeDatas(moveDirection, _edgeCubes);
            if (_edgeCubes.Count <= 0) // Error
            {
                _tempList.Add(CubeConfig.CORE_POSITION);
                return _tempList;
            }

            if (_edgeCubes.Count == 1)
            {
                _tempList.Add(_edgeCubes[0].ShapePoisition);
                return _tempList;
            }

            float nearestDist = Vector3Int.Distance(CubeConfig.CORE_POSITION, _edgeCubes[0].ShapePoisition);
            foreach (CubeData cubeData in _edgeCubes)
            {
                float dist = Vector3Int.Distance(CubeConfig.CORE_POSITION, cubeData.ShapePoisition);
                if (dist < nearestDist)
                {
                    _tempList.Clear();
                    _tempList.Add(cubeData.ShapePoisition);
                    nearestDist = dist;
                }
                else if (dist == nearestDist)
                    _tempList.Add(cubeData.ShapePoisition);
            }

            return _tempList;
        }

        private Vector3Int FindRotateAxis(Enums.Direction direction)
        {
            List<Vector3Int> nearestCubes = FindAxisCandidates(Enums.Direction3D.Down);

            if (nearestCubes.Count == 1)
                return nearestCubes[0];

            _tempDictionary.Clear();
            foreach (Vector3Int position in nearestCubes)
            {
                Vector3 dir = position - CubeConfig.CORE_POSITION;
                dir.y = 0f;

                Enums.Direction3D key = DirectionEnumUtils.ConvertDirection(_moveDirection.GetDirections(), dir);
                _tempDictionary[key] = position;
            }

            Enums.Direction3D[] priorities = PRIORITY_CANDIDATES;
            if (direction == Enums.Direction.Left)
                priorities = REVERSE_PRIORITY_CANDIDATES;

            for (int i = 0; i < priorities.Length; i++)
            {
                if (_tempDictionary.TryGetValue(priorities[i], out Vector3Int position))
                    return position;
            }

            return CubeConfig.CORE_POSITION;
        }
    }
}