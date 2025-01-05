using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


public class DVKeyboardManager : SingletonMonoBehaviour<DVKeyboardManager>
{
    #region Types
    private class KeyLock
    {
        private KeyCode _lockKey;
        private bool _locked;

        public KeyLock() {
            _locked = false;
        }

        public bool Locked { get => _locked; }

        public void Lock(KeyCode key) {
            _lockKey = key;
            _locked = true;
        }

        public void Unlock(KeyCode key) {
            if (key == _lockKey) {
                _locked = false;
            }
        }
    }
    #endregion

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
    private Dictionary<KeyCode, (Action, Action<float>)> _keyTriggerDic = new Dictionary<KeyCode, (Action, Action<float>)>();
    private HashSet<KeyCode> _usingKey = new HashSet<KeyCode>();
    private List<KeyCode> _circitKey = new List<KeyCode>();

    private Dictionary<KeyCode, KeyLock> _keyLocks = new Dictionary<KeyCode, KeyLock>();
    private Dictionary<KeyLock, HashSet<KeyCode>> _keyLockFinder = new Dictionary<KeyLock, HashSet<KeyCode>>();

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
                if (_keyLocks.TryGetValue(key, out var keyLock)) {
                    if (keyLock.Locked)
                        continue;
                    keyLock.Lock(key);
                }

                if (_keyTriggerDic[key].Item1 != null)
                    _keyTriggerDic[key].Item1();
            }
            else if (Input.GetKey(key))
            {
                _keyingDic[key] = Mathf.Clamp(_keyingDic[key] + Time.deltaTime, 0f, MAX_KEYING_TIME);
            }
            else if (Input.GetKeyUp(key))
            {
                if (_keyTriggerDic[key].Item2 != null)
                    _keyTriggerDic[key].Item2(_keyingDic[key]);

                if (_keyLocks.TryGetValue(key, out var keyLock))
                {
                    keyLock.Unlock(key);
                }

                _keyingDic[key] = 0f;
            }
        }
    }
    #endregion

    #region Public Functions
    public void SetKeyDownUp(KeyCode keyCode, Action actionDown, Action<float> actionUp)
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

    public void SetKeyUp(KeyCode keyCode, Action<float> action)
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
        if (_keyingDic.ContainsKey(keyCode))
        {
            _keyingDic.Remove(keyCode);
        }
        if (_keyLocks.TryGetValue(keyCode, out var keyLock)) {
            _keyLocks.Remove(keyCode);
            _keyLockFinder[keyLock].Remove(keyCode);
        }

        _usingKey.Remove(keyCode);
        _keyChanged = true;
    }

    public void DeleteKeys(KeyCode[] keyCodes) {
        for (int i = 0; i < keyCodes.Length; i++)
            DeleteKey(keyCodes[i]);
    }

    public void ResetKeySetting() {
        _usingKey.Clear();
        _keyingDic.Clear();
        _keyTriggerDic.Clear();
        _keyLocks.Clear();
        _keyLockFinder.Clear();
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

    public void SetKeyLocks(KeyCode[] keyCodes) {
        KeyLock keyLock = new KeyLock();
        _keyLockFinder[keyLock] = new HashSet<KeyCode>();

        for (int i = 0; i < keyCodes.Length; i++) {
            if (!CheckAddKeyLock(keyCodes[i]))
                continue;

            _keyLocks[keyCodes[i]] = keyLock;
            _keyLockFinder[keyLock].Add(keyCodes[i]);
        }
    }

    public void AddKeyLocks(KeyCode key, KeyCode[] keyCodes) {
        if (!TryGetKeyLock(key, out var keyLock))
            return;

        if (_keyLockFinder[keyLock] == null)
            _keyLockFinder[keyLock] = new HashSet<KeyCode>();

        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (!CheckAddKeyLock(keyCodes[i]))
                continue;

            _keyLocks[keyCodes[i]] = keyLock;
            _keyLockFinder[keyLock].Add(keyCodes[i]);
        }
    }

    public void DeleteKeyLocks(KeyCode keyCode) {
        if (!TryGetKeyLock(keyCode, out var keyLock))
            return;

        foreach (var key in _keyLocks.Keys) {
            if (_keyLocks.ContainsKey(key))
                _keyLocks.Remove(key);
        }

        _keyLockFinder[keyLock] = null;
        _keyLockFinder.Remove(keyLock);
        keyLock = null;
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

    private bool CheckAddKeyLock(KeyCode keyCode) {
        if (!_usingKey.Contains(keyCode))
        {
            Debug.Log($"{keyCode} is not using");
            return false;
        }
        if (_keyLocks.ContainsKey(keyCode))
        {
            Debug.Log($"{keyCode} is already used KeyLock");
            return false;
        }

        return true;
    }

    private bool TryGetKeyLock(KeyCode keyCode, out KeyLock keyLock) {
        if (!_keyLocks.TryGetValue(keyCode, out keyLock))
        {
            Debug.Log($"{keyCode} is not used KeyLock");
            return false;
        }

        if (keyLock == null || !_keyLockFinder.ContainsKey(keyLock))
        {
            Debug.Log($"KeyLock does not exist");
            return false;
        }

        return true;
    }
    #endregion
}
