using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


public class DVKeyboardManager : SingletonMonoBehaviour<DVKeyboardManager>
{
    #region Variables
    private const float MAX_KEYING_TIME = 20f;

    private readonly HashSet<KeyCode> _systemKeyCodes = new HashSet<KeyCode> {
        KeyCode.Escape, KeyCode.Tab,
        KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4,
        KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8,
        KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12,
    };

    private readonly HashSet<KeyCode> _userKeyCodes = new HashSet<KeyCode> { 
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E,
        KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
        KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O,
        KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
        KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y,
        KeyCode.Z, 
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
        KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
        KeyCode.Alpha0, 
        KeyCode.Space, 
        KeyCode.Comma, KeyCode.Period, KeyCode.Slash,
        KeyCode.Semicolon, KeyCode.Quote,
        KeyCode.LeftBracket, KeyCode.RightBracket,
        KeyCode.Minus, KeyCode.Equals,
        KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.DownArrow,
    };

    private Dictionary<KeyCode, float> _keyingDic = new Dictionary<KeyCode, float>();
    private Dictionary<KeyCode, (Action, Action)> _keyTriggerDic = new Dictionary<KeyCode, (Action, Action)>();
    private HashSet<KeyCode> _usingKey = new HashSet<KeyCode>();
    private List<KeyCode> _circitKey = new List<KeyCode>();

    private bool _keyChanged = false;

    public bool keyBlocked = false;
    #endregion

    #region Unity Functions
    private void Update()
    {
        if (_usingKey.Count <= 0 || keyBlocked)
            return;

        if (_keyChanged) {
            _circitKey = _usingKey.ToList();
            _keyChanged = false;
        }

        foreach (var key in _circitKey) {
            if (keyBlocked)
                return;

            if (_keyChanged) { 
                _keyChanged = false;
                return;
            }

            if (Input.GetKeyDown(key)) {
                if (_keyTriggerDic[key].Item1 != null)
                    _keyTriggerDic[key].Item1();
            }
            else if (Input.GetKey(key))
            {
                _keyingDic[key] = Mathf.Clamp(_keyingDic[key] + Time.deltaTime, 0f, MAX_KEYING_TIME);
            }
            else if (Input.GetKeyUp(key))
            {
                Debug.Log($"Keying: {_keyingDic[key]}");
                _keyingDic[key] = 0f;

                if (_keyTriggerDic[key].Item2 != null)
                    _keyTriggerDic[key].Item2();
            }
        }
    }
    #endregion

    #region Public Functions
    public void SetKeyDownUp(KeyCode keyCode, Action actionDown, Action actionUp)
    {
        if (!CheckSetKey(keyCode))
            return;

        _usingKey.Add(keyCode);
        _keyingDic[keyCode] = 0f;
        _keyTriggerDic[keyCode] = (actionDown, actionUp);
        _keyChanged = true;
    }

    public void SetKeyDown(KeyCode keyCode, Action action)
    {
        SetKeyDownUp(keyCode, action, null);
    }

    public void SetKeyUp(KeyCode keyCode, Action action)
    {
        SetKeyDownUp(keyCode, null, action);
    }

    public float GetKeyingTime(KeyCode keyCode) {
        if (_keyingDic.TryGetValue(keyCode, out var time))
        {
            return time;
        }

        return 0f;
    }

    public bool IsKeying(KeyCode keyCode) { 
        float time = GetKeyingTime(keyCode);
        return !Mathf.Approximately(time, 0f);
    }

    public void DeleteKey(KeyCode keyCode) {
        if (!_usingKey.Contains(keyCode))
            return;

        if (_keyTriggerDic.TryGetValue(keyCode, out var val))
        {
            val.Item1 = null;
            val.Item2 = null;
            _keyTriggerDic.Remove(keyCode);
        }
        else if (_keyingDic.ContainsKey(keyCode))
        {
            _keyingDic.Remove(keyCode);
        }

        _usingKey.Remove(keyCode);
        _keyChanged = true;
    }

    public void ResetKeySetting() { 
        _usingKey.Clear();
        _keyingDic.Clear();
        _keyTriggerDic.Clear();
        _keyChanged = true;

        GC.Collect();
    }

    public bool CheckSetUserKey(KeyCode keyCode)
    {
        if (_userKeyCodes.Contains(keyCode))
        {
            return true;
        }

        Debug.Log($"{keyCode} is not User KeyCode");
        return false;
    }
    #endregion

    #region Utils
    private bool CheckSetKey(KeyCode keyCode) {
        if (_usingKey.Contains(keyCode)) {
            Debug.Log($"{keyCode} is using");
            return false;
        }

        if (!CheckSetUsableKey(keyCode)) { 
            return false;
        }

        return true;
    }

    private bool CheckSetUsableKey(KeyCode keyCode) {
        if (_systemKeyCodes.Contains(keyCode) || _userKeyCodes.Contains(keyCode)) {
            return true;
        }

        Debug.Log($"{keyCode} is not usable KeyCode");
        return false;
    }
    #endregion
}
