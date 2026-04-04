using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public class GolemData
    {
        public int MoveSpeedPoint { get; private set; }

        private readonly Dictionary<Vector3Int, CubeData> _cubeDatas; // (0,0,0)Àº Core (R:x+1, L:x-1, U:y+1, D:y-1, F:z+1, B:z-1)
        private readonly Dictionary<Vector3Int, List<Vector3Int>> _childs;
        private readonly Dictionary<Vector3Int, Vector3Int> _parents;

        public IReadOnlyDictionary<Vector3Int, CubeData> CubeDatas => _cubeDatas;
        public IReadOnlyDictionary<Vector3Int, List<Vector3Int>> Childs => _childs;
        public IReadOnlyDictionary<Vector3Int, Vector3Int> Parents => _parents;

        public GolemData(int moveSpeedPoint = 0,
            Dictionary<Vector3Int, CubeData> cubeDatas = null,
            Dictionary<Vector3Int, List<Vector3Int>> childs = null,
            Dictionary<Vector3Int, Vector3Int> parents = null)
        {
            MoveSpeedPoint = moveSpeedPoint;

            if (cubeDatas != null)
                _cubeDatas = cubeDatas;
            else
                _cubeDatas = new Dictionary<Vector3Int, CubeData>()
                { { Vector3Int.zero, new CubeData(new StatusPoint(), Vector3Int.zero, true, Color.white) } };

            if (childs != null)
                _childs = childs;
            else
                _childs = new Dictionary<Vector3Int, List<Vector3Int>>();

            if (parents != null)
                _parents = parents;
            else
                _parents = new Dictionary<Vector3Int, Vector3Int>();
        }

        public GolemData Copy()
        {
            Dictionary<Vector3Int, CubeData> cubeDatas = new Dictionary<Vector3Int, CubeData>();
            foreach (KeyValuePair<Vector3Int, CubeData> cubeData in CubeDatas)
                cubeDatas[cubeData.Key] = cubeData.Value.Copy();

            Dictionary<Vector3Int, List<Vector3Int>> childs = new Dictionary<Vector3Int, List<Vector3Int>>();
            if (Childs != null)
            {
                foreach (KeyValuePair<Vector3Int, List<Vector3Int>> data in Childs)
                {
                    List<Vector3Int> list = new List<Vector3Int>();
                    foreach (Vector3Int position in data.Value)
                        list.Add(position);

                    childs.Add(data.Key, list);
                }
            }

            Dictionary<Vector3Int, Vector3Int> parents = new Dictionary<Vector3Int, Vector3Int>();
            if (Parents != null)
            {
                foreach (KeyValuePair<Vector3Int, Vector3Int> data in Parents)
                    parents.Add(data.Key, data.Value);
            }

            return new GolemData(MoveSpeedPoint, cubeDatas, childs, parents);
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

        public bool AddCube(Vector3Int parentPosition, Enums.Direction3D direction)
        {
            if (!_cubeDatas.ContainsKey(parentPosition))
            {
                Debug.LogError($"This Golem has not this Position({parentPosition})");
                return false;
            }

            Vector3Int position = parentPosition + DirectionEnumUtils.GetDirection3DValue(direction);
            if (_cubeDatas.ContainsKey(position))
            {
                Debug.LogError($"{position} is already exist");
                return false;
            }

            _cubeDatas[position] = _cubeDatas[parentPosition].MakeChildData(position);
            if (!_childs.ContainsKey(parentPosition) || _childs[parentPosition] == null)
                _childs[parentPosition] = new List<Vector3Int>();
            _childs[parentPosition].Add(position);
            _parents[position] = parentPosition;

            return true;
        }

        public void RemoveCube(Vector3Int position)
        {
            if (!_cubeDatas.ContainsKey(position))
                return;

            if (position == Vector3Int.zero)
            {
                _cubeDatas.Clear();
                _childs.Clear();
                _parents.Clear();
                return;
            }

            _cubeDatas.Remove(position);
            _parents.Remove(position);

            if (_childs.ContainsKey(position))
            {
                foreach (Vector3Int child in _childs[position])
                    RemoveCube(child);
                _childs.Remove(position);
            }
        }

        private int FindEdge(int vectorIndex, bool isMax)
        {
            int result = 0;

            foreach (CubeData data in CubeDatas.Values)
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

            foreach (CubeData data in CubeDatas.Values)
            {
                if (data.ShapePoisition[vectorIndex] == edge)
                    result.Add(data);
            }

            return result;
        }
    }
}