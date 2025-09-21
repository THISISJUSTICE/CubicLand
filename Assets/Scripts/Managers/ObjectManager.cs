using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public class ObjectManager : SingletonMonoBehaviour<ObjectManager>
    {
        #region Variables
        private Dictionary<GameObject, Stack<GameObject>> _objects;
        private Dictionary<GameObject, GameObject> _instKeys;
        private Dictionary<GameObject, List<Component>> _componentDic;
        private Dictionary<GameObject, long> _memorySizes;

        private GameObject _emptyObject;
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
        protected override void Awake()
        {
            base.Awake();

            _objects = new Dictionary<GameObject, Stack<GameObject>>();
            _instKeys = new Dictionary<GameObject, GameObject>();
            _componentDic = new Dictionary<GameObject, List<Component>>();
            _memorySizes = new Dictionary<GameObject, long>();

            _emptyObject = new GameObject("Empty");
            _emptyObject.transform.SetParent(transform);
            _emptyObject.transform.Reset();
            _emptyObject.SetActive(false);
        }
        #endregion

        #region Public Functions
        public GameObject InstanitateObject(GameObject prefab, bool instMat = false)
        {
            if (prefab == null)
                return null;

            if (!_objects.ContainsKey(prefab))
                AddKey(prefab, instMat);

            GameObject res;

            if (_objects[prefab].Count > 0)
            {
                res = _objects[prefab].Pop();
                res.SetActive(true);
            }
            else
            {
                res = Instantiate(prefab);
                _instKeys[res] = prefab;

                if (instMat)
                {
                    Renderer baseRen = res.GetComponent<Renderer>();

                    if (baseRen != null)
                    {
                        switch (baseRen)
                        {
                            case TrailRenderer trail:
                                if (trail.sharedMaterial != null)
                                    trail.sharedMaterial = Instantiate(trail.sharedMaterial);
                                break;
                            case LineRenderer line:
                                if (line.sharedMaterial != null)
                                    line.sharedMaterial = Instantiate(line.sharedMaterial);
                                break;
                            case ParticleSystemRenderer particle:
                                {
                                    var sharedMats = particle.sharedMaterials;
                                    var newMats = new Material[sharedMats.Length];

                                    for (int i = 0; i < sharedMats.Length; i++)
                                        newMats[i] = sharedMats[i] != null ? Instantiate(sharedMats[i]) : null;

                                    particle.sharedMaterials = newMats;
                                }
                                break;
                            default:
                                {
                                    var sharedMats = baseRen.sharedMaterials;
                                    var newMats = new Material[sharedMats.Length];

                                    for (int i = 0; i < sharedMats.Length; i++)
                                        newMats[i] = sharedMats[i] != null ? Object.Instantiate(sharedMats[i]) : null;

                                    baseRen.sharedMaterials = newMats;
                                }
                                break;
                        }
                    }
                }
            }

            res.transform.Reset();

            return res;
        }

        public GameObject GetEmptyObject(string objectName = "")
        {
            GameObject res = InstanitateObject(_emptyObject);

            if (!string.IsNullOrEmpty(objectName))
                res.name = objectName;

            return res;
        }

        public void DestroyObject(GameObject go)
        {
            if (CurrentMemorySize < PerfomanceConfigs.MemoryLimit) // 메모리 여유가 있는 경우 풀링
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
            else
            {
                RemoveObject(go);
            }
        }

        public void DeleteObject(GameObject prefab)
        {
            while (_objects[prefab].Count > 0)
            {
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
            if (component != null)
                _componentDic[go].Add(component);

            return component;
        }
        #endregion

        #region Utils
        private void AddKey(GameObject prefab, bool instMat)
        {
            _objects[prefab] = new Stack<GameObject>();
            _memorySizes[prefab] = PerfomanceConfigs.EstimateGameObjectMemory(prefab, instMat);
        }

        private void RemoveObject(GameObject go)
        {
            if (_instKeys.ContainsKey(go))
            {
                _instKeys.Remove(go);
            }
            GameObject.Destroy(go);
        }
        #endregion
    }
}