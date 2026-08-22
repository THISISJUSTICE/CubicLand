using System;
using System.Collections.Generic;
using UnityEngine;

namespace Commar.CubicLand.Cube
{
    public class GolemData
    {
        /*TODO: 저장 대상*/
        private readonly Dictionary<Vector3Int, CubeData> _cubeDatas; // (0,0,0)은 Core (R:x+1, L:x-1, U:y+1, D:y-1, F:z+1, B:z-1)
        /*TODO: 저장 대상*/
        private readonly Dictionary<Vector3Int, List<Vector3Int>> _children;
        private readonly Dictionary<Vector3Int, Vector3Int> _parents;
        private readonly Dictionary<Vector3Int, int> _childDepths;

        private readonly List<Vector3Int> _availables = new List<Vector3Int>();
        private readonly Dictionary<Vector3Int, CubeData> _visibleCubes = new Dictionary<Vector3Int, CubeData>();

        /*TODO: 저장 대상*/
        public int MoveSpeedPoint { get; set; }
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

        public GolemData Clone()
        {
            Dictionary<Vector3Int, CubeData> cubeDatas = new Dictionary<Vector3Int, CubeData>();
            foreach (KeyValuePair<Vector3Int, CubeData> cubeData in CubeDatas)
                cubeDatas[cubeData.Key] = cubeData.Value.Clone();

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

        public int GetDirectionLength(Enums.Direction3D direction)
        {
            int vectorIndex = (int)direction / 2;
            bool isMax = (int)direction % 2 == 0;

            SetAvailables();

            int result = 0;

            foreach (Vector3Int position in _availables)
            {
                if (!_cubeDatas.TryGetValue(position, out CubeData data))
                    continue;

                if (isMax)
                    result = Mathf.Max(result, data.ShapePoisition[vectorIndex]);
                else
                    result = Mathf.Min(result, data.ShapePoisition[vectorIndex]);
            }

            return result;
        }

        public void FindEdgeCubes(Enums.Direction3D direction, IList<CubeData> list)
        {
            if (list == null)
                return;

            int vectorIndex = (int)direction / 2;
            int edge = GetDirectionLength(direction);

            SetAvailables();
            list.Clear();

            foreach (Vector3Int position in _availables)
            {
                if (!_cubeDatas.TryGetValue(position, out CubeData data))
                    continue;

                if (data.ShapePoisition[vectorIndex] == edge)
                    list.Add(data);
            }
        }

        public void FindVisibleCubes(Enums.Direction3D direction, IList<CubeData> list)
        {
            if (list == null)
                return;

            int vectorIndex = (int)direction / 2;
            bool isMax = (int)direction % 2 == 0;

            SetAvailables();
            _visibleCubes.Clear();
            list.Clear();

            foreach (Vector3Int position in _availables)
            {
                if (!_cubeDatas.TryGetValue(position, out CubeData data))
                    continue;

                Vector3Int column = position;
                column[vectorIndex] = 0;

                if (!_visibleCubes.TryGetValue(column, out CubeData visibleCube))
                {
                    _visibleCubes.Add(column, data);
                    continue;
                }

                int axisPosition = position[vectorIndex];
                int visibleAxisPosition = visibleCube.ShapePoisition[vectorIndex];

                if ((isMax && axisPosition > visibleAxisPosition)
                    || (!isMax && axisPosition < visibleAxisPosition))
                    _visibleCubes[column] = data;
            }

            foreach (CubeData visibleCube in _visibleCubes.Values)
                list.Add(visibleCube);
        }

        public void GetAddablePositions(Vector3Int parentPosition, IList<Vector3Int> list)
        {
            if (list == null)
                return;

            list.Clear();
            int length = Enum.GetValues(typeof(Enums.Direction3D)).Length;

            for (int i = 0; i < length; i++)
            {
                Vector3Int position = parentPosition + DirectionEnumUtils.GetDirection3DValue((Enums.Direction3D)i);
                if (!_cubeDatas.ContainsKey(position))
                    list.Add(position);
            }
        }

        public void FindChildren(Vector3Int parentPosition, IList<CubeData> list)
        {
            if (list == null)
                return;

            list.Clear();
            AddChildren(parentPosition, list);
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

        private void SetAvailables()
        {
            _availables.Clear();
            foreach (CubeData data in _cubeDatas.Values)
            {
                if (!data.IsBreaked)
                    _availables.Add(data.ShapePoisition);
            }
        }

        private void AddChildren(Vector3Int parentPosition, IList<CubeData> list)
        {
            if (!_cubeDatas.ContainsKey(parentPosition) || !_children.ContainsKey(parentPosition) || _children[parentPosition].Count == 0)
                return;

            foreach (Vector3Int position in _children[parentPosition])
            {
                list.Add(_cubeDatas[position]);
                AddChildren(position, list);
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