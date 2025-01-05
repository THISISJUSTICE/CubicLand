using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DVGolemInfo
{
    public DVStatus Status;
    public HashSet<Vector3Int> Shape;// (0,0,0)은 Core (R:x+1, L:x-1, U:y+1, D:y-1, F:z+1, B:z-1)
    public Dictionary<Vector3Int, HashSet<Vector3Int>> ChildMap;
    public Dictionary<Vector3Int, Vector3Int> ParentMap;

    // TODO: 보유 스킬 및 스킬 레벨

    public DVGolemInfo(bool init = true) {
        Status = new DVStatus();
        Status.SetInitValue();
        Shape = new HashSet<Vector3Int>();
        Shape.Add(Vector3Int.zero);
        ChildMap = new Dictionary<Vector3Int, HashSet<Vector3Int>>();
        ParentMap = new Dictionary<Vector3Int, Vector3Int>();
    }

    public DVGolemInfo(DVStatus status, HashSet<Vector3Int> shape, 
        Dictionary<Vector3Int, HashSet<Vector3Int>> childMap,
        Dictionary<Vector3Int, Vector3Int> parentMap) { 
        Status = status;
        Shape = shape;
        ChildMap = childMap;
        ParentMap = parentMap;
    }
}

public static class DVGolemInfoExtensions {
    public static int GetDirectionSize(this DVGolemInfo golemInfo, DVEnums.Direction3D direction)
    {
        switch (direction)
        {
            case DVEnums.Direction3D.RIGHT:
                return GetSize(golemInfo, 0, true);
            case DVEnums.Direction3D.LEFT:
                return GetSize(golemInfo, 0, false);
            case DVEnums.Direction3D.UP:
                return GetSize(golemInfo, 1, true);
            case DVEnums.Direction3D.DOWN:
                return GetSize(golemInfo, 1, false);
            case DVEnums.Direction3D.FRONT:
                return GetSize(golemInfo, 2, true);
            case DVEnums.Direction3D.BACK:
                return GetSize(golemInfo, 2, false);
            default:
                return 0;
        }
    }

    private static int GetSize(DVGolemInfo golemInfo, int vectorDirection, bool isMax)
    {
        int value = 0;

        foreach (Vector3Int pos in golemInfo.Shape)
        {
            if (isMax)
            {
                switch (vectorDirection)
                {
                    case 0:
                        value = (int)Mathf.Max(value, pos.x);
                        break;
                    case 1:
                        value = (int)Mathf.Max(value, pos.y);
                        break;
                    default:
                        value = (int)Mathf.Max(value, pos.z);
                        break;
                }
            }
            else
            {
                switch (vectorDirection)
                {
                    case 0:
                        value = (int)Mathf.Min(value, pos.x);
                        break;
                    case 1:
                        value = (int)Mathf.Min(value, pos.y);
                        break;
                    default:
                        value = (int)Mathf.Min(value, pos.z);
                        break;
                }
            }
        }

        return value + 1;
    }

    public static Vector3Int[] GetAbleAddCubePosition(this DVGolemInfo golemInfo, Vector3Int parentPos) { 
        List<Vector3Int> list = new List<Vector3Int>();
        for (int i = 0; i < DVUtil.Direction3DLength; i++) {
            Vector3Int childPos = parentPos + DVUtil.GetDirection3DValue((DVEnums.Direction3D)i);
            if(!golemInfo.Shape.Contains(childPos))
                list.Add(childPos);
        }

        return list.ToArray();
    }

    public static bool AddCube(this DVGolemInfo golemInfo, Vector3Int parentPos, DVEnums.Direction3D direction)
    {
        Vector3Int childPos = parentPos + DVUtil.GetDirection3DValue(direction);
        if (golemInfo.Shape.Contains(childPos)) {
            Debug.Log($"{childPos} is already exist");
            return false;
        }

        golemInfo.Shape.Add(childPos);
        if (golemInfo.ChildMap[parentPos] == null)
            golemInfo.ChildMap[parentPos] = new HashSet<Vector3Int>();
        golemInfo.ChildMap[parentPos].Add(childPos);
        golemInfo.ParentMap[childPos] = parentPos;

        return true;  
    }

    public static bool RemoveCube(this DVGolemInfo golemInfo, Vector3Int cubePos)
    {
        if (golemInfo.Shape.Contains(cubePos))
        {
            Debug.Log($"{cubePos} is not exist");
            return false;
        }

        golemInfo.Shape.Remove(cubePos);

        List<Vector3Int> childPosList = GetChilds(golemInfo, cubePos, remove: true);
        foreach (Vector3Int childPos in childPosList) { 
            golemInfo.ParentMap.Remove(childPos);
        }

        return true;
    }

    private static List<Vector3Int> GetChilds(DVGolemInfo golemInfo, Vector3Int cubePos, bool remove = false) {
        List<Vector3Int> list = new List<Vector3Int>();
        List<Vector3Int> childList = new List<Vector3Int>();
        if (golemInfo.ChildMap[cubePos] != null && golemInfo.ChildMap[cubePos].Count > 0)
        {
            foreach (var childPos in golemInfo.ChildMap[cubePos])
            {
                list.Add(childPos);
            }

            if(remove)
                golemInfo.ChildMap.Remove(cubePos);

            foreach (var childPos in list) { 
                var childs = GetChilds(golemInfo, childPos, remove);
                foreach (var child in childs) { 
                    childList.Add(child);
                }
            }
        }

        foreach (var childPos in childList) { 
            list.Add(childPos);
        }

        return list;
    }
}
