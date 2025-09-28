using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public class ObjectManager : SingletonMonoBehaviour<ObjectManager>
    {
        [SerializeField] private int _maxSizeInWindow = 1000;
        [SerializeField] private int _maxSizeInMobile = 100;

        private int MaxSize
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                return _maxSizeInWindow;
#else
                return _maxSizeInMobile;
#endif
            }
        }

        private readonly Dictionary<GameObject, Stack<GameObjectInstance>> _instancePools = new Dictionary<GameObject, Stack<GameObjectInstance>>();
        private readonly Dictionary<GameObjectInstance, GameObject> _instanceOrigins = new Dictionary<GameObjectInstance, GameObject>();
        private readonly Dictionary<GameObjectInstance, GameObject> _instanceBases = new Dictionary<GameObjectInstance, GameObject>();
        private readonly Dictionary<GameObject, GameObjectInstance> _instances = new Dictionary<GameObject, GameObjectInstance>();
        private readonly Dictionary<GameObjectInstance, List<Component>> _instanceComponents = new Dictionary<GameObjectInstance, List<Component>>();

        private GameObject _emptyObject;

        protected override void Awake()
        {
            base.Awake();

            _emptyObject = new GameObject("Empty");
            _emptyObject.transform.SetParent(transform);
            _emptyObject.transform.Reset();
            _emptyObject.SetActive(false);
        }

        public GameObjectInstance InstanitateGameObject(GameObject prefab, Transform parent = null, bool useInstanceMaterial = false)
        {
            if (prefab == null)
                return null;

            if (!_instancePools.ContainsKey(prefab))
                _instancePools.Add(prefab, new Stack<GameObjectInstance>());

            GameObjectInstance res;

            if (_instancePools[prefab].Count > 0)
            {
                res = _instancePools[prefab].Pop();
                _instanceBases[res].SetActive(true);
            }
            else
            {
                GameObject go = Instantiate(prefab);
                res = new GameObjectInstance(go);

                _instanceOrigins.Add(res, prefab);
                _instanceBases.Add(res, go);
                _instances.Add(go, res);

                if (useInstanceMaterial)
                    InstantiateMaterial(go);
            }

            if(parent == null)
                parent = transform;

            _instanceBases[res].transform.SetParent(parent);
            _instanceBases[res].transform.Reset();

            return res;
        }

        public static GameObjectInstance InstantiateEmptyObject(string objectName = "empty", Transform parent = null)
        {
            GameObjectInstance res = Instance.InstanitateGameObject(Instance._emptyObject, parent);

            if (!string.IsNullOrEmpty(objectName))
                res.Name = objectName;

            return res;
        }

        public void DestroyGameObject(GameObjectInstance instance)
        {
            GameObject origin = _instanceOrigins[instance];

            if (_instancePools[origin].Count < MaxSize)
            {
                ReleaseComponentInstance(instance);

                _instancePools[origin].Push(instance);

                GameObject baseGo = _instanceBases[instance];
                baseGo.SetActive(false);
                baseGo.transform.SetParent(transform);
            }
            else
            {
                RemoveGameObject(instance);
            }
        }

        public void DestroyGameObject(GameObject go)
        {
            if (_instances.TryGetValue(go, out GameObjectInstance instance))
                DestroyGameObject(instance);
            else
                Destroy(go);
        }

        public T AddComponent<T>(GameObjectInstance instance) where T : Component
        {
            if (!_instanceComponents.ContainsKey(instance))
                _instanceComponents.Add(instance, new List<Component>());

            T component = _instanceBases[instance].AddComponent<T>();
            _instanceComponents[instance].Add(component);

            return component;
        }

        public void Clear(GameObject prefab)
        {
            while (_instancePools[prefab].Count > 0)
            {
                GameObjectInstance instance = _instancePools[prefab].Pop();
                RemoveGameObject(instance);
            }

            _instancePools.Remove(prefab);
        }

        private void InstantiateMaterial(GameObject go)
        {
            Renderer renderer = go.GetComponent<Renderer>();

            if (renderer == null)
                return;

            Material[] sharedMaterials = renderer.sharedMaterials;
            Material[] materialInstances = new Material[sharedMaterials.Length];

            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                if (sharedMaterials[i] == null)
                    materialInstances = null;
                else
                    materialInstances[i] = Instantiate(sharedMaterials[i]);
            }

            renderer.sharedMaterials = materialInstances;
        }

        private void RemoveGameObject(GameObjectInstance instance)
        {
            if (!_instanceBases.ContainsKey(instance))
                return;

            ReleaseComponentInstance(instance);
            ReleaseMaterialInstance(instance);

            GameObject go = _instanceBases[instance];
            _instanceBases.Remove(instance);
            _instances.Remove(go);

            if (_instanceOrigins.ContainsKey(instance))
                _instanceOrigins.Remove(instance);

            Destroy(go);
        }

        private void ReleaseComponentInstance(GameObjectInstance instance)
        {
            if (_instanceComponents.ContainsKey(instance))
            {
                foreach (Component component in _instanceComponents[instance])
                    Destroy(component);

                _instanceComponents.Clear();
            }
        }

        private void ReleaseMaterialInstance(GameObjectInstance instance)
        {
            Renderer renderer = _instanceBases[instance].GetComponent<Renderer>();

            if (renderer == null)
                return;

            Material[] sharedMaterials = renderer.sharedMaterials;
            foreach (Material material in sharedMaterials)
            {
                if (material != null)
                    Resources.UnloadAsset(material);
            }
        }
    }
}