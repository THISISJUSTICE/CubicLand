using System.Collections.Generic;
using UnityEngine;

public class DVObjectManager : SingletonMonoBehaviour<DVObjectManager>
{
    #region Variables
    private Dictionary<GameObject, Stack<GameObject>> _objects;
    private Dictionary<GameObject, GameObject> _instKeys;
    private Dictionary<GameObject, List<Component>> _componentDic;
    private Dictionary<GameObject, long> _memorySizes;
    #endregion

    #region Properties
    private long CurrentMemorySize
    {
        get
        {
            long memorySize = 0;

            foreach (var memory in _memorySizes)
            {
                memorySize += _objects[memory.Key].Count * memory.Value;
            }

            return memorySize;
        }
    }
    #endregion

    #region Unity Functions
    private void Awake()
    {
        _objects = new Dictionary<GameObject, Stack<GameObject>>();
        _instKeys = new Dictionary<GameObject, GameObject>();
        _componentDic = new Dictionary<GameObject, List<Component>>();
        _memorySizes = new Dictionary<GameObject, long>();
    }
    #endregion

    #region Public Functions
    public GameObject InstanitateObject(GameObject prefab, bool instMat = false) {
        if (prefab == null)
            return null;

        if(!_objects.ContainsKey(prefab))
            AddKey(prefab, instMat);

        GameObject res;

        if (_objects[prefab].Count > 0)
        {
            res = _objects[prefab].Pop();
        }
        else
        {
            res = GameObject.Instantiate(prefab);
            _instKeys[res] = prefab;

            if (instMat) { 
                Renderer baseRen = prefab.GetComponent<Renderer>();
                Renderer ren = res.GetComponent<Renderer>();

                if(baseRen.sharedMaterial != null)
                    ren.sharedMaterial = GameObject.Instantiate(baseRen.sharedMaterial);
            }
        }

        res.transform.Reset();

        return res;
    }

    public void DestroyObject(GameObject go) {
        if (CurrentMemorySize < DVPerfomanceConfigs.MemoryLimit) // 메모리 여유가 있는 경우 풀링
        {
            if (_componentDic.ContainsKey(go))
            {
                foreach (var component in _componentDic[go])
                {
                    Destroy(component);
                }
                _componentDic[go].Clear();
            }

            _objects[_instKeys[go]].Push(go);
            go.SetActive(false);
            go.transform.SetParent(gameObject.transform);
        }
        else {
            RemoveObject(go);
        }
    }

    public void DeleteObject(GameObject prefab) {
        while (_objects[prefab].Count > 0) { 
            GameObject obj = _objects[prefab].Pop();
            DestroyObject(obj);
        }

        _objects.Remove(prefab);
        _memorySizes.Remove(prefab);
    }

    public T AddComponent<T>(GameObject go) where T : Component
    {
        if (!_componentDic.ContainsKey(go))
            _componentDic[go] = new List<Component>();

        T component = go.AddComponent<T>();
        if(component != null) 
            _componentDic[go].Add(component);

        return component;
    }
    #endregion

    #region Utils
    private void AddKey(GameObject prefab, bool instMat) {
        _objects[prefab] = new Stack<GameObject>();
        _memorySizes[prefab] = DVPerfomanceConfigs.EstimateGameObjectMemory(prefab, instMat);
    }

    private void RemoveObject(GameObject go)
    {
        if (_instKeys.ContainsKey(go)) { 
            _instKeys.Remove(go);
        }
        GameObject.Destroy(go);
    }
    #endregion
}
