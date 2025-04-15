using UnityEngine;

public class DVLoadFlag
{
    #region Variables
    private int _loadCount = 0;
    private int _finishCount = 0;
    private bool _success = true;
    #endregion

    public DVLoadFlag(int count) {
        _finishCount = count;
    }

    public void SetFlag(bool success) {
        _success &= success;
        _loadCount++;
    }

    public bool Loading { get => _loadCount < _finishCount; }

    public bool IsSuccess { get => _success; }
}
