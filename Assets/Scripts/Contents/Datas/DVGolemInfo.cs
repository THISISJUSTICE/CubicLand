using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DVGolemInfo
{ 
    #region Data Variables
    private DVStatus _status;
    private int _moveSpeedPoint;
    private HashSet<Vector3Int> _shape;// (0,0,0)은 Core (R:x+1, L:x-1, U:y+1, D:y-1, F:z+1, B:z-1)

    // TODO: Dictionary를 List<KeyValuePair<TKey, TValue>>로 변환
    // TODO: HashSet은 List로 변환
    private Dictionary<Vector3Int, HashSet<Vector3Int>> _childMap;
    private Dictionary<Vector3Int, Vector3Int> _parentMap;

    // TODO: 각 파츠 큐브 별 현재 체력 저장
    // private Dictionary<Vector3Int, DVCurrentStatus> _everyStatuses;

    // TODO: 보유 스킬 및 스킬 레벨
    #endregion

    #region Properties
    public DVStatus Status { get => _status; }
    public int MoveSpeedPoint { get => _moveSpeedPoint; }
    public int MoveSpeed { get => DVStatusConfig.INIT_MOVE_SPEED + _moveSpeedPoint * DVStatusConfig.ADD_MOVE_SPEED; }
    public HashSet<Vector3Int> Shape { get => _shape; }
    public Dictionary<Vector3Int, HashSet<Vector3Int>> ChildMap { get => _childMap; }
    public Dictionary<Vector3Int, Vector3Int> ParentMap { get => _parentMap; }
    #endregion

    #region Constructors
    public DVGolemInfo() {
        _status = new DVStatus(0, 0, 0);
        _moveSpeedPoint = 0;
        _shape = new HashSet<Vector3Int>();
        _shape.Add(Vector3Int.zero);
        _childMap = new Dictionary<Vector3Int, HashSet<Vector3Int>>();
        _parentMap = new Dictionary<Vector3Int, Vector3Int>();
    }

    public DVGolemInfo(DVGolemInfo golemInfo) 
        : this(golemInfo.Status, golemInfo.MoveSpeedPoint, golemInfo.Shape, golemInfo.ChildMap, golemInfo.ParentMap)
    {

    }

    public DVGolemInfo(DVStatus status, int moveSpeedPoint = 0,
        HashSet<Vector3Int> shape = null, Dictionary<Vector3Int, HashSet<Vector3Int>> childMap = null,
        Dictionary<Vector3Int, Vector3Int> parentMap = null) { 
        _status = status;
        _moveSpeedPoint = moveSpeedPoint;

        if (shape != null)
            _shape = shape;
        else
        {
            _shape = new HashSet<Vector3Int>();
            _shape.Add(Vector3Int.zero);
        }
        if (childMap != null)
            _childMap = childMap;
        else
            _childMap = new Dictionary<Vector3Int, HashSet<Vector3Int>>();

        if (parentMap != null)
            _parentMap = parentMap;
        else
            _parentMap = new Dictionary<Vector3Int, Vector3Int>();
    }
    #endregion

    #region Public Functions
    public int GetDirectionSize(DVEnums.Direction3D direction)
    {
        switch (direction)
        {
            case DVEnums.Direction3D.RIGHT:
                return GetSize(0, true);
            case DVEnums.Direction3D.LEFT:
                return GetSize(0, false);
            case DVEnums.Direction3D.UP:
                return GetSize(1, true);
            case DVEnums.Direction3D.DOWN:
                return GetSize(1, false);
            case DVEnums.Direction3D.FRONT:
                return GetSize(2, true);
            case DVEnums.Direction3D.BACK:
                return GetSize(2, false);
            default:
                return 0;
        }
    }

    public List<Vector3Int> FindEdgeChilds(DVEnums.Direction3D direction, int length) {
        length = Mathf.RoundToInt(Mathf.Abs(length));
        switch (direction)
        {
            case DVEnums.Direction3D.RIGHT:
                return FindChilds(0, length);
            case DVEnums.Direction3D.LEFT:
                return FindChilds(0, -length);
            case DVEnums.Direction3D.UP:
                return FindChilds(1, length);
            case DVEnums.Direction3D.DOWN:
                return FindChilds(1, -length);
            case DVEnums.Direction3D.FRONT:
                return FindChilds(2, length);
            case DVEnums.Direction3D.BACK:
                return FindChilds(2, -length);
            default:
                return null;
        }
    }

    public Vector3Int[] GetAbleAddCubePosition(Vector3Int parentPos)
    {
        List<Vector3Int> list = new List<Vector3Int>();
        for (int i = 0; i < DVUtil.Direction3DLength; i++)
        {
            Vector3Int childPos = parentPos + DVUtil.GetDirection3DValue((DVEnums.Direction3D)i);
            if (!_shape.Contains(childPos))
                list.Add(childPos);
        }

        return list.ToArray();
    }

    public bool AddCube(Vector3Int parentPos, DVEnums.Direction3D direction)
    {
        Vector3Int childPos = parentPos + DVUtil.GetDirection3DValue(direction);
        if (_shape.Contains(childPos))
        {
            //Debug.Log($"{childPos} is already exist");
            return false;
        }

        _shape.Add(childPos);
        if (!_childMap.ContainsKey(parentPos) || _childMap[parentPos] == null)
            _childMap[parentPos] = new HashSet<Vector3Int>();
        _childMap[parentPos].Add(childPos);
        _parentMap[childPos] = parentPos;

        return true;
    }

    public bool RemoveCube(Vector3Int cubePos)
    {
        if (!_shape.Contains(cubePos))
        {
            //Debug.Log($"{cubePos} is not exist");
            return false;
        }

        _shape.Remove(cubePos);

        List<Vector3Int> childPosList = GetChilds(cubePos, remove: true);
        foreach (Vector3Int childPos in childPosList)
        {
            _shape.Remove(childPos);
            _parentMap.Remove(childPos);
        }

        return true;
    }
    #endregion

    #region Utils
    private int GetSize(int vectorDirection, bool isMax)
    {
        int value = 0;

        foreach (Vector3Int pos in _shape)
        {
            int[] posDirs = new int[3] { pos.x, pos.y, pos.z };
            if (isMax)
                value = Mathf.RoundToInt(Mathf.Max(value, posDirs[vectorDirection]));
            else
                value = Mathf.RoundToInt(Mathf.Min(value, posDirs[vectorDirection]));
        }

        return (int)Mathf.Abs(value) + 1;
    }

    private List<Vector3Int> FindChilds(int vectorDirection, int edge) {
        List<Vector3Int> childs = new List<Vector3Int>();
        foreach (Vector3Int pos in _shape)
        {
            int[] posDirs = new int[3] { pos.x, pos.y, pos.z };
            if (pos[vectorDirection] == edge) 
                childs.Add(pos);
        }

        return childs;
    }

    private List<Vector3Int> GetChilds(Vector3Int cubePos, bool remove = false)
    {
        List<Vector3Int> list = new List<Vector3Int>();
        List<Vector3Int> childList = new List<Vector3Int>();
        if (_childMap.ContainsKey(cubePos) && _childMap[cubePos] != null && _childMap[cubePos].Count > 0)
        {
            foreach (var childPos in _childMap[cubePos])
            {
                list.Add(childPos);
            }

            if (remove)
                _childMap.Remove(cubePos);

            foreach (var childPos in list)
            {
                var childs = GetChilds(childPos, remove);
                foreach (var child in childs)
                {
                    childList.Add(child);
                }
            }
        }

        foreach (var childPos in childList)
        {
            list.Add(childPos);
        }

        return list;
    }
    #endregion

}
