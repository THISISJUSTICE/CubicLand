using UnityEngine;
using System.Collections.Generic;

namespace CustomTIJI.CubicLand
{
    public class ObjectPool : IObjectPool
    {
        private Transform _parent;

        private readonly int _maxSize;

        private readonly Dictionary<GameObject, Stack<PooledObjectHandle>> _instancePools = new Dictionary<GameObject, Stack<PooledObjectHandle>>();
        private readonly Dictionary<PooledObjectHandle, List<Component>> _instanceComponents = new Dictionary<PooledObjectHandle, List<Component>>();

        public ObjectPool(int maxSize)
        { 
            _maxSize = maxSize;
        }

        public PooledObjectHandle Instantiate(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
                return null;

            CreateParent();

            if (!_instancePools.ContainsKey(prefab))
                _instancePools.Add(prefab, new Stack<PooledObjectHandle>());

            PooledObjectHandle handle;

            if (_instancePools[prefab].Count > 0)
            {
                handle = _instancePools[prefab].Pop();
                handle.GameObject.SetActive(true);
            }
            else
            {
                GameObject go = Object.Instantiate(prefab);
                handle = new PooledObjectHandle(prefab, go, this);
            }

            handle.GameObject.transform.SetParent(parent);

            return handle;
        }

        public void Destroy(PooledObjectHandle handle)
        {
            GameObject prefab = handle.Prefab;

            if (_instancePools[prefab].Count < _maxSize)
            {
                ReleaseComponentInstance(handle);

                _instancePools[prefab].Push(handle);

                handle.GameObject.SetActive(false);
                handle.GameObject.transform.SetParent(_parent);
            }
            else
            {
                Object.Destroy(handle.GameObject);
            }
        }

        public void DestoryPool(GameObject prefab)
        {
            while (_instancePools[prefab].Count > 0)
            {
                PooledObjectHandle handle = _instancePools[prefab].Pop();
                Object.Destroy(handle.GameObject);
            }

            _instancePools.Remove(prefab);
        }

        public T AddComponent<T>(PooledObjectHandle handle) where T : Component
        {
            if (!_instanceComponents.ContainsKey(handle))
                _instanceComponents.Add(handle, new List<Component>());

            T component = handle.GameObject.AddComponent<T>();
            _instanceComponents[handle].Add(component);

            return component;
        }

        private void CreateParent()
        {
            if (_parent != null)
                return;

            _parent = new GameObject("ObjectPool").transform;
            _parent.Reset();
            Object.DontDestroyOnLoad(_parent.gameObject);
        }

        private void ReleaseComponentInstance(PooledObjectHandle handle)
        {
            if (_instanceComponents.ContainsKey(handle))
            {
                foreach (Component component in _instanceComponents[handle])
                    Object.Destroy(component);

                _instanceComponents.Clear();
            }
        }
    }
}