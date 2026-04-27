using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public class GolemData
    {
        /*TODO: 저장 대상*/private readonly Dictionary<Vector3Int, CubeData> _cubeDatas; // (0,0,0)은 Core (R:x+1, L:x-1, U:y+1, D:y-1, F:z+1, B:z-1)
        /*TODO: 저장 대상*/private readonly Dictionary<Vector3Int, List<Vector3Int>> _children;
        private readonly Dictionary<Vector3Int, Vector3Int> _parents;
        private readonly Dictionary<Vector3Int, int> _childDepths;

        /*TODO: 저장 대상*/public int MoveSpeedPoint { get; set; }
        public int MoveSpeed => CubeConfig.Status.INIT_MOVE_SPEED + MoveSpeedPoint * CubeConfig.Status.ADD_MOVE_SPEED;
        public IReadOnlyDictionary<Vector3Int, CubeData> CubeDatas => _cubeDatas;
        public IReadOnlyDictionary<Vector3Int, List<Vector3Int>> Children => _children;
        public IReadOnlyDictionary<Vector3Int, Vector3Int> Parents => _parents;
        public IReadOnlyDictionary<Vector3Int, int> ChildDepths => _childDepths;

        public GolemData(int moveSpeedPoint = 0,
            Dictionary<Vector3Int, CubeData> cubeDatas = null,
            Dictionary<Vector3Int, List<Vector3Int>> children = null)
        {
            MoveSpeedPoint = moveSpeedPoint;

            if (cubeDatas != null)
                _cubeDatas = cubeDatas;
            else
                _cubeDatas = new Dictionary<Vector3Int, CubeData>()
                { { CubeConfig.CORE_POSITION, new CubeData(new StatusPoint(), CubeConfig.CORE_POSITION, Color.white) } };

            if (children != null)
                _children = children;
            else
                _children = new Dictionary<Vector3Int, List<Vector3Int>>();

            _parents = new Dictionary<Vector3Int, Vector3Int>();
            foreach (Vector3Int parent in _children.Keys)
            {
                if (!_children.TryGetValue(parent, out List<Vector3Int> childList)
                    || childList == null || childList.Count == 0)
                    continue;

                foreach (Vector3Int child in childList)
                    _parents[child] = parent;
            }

            _childDepths = new Dictionary<Vector3Int, int>();
            _childDepths[CubeConfig.CORE_POSITION] = 0;
            BuildChildDepths();
        }

        public GolemData Copy()
        {
            Dictionary<Vector3Int, CubeData> cubeDatas = new Dictionary<Vector3Int, CubeData>();
            foreach (KeyValuePair<Vector3Int, CubeData> cubeData in CubeDatas)
                cubeDatas[cubeData.Key] = cubeData.Value.Copy();

            Dictionary<Vector3Int, List<Vector3Int>> children = new Dictionary<Vector3Int, List<Vector3Int>>();
            if (Children != null)
            {
                foreach (KeyValuePair<Vector3Int, List<Vector3Int>> data in Children)
                {
                    List<Vector3Int> list = new List<Vector3Int>();
                    list.AddRange(data.Value);
                    children.Add(data.Key, list);
                }
            }

            return new GolemData(MoveSpeedPoint, cubeDatas, children);
        }

        public int GetDirectionEdge(Enums.Direction3D direction)
        {
            return FindEdge((int)direction / 2, (int)direction % 2 == 0);
        }

        public List<CubeData> FindEdgeCubes(Enums.Direction3D direction)
        {
            return FindEdgeCubes((int)direction / 2, GetDirectionEdge(direction));
        }

        public List<Vector3Int> GetAddablePositions(Vector3Int parentPosition)
        {
            List<Vector3Int> list = new List<Vector3Int>();
            int length = Enum.GetValues(typeof(Enums.Direction3D)).Length;

            for (int i = 0; i < length; i++)
            {
                Vector3Int position = parentPosition + DirectionEnumUtils.GetDirection3DValue((Enums.Direction3D)i);
                if (!_cubeDatas.ContainsKey(position))
                    list.Add(position);
            }

            return list;
        }

        public List<CubeData> FindChildren(Vector3Int parentPosition)
        {
            List<CubeData> list = new List<CubeData>();
            FindChildren(parentPosition, list);

            return list;
        }

        internal bool TryAddCube(Vector3Int parentPosition, Enums.Direction3D direction, out Vector3Int position)
        {
            position = parentPosition;
            if (!_cubeDatas.ContainsKey(parentPosition))
            {
                Debug.LogError($"This Golem has not this Position({parentPosition})");
                return false;
            }

            position = parentPosition + DirectionEnumUtils.GetDirection3DValue(direction);
            if (_cubeDatas.ContainsKey(position))
            {
                Debug.LogError($"{position} is already exist");
                return false;
            }

            _cubeDatas[position] = _cubeDatas[parentPosition].MakeChildData(position);
            if (!_children.ContainsKey(parentPosition) || _children[parentPosition] == null)
                _children[parentPosition] = new List<Vector3Int>();
            _children[parentPosition].Add(position);
            _parents[position] = parentPosition;
            _childDepths[position] = _childDepths[parentPosition] + 1;

            return true;
        }

        private int FindEdge(int vectorIndex, bool isMax)
        {
            int result = 0;

            foreach (CubeData data in _cubeDatas.Values)
            {
                if (isMax)
                    result = Mathf.Max(result, data.ShapePoisition[vectorIndex]);
                else
                    result = Mathf.Min(result, data.ShapePoisition[vectorIndex]);
            }

            return result;
        }

        private List<CubeData> FindEdgeCubes(int vectorIndex, int edge)
        {
            List<CubeData> result = new List<CubeData>();

            foreach (CubeData data in _cubeDatas.Values)
            {
                if (data.ShapePoisition[vectorIndex] == edge)
                    result.Add(data);
            }

            return result;
        }

        private void FindChildren(Vector3Int parentPosition, List<CubeData> list)
        {
            if (!_cubeDatas.ContainsKey(parentPosition) || !_children.ContainsKey(parentPosition) || _children[parentPosition].Count == 0)
                return;

            foreach (Vector3Int position in _children[parentPosition])
            {
                list.Add(_cubeDatas[position]);
                FindChildren(position, list);
            }
        }

        private void BuildChildDepths()
        {
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            queue.Enqueue(CubeConfig.CORE_POSITION);

            while (queue.Count > 0)
            { 
                Vector3Int parent = queue.Dequeue();
                int nextDepth = _childDepths[parent] + 1;

                if (!_children.TryGetValue(parent, out List<Vector3Int> childList))
                    continue;

                foreach (Vector3Int child in childList)
                {
                    _childDepths[child] = nextDepth;
                    queue.Enqueue(child);
                }
            }
        }
    }
}