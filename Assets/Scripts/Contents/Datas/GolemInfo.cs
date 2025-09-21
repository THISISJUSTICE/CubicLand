using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand
{
    [System.Serializable]
    public class GolemInfo
    {
        #region Data Variables
        [SerializeField] private Status _status;
        [SerializeField] private int _moveSpeedPoint;
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
        public Status Status { get => _status; }
        public int MoveSpeedPoint { get => _moveSpeedPoint; }
        public int MoveSpeed { get => StatusConfig.INIT_MOVE_SPEED + _moveSpeedPoint * StatusConfig.ADD_MOVE_SPEED; }
        public HashSet<Vector3Int> Shape { get => _shape; }
        public Dictionary<Vector3Int, HashSet<Vector3Int>> ChildMap { get => _childMap; }
        public Dictionary<Vector3Int, Vector3Int> ParentMap { get => _parentMap; }
        #endregion

        #region Constructors
        public GolemInfo()
        {
            _status = new Status(0, 0, 0);
            _moveSpeedPoint = 0;
            _shape = new HashSet<Vector3Int>();
            _shape.Add(Vector3Int.zero);
            _childMap = new Dictionary<Vector3Int, HashSet<Vector3Int>>();
            _parentMap = new Dictionary<Vector3Int, Vector3Int>();
        }

        public GolemInfo(GolemInfo golemInfo)
            : this(golemInfo.Status, golemInfo.MoveSpeedPoint, golemInfo.Shape, golemInfo.ChildMap, golemInfo.ParentMap)
        {

        }

        public GolemInfo(Status status, int moveSpeedPoint = 0,
            HashSet<Vector3Int> shape = null, Dictionary<Vector3Int, HashSet<Vector3Int>> childMap = null,
            Dictionary<Vector3Int, Vector3Int> parentMap = null)
        {
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

        #region Utils
        public int GetDirectionSize(Enums.Direction3D direction)
        {
            return Mathf.Abs(GetDirectionValue(direction));
        }

        public int GetDirectionValue(Enums.Direction3D direction)
        {
            return ShapeUtil.GetDirectionValue(_shape, direction);
        }

        public List<Vector3Int> FindEdgeChilds(Enums.Direction3D direction, int length)
        {
            return ShapeUtil.FindEdgeChilds(_shape, direction, length);
        }

        public Vector3Int[] GetAbleAddCubePosition(Vector3Int parentPos)
        {
            List<Vector3Int> list = new List<Vector3Int>();
            for (int i = 0; i < Utils.Direction3DLength; i++)
            {
                Vector3Int childPos = parentPos + Utils.GetDirection3DValue((Enums.Direction3D)i);
                if (!_shape.Contains(childPos))
                    list.Add(childPos);
            }

            return list.ToArray();
        }

        public bool AddCube(Vector3Int parentPos, Enums.Direction3D direction)
        {
            Vector3Int childPos = parentPos + Utils.GetDirection3DValue(direction);
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

        public List<Vector3Int> GetChilds(Vector3Int cubePos, bool remove = false)
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

    public static class ShapeUtil
    {
        public static int GetDirectionValue(HashSet<Vector3Int> shape, Enums.Direction3D direction)
        {
            switch (direction)
            {
                case Enums.Direction3D.Right:
                    return GetValue(shape, 0, true);
                case Enums.Direction3D.Left:
                    return GetValue(shape, 0, false);
                case Enums.Direction3D.Up:
                    return GetValue(shape, 1, true);
                case Enums.Direction3D.Down:
                    return GetValue(shape, 1, false);
                case Enums.Direction3D.Front:
                    return GetValue(shape, 2, true);
                case Enums.Direction3D.Back:
                    return GetValue(shape, 2, false);
                default:
                    return 0;
            }
        }

        private static int GetValue(HashSet<Vector3Int> shape, int vectorDirection, bool isMax)
        {
            int value = 0;

            foreach (Vector3Int pos in shape)
            {
                if (isMax)
                    value = Mathf.RoundToInt(Mathf.Max(value, pos[vectorDirection]));
                else
                    value = Mathf.RoundToInt(Mathf.Min(value, pos[vectorDirection]));
            }

            return Mathf.Abs(value);
        }

        public static List<Vector3Int> FindEdgeChilds(HashSet<Vector3Int> shape, Enums.Direction3D direction, int length)
        {
            length = Mathf.RoundToInt(Mathf.Abs(length));

            switch (direction)
            {
                case Enums.Direction3D.Right:
                    return FindChilds(shape, 0, length);
                case Enums.Direction3D.Left:
                    return FindChilds(shape, 0, -length);
                case Enums.Direction3D.Up:
                    return FindChilds(shape, 1, length);
                case Enums.Direction3D.Down:
                    return FindChilds(shape, 1, -length);
                case Enums.Direction3D.Front:
                    return FindChilds(shape, 2, length);
                case Enums.Direction3D.Back:
                    return FindChilds(shape, 2, -length);
                default:
                    return null;
            }
        }

        private static List<Vector3Int> FindChilds(HashSet<Vector3Int> shape, int vectorDirection, int edge)
        {
            List<Vector3Int> childs = new List<Vector3Int>();
            foreach (Vector3Int pos in shape)
            {
                if (pos[vectorDirection] == edge)
                    childs.Add(pos);
            }

            return childs;
        }
    }
}