using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public sealed class DVPlayerController : DVGolemController
{
    #region Types
    public enum PlayerActKey { 
        MOVE_FRONT,
        MOVE_RIGHT,
        MOVE_LEFT,
        MOVE_BACK,

        ROTATE_RIGHT,
        ROTATE_LEFT,

        JUMP
    }
    #endregion

    #region Variables
    private int PlayerActKeyLength => System.Enum.GetNames(typeof(PlayerActKey)).Length;

    private List<KeyCode> _playerActKeys;

    private List<DVGolemSkill> _skillList;

    private DVGolemSkill _usingSkill = null;
    #endregion

    #region Unity Functions
    protected override void Awake()
    {
        base.Awake();
        _playerActKeys = new KeyCode[PlayerActKeyLength].ToList();

        // TODO: 저장된 데이터를 통해 스킬 데이터 불러오기

        DVGolemInfo golemInfo = new DVGolemInfo(new DVStatus(0, 0, 5), moveSpeedPoint: 20);
        for (int i = 0; i < 10; i++)
            DVCubeCreator.Instance.AddRandomGolemCube(golemInfo);

        _skillList = new List<DVGolemSkill>() { new DVChargeShoot(this, golemInfo) };
    }

    private void Start()
    {
        SetInitActKey();

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DVKeyboardManager.Instance?.DeleteKeys(_playerActKeys);
    }
    #endregion

    #region Public Functions
    public void SetActKey(PlayerActKey playerActKey, KeyCode keyCode) {
        DVKeyboardManager.Instance.DeleteKeys(_playerActKeys);
        _playerActKeys[(int)playerActKey] = keyCode;
        KeySetting();
    }
    #endregion

    #region Utils
    private void SetInitActKey() {

        // TODO: 임시 키 세팅
        // TODO: 데이터를 통해 키를 받아오도록 변경
        // TODO: Window에서만 적용
        _playerActKeys[(int)PlayerActKey.MOVE_FRONT] = KeyCode.W;
        _playerActKeys[(int)PlayerActKey.MOVE_RIGHT] = KeyCode.D;
        _playerActKeys[(int)PlayerActKey.MOVE_LEFT] = KeyCode.A;
        _playerActKeys[(int)PlayerActKey.MOVE_BACK] = KeyCode.S;

        _playerActKeys[(int)PlayerActKey.ROTATE_RIGHT] = KeyCode.E;
        _playerActKeys[(int)PlayerActKey.ROTATE_LEFT] = KeyCode.Q;

        _playerActKeys[(int)PlayerActKey.JUMP] = KeyCode.Space;

        _playerActKeys.Add(KeyCode.L);
        
        KeySetting();
    }

    private void KeySetting()
    {
        // Move
        {
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.MOVE_FRONT],
                () => MoveJump(DVEnums.Direction.FRONT),
                (keyTime) => CancelMove());
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.MOVE_RIGHT],
                () => RollGolem(DVEnums.Direction.RIGHT),
                (keyTime) => CancelMove());
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.MOVE_LEFT],
                () => RollGolem(DVEnums.Direction.LEFT),
                (keyTime) => CancelMove());
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.MOVE_BACK],
                () => RollGolem(DVEnums.Direction.BACK),
                (keyTime) => CancelMove());
        }

        // Rotate
        {
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.ROTATE_RIGHT],
                () => Rotate(DVEnums.Direction.RIGHT),
                (keyTime) => CancelMove());
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.ROTATE_LEFT],
                () => Rotate(DVEnums.Direction.LEFT),
                (keyTime) => CancelMove());
        }

        // Jump
        {
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.JUMP],
                ChargeJumpReady, ChargeJumpAction);
        }

        // Skills
        for (int i = 0; i < _skillList.Count; i++)
        {
            DVGolemSkill skill = _skillList[i];
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[PlayerActKeyLength + i],
                () => KeyDownSkill(skill),
                (keyTime) => KeyUpSkill());
        }

        DVKeyboardManager.Instance.SetKeyLocks(_playerActKeys);
    }

    private void KeyDownSkill(DVGolemSkill skill)
    {
        // TODO: 쿨타임 중엔 return
        if (_move.Acting || _usingSkill != null)
            return;

        _move.ActFlag = true;
        _move.Acting = true;

        skill = skill.Clone();
        skill.KeyDown();
        _usingSkill = skill;
    }

    private void KeyUpSkill()
    {
        if (_usingSkill == null)
            return;

        _usingSkill.KeyUp();
        this.WaitTimeAct(_usingSkill.DelayTime, SetInit);
        _usingSkill = null;
    }
    #endregion

}
