using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand
{
    // Attack, Shape, MoveSpeed, Directions
    [Serializable]
    public class ChargeShoot : GolemSkillSummon
    {
        #region Data Variables
        [SerializeField] private GolemInfo _skillGolemInfo;
        [SerializeField] private List<Enums.Direction3D> _moveDirections;
        [SerializeField] private List<int> _convertDirections; // Length: _moveDirections - 1
        #endregion

        #region Inner Variables
        private Coroutine _chargeCor;
        private SkillGolemCore _skillGolem;

        private GolemInfo _maxGolemInfo;
        private GolemInfo _golemInfoLevel;
        private int _currentLevel;
        private HashSet<Vector3Int> _shapeLevel;

        private bool _isCreatingCube = false;

        private const float ChargeTime = 1f;

        private const int MaxChargeLevel = 10;

        public override float DelayTime => 0.2f;
        #endregion

        public ChargeShoot(GolemController owner, GolemInfo skillGolemInfo = null,
            List<Enums.Direction3D> moveDirections = null, List<int> convertDirections = null)
            : base(owner)
        {
            if (skillGolemInfo == null)
                skillGolemInfo = new GolemInfo();
            _skillGolemInfo = skillGolemInfo;

            Status status = new Status(0, 0, MaxChargeLevel - 1 + _skillGolemInfo.Status.Point_Attack * 5);
            int moveSpeedPoint = MaxChargeLevel / 2 + _skillGolemInfo.MoveSpeedPoint * 10;
            _maxGolemInfo = new GolemInfo(status, moveSpeedPoint, _skillGolemInfo.Shape, _skillGolemInfo.ChildMap, _skillGolemInfo.ParentMap);

            if (moveDirections == null || convertDirections == null)
            {
                moveDirections = new List<Enums.Direction3D>() { Enums.Direction3D.Front };
                convertDirections = new List<int>();
            }

            if (moveDirections.Count - 1 != convertDirections.Count)
            {
                // TODO: 더 작은 리스트를 위주로 구현
            }
            _moveDirections = moveDirections;
            _convertDirections = convertDirections;
        }

        public override GolemSkill Clone()
        {
            return new ChargeShoot(_owner, _skillGolemInfo, _moveDirections, _convertDirections);
        }

        #region Public Functions
        public void AddMoveDirection(Enums.Direction3D moveDirection, int convertDirection)
        {
            _moveDirections.Add(moveDirection);
            _convertDirections.Add(Mathf.Max(1, convertDirection));
        }

        public override void KeyDown()
        {
            _currentLevel = 0;
            _shapeLevel = new HashSet<Vector3Int>(_maxGolemInfo.Shape);
            _chargeCor = _owner.StartCoroutine(ChargeCor());
        }

        public override void KeyUp()
        {
            if (_chargeCor != null)
            {
                _owner.StopCoroutine(_chargeCor);
                _chargeCor = null;
            }

            _skillGolem.StartCoroutine(ShootCor());
            // TODO: 지속 시간동안만 유지 이후 파괴?
        }

        public override void Cancel()
        {
            // TODO: Core가 파괴될 시 취소
            // TODO: 직접 공격 받으면 취소
        }
        #endregion

        #region Utils
        private void CreateCore()
        {
            _shapeLevel.Remove(Vector3Int.zero);
            _golemInfoLevel = new GolemInfo(new Status(0, 0, 0));

            _skillGolem = CubeCreator.Instance.SummonSkillGolemCore(_owner, _golemInfoLevel, $"{_owner.name} ChargeShoot", CalculateCorePosition());
        }

        private Vector3 CalculateCorePosition()
        {
            Vector3 ownerFrontDirection = (_owner.PlayerViewRotation * Vector3.forward).normalized;
            Enums.Direction3D viewFront = _owner.ConvertMoveToTransformDirection(Enums.Direction3D.Front);
            Enums.Direction3D viewBack = _owner.ConvertMoveToTransformDirection(Enums.Direction3D.Back);
            Enums.Direction3D viewDown = _owner.ConvertMoveToTransformDirection(Enums.Direction3D.Down);

            float ownerFrontLength = Configs.CUBE_BASE_LENGHT *
                (_ownerCore.GolemInfo.GetDirectionSize(viewFront) + 1);
            float ownerDownLength = Configs.CUBE_BASE_LENGHT *
                _ownerCore.GolemInfo.GetDirectionSize(viewDown);

            float skillBackLength = 0f;
            float skillDownLength = 0f;

            if (_skillGolem != null)
            {
                skillBackLength = Configs.CUBE_BASE_LENGHT * (float)_skillGolem.GolemInfo.GetDirectionSize(viewBack);
                skillDownLength = Configs.CUBE_BASE_LENGHT * (float)_skillGolem.GolemInfo.GetDirectionSize(viewDown);
            }

            Debug.Log($"ownerFrontLength({ownerFrontLength}), skillBackLength({skillBackLength})");

            Vector3 skillCorePosition = _owner.transform.position
                + ownerFrontDirection * (ownerFrontLength + skillBackLength + Configs.CUBE_BASE_LENGHT);

            if (ownerDownLength < Configs.CUBE_BASE_LENGHT)
                skillCorePosition += Vector3.up * Configs.CUBE_BASE_LENGHT;

            if (skillDownLength > ownerDownLength)
                skillCorePosition += Vector3.up * (skillDownLength - ownerDownLength);

            return skillCorePosition;
        }

        private void AddChild(List<Vector3Int> childs)
        {
            // TODO: SkillGolem의 부모가 파괴될 시 제자리에서 정지

            // TODO: 파티클 생성 효과
            // TODO: 스킬 큐브 파괴 이펙트는 자연 소멸, 타격 적용 

            CubeCreator.Instance.SummonSkillGolemChilds(_skillGolem, childs.ToArray());
        }

        private void EnhanceSkill(float time)
        {
            int level = Mathf.FloorToInt(time / ChargeTime);
            if (level <= _currentLevel)
                return;

            _currentLevel = level;
            float rate = (float)_currentLevel / (float)MaxChargeLevel;
            int attackPoint = Mathf.FloorToInt((float)_maxGolemInfo.Status.Point_Attack * rate);
            int speedPoint = Mathf.FloorToInt((float)_maxGolemInfo.MoveSpeedPoint * rate);

            Status status = new Status(0, 0, attackPoint);
            GolemInfo golemInfo = new GolemInfo(status, speedPoint, _golemInfoLevel.Shape, _golemInfoLevel.ChildMap, _golemInfoLevel.ParentMap);

            _golemInfoLevel = golemInfo;

            int shapeCount = Mathf.FloorToInt((float)_maxGolemInfo.Shape.Count * rate);
            if (_golemInfoLevel.Shape.Count != shapeCount)
            {
                // TODO: 큐브 생성 시간보다 좀 더 빠르게
                _skillGolem.StartCoroutine(MoveSkillCore(0.02f));

                int addChildCount = shapeCount - _golemInfoLevel.Shape.Count;
                List<Vector3Int> addChilds = new List<Vector3Int>();

                foreach (Vector3Int childPos in _shapeLevel)
                {
                    Vector3Int parentPos = _maxGolemInfo.ParentMap[childPos];
                    if (_golemInfoLevel.Shape.Contains(parentPos))
                    {
                        _golemInfoLevel.AddCube(parentPos, Utils.ConvertDirection(childPos - parentPos));
                        addChilds.Add(childPos);

                        if (--addChildCount <= 0)
                            break;
                    }
                }

                foreach (Vector3Int childPos in addChilds)
                    _shapeLevel.Remove(childPos);

                AddChild(addChilds);
            }

            _skillGolem.SetGolemInfo(_golemInfoLevel);
        }
        #endregion

        #region Coroutines
        private IEnumerator ChargeCor()
        {
            // TODO: 차지 애니메이션
            // TODO: 생성 시 약간의 지연
            CreateCore();

            float time = 0f;
            while (_currentLevel < MaxChargeLevel)
            {
                yield return null;
                time += Time.deltaTime;
                EnhanceSkill(time);
            }

            _chargeCor = null;
        }

        private IEnumerator MoveSkillCore(float moveTime)
        {
            _isCreatingCube = true; // TODO: 제거

            Vector3 startPos = _skillGolem.transform.position;
            Vector3 moveDir = CalculateCorePosition() - startPos;
            float moveDist = moveDir.magnitude;
            moveDir.Normalize();

            float frameCount = Mathf.Ceil(moveTime / Time.fixedDeltaTime);
            float frameDist = moveDist / frameCount;

            Vector3 currentPos;

            /*for (int i = 0; i < frameCount; i++)
            {
                yield return DVHelper.YieldCache.WaitForFixedUpdate;

                currentPos = _skillGolem.transform.position;
                _skillGolem.transform.position = currentPos + moveDir * frameDist;
            }*/

            _skillGolem.transform.position = startPos + moveDir * moveDist;

            _isCreatingCube = false; // TODO: 제거
            yield break;
        }

        private IEnumerator ShootCor()
        {
            yield return new WaitUntil(() => !_isCreatingCube);

            Quaternion rotation = _owner.PlayerViewRotation;
            float moveTime = _skillGolem.CalculateMoveTime(INIT_MOVE_TIME, MIN_MOVE_TIME, MAX_MOVE_TIME);
            Vector3 prevPos = _skillGolem.transform.position;
            Vector3 moveDir = (rotation * Utils.GetDirection3DValue(_moveDirections[0])).normalized;

            _skillGolem.rb.UseLinear(true);
            _skillGolem.rb.AddForce(moveDir * _skillGolem.rb.GetVelocityForce(Configs.CUBE_BASE_LENGHT, moveTime), ForceMode.Impulse);

            Vector3 prevMoveDir; int convert;
            for (int i = 0; i < _convertDirections.Count; i++)
            {
                prevMoveDir = moveDir;
                convert = _convertDirections[i];
                moveDir = (rotation * Utils.GetDirection3DValue(_moveDirections[i + 1])).normalized;

                yield return Helper.YieldCache.GetWaitForSeconds(moveTime * (float)convert);
                _skillGolem.rb.CancelVelocity();

                prevPos += prevMoveDir * Configs.CUBE_BASE_LENGHT;
                _skillGolem.rb.MovePosition(prevPos);

                _skillGolem.rb.AddForce(moveDir * _skillGolem.rb.GetVelocityForce(Configs.CUBE_BASE_LENGHT, moveTime), ForceMode.Impulse);
            }
        }
        #endregion
    }
}