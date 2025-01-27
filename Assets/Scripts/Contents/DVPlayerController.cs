using UnityEngine;
using System.Collections;

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
    private KeyCode[] _playerActKeys;
    #endregion

    #region Unity Functions
    protected override void Awake()
    {
        base.Awake();
        _playerActKeys = new KeyCode[System.Enum.GetValues(typeof(PlayerActKey)).Length];
    }

    protected override void Start()
    {
        base.Start();
        SetInitActKey();

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
        _playerActKeys[(int)PlayerActKey.MOVE_FRONT] = KeyCode.W;
        _playerActKeys[(int)PlayerActKey.MOVE_RIGHT] = KeyCode.D;
        _playerActKeys[(int)PlayerActKey.MOVE_LEFT] = KeyCode.A;
        _playerActKeys[(int)PlayerActKey.MOVE_BACK] = KeyCode.S;

        _playerActKeys[(int)PlayerActKey.ROTATE_RIGHT] = KeyCode.E;
        _playerActKeys[(int)PlayerActKey.ROTATE_LEFT] = KeyCode.Q;

        _playerActKeys[(int)PlayerActKey.JUMP] = KeyCode.Space;
        
        KeySetting();
    }

    private void KeySetting()
    {
        // Move
        {
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.MOVE_FRONT],
                () => MoveGolemWithJump(DVEnums.Direction.FRONT, 1),
                (keyTime) => CancelMoveGolem());
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.MOVE_RIGHT],
                () => RollGolem(DVEnums.Direction.RIGHT),
                (keyTime) => CancelMoveGolem());
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.MOVE_LEFT],
                () => RollGolem(DVEnums.Direction.LEFT),
                (keyTime) => CancelMoveGolem());
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.MOVE_BACK],
                () => RollGolem(DVEnums.Direction.BACK),
                (keyTime) => CancelMoveGolem());
        }

        // Rotate
        {
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.ROTATE_RIGHT],
                () => RotateGolem(DVEnums.Direction.RIGHT),
                (keyTime) => CancelMoveGolem());
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.ROTATE_LEFT],
                () => RotateGolem(DVEnums.Direction.LEFT),
                (keyTime) => CancelMoveGolem());
        }

        // Jump
        {
            DVKeyboardManager.Instance.SetKeyDownUp(_playerActKeys[(int)PlayerActKey.JUMP],
                () => ChargeJumpReady(),
                (keyTime) => ChargeJumpAction(keyTime));
        }

        DVKeyboardManager.Instance.SetKeyLocks(_playerActKeys);
    }
    #endregion

}
