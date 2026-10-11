using System.Collections.Generic;
using UnityEngine;

namespace Commar.CubicLand
{
    public class ObjectPool : IObjectPool
    {
        private Transform _parent;

        private readonly int _maxSize;

        private readonly Dictionary<GameObject, Stack<PooledObjectHandle>> _instancePools = new Dictionary<GameObject, Stack<PooledObjectHandle>>();
        private readonly Dictionary<PooledObjectHandle, List<Component>> _instanceComponents = new Dictionary<PooledObjectHandle, List<Component>>();
        private readonly Dictionary<PooledObjectHandle, List<IPoolReleasable>> _poolReleasables = new Dictionary<PooledObjectHandle, List<IPoolReleasable>>();

        public ObjectPool(int maxSize)
        {
            _maxSize = maxSize;
        }

        public PooledObjectHandle Instantiate(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
                return null;

            CreateParent();

            if (!_instancePools.TryGetValue(prefab, out Stack<PooledObjectHandle> handleStack))
            {
                handleStack = new Stack<PooledObjectHandle>();
                _instancePools.Add(prefab, handleStack);
            }

            PooledObjectHandle handle;

            if (handleStack.Count > 0)
            {
                handle = handleStack.Pop();
            }
            else
            {
                GameObject go = Object.Instantiate(prefab, parent);
                handle = new PooledObjectHandle(prefab, go, this);
            }

            handle.IsRented = true;
            handle.GameObject.transform.SetParent(parent);
            handle.GameObject.SetActive(true);

            return handle;
        }

        public void Destroy(PooledObjectHandle handle)
        {
            if (handle == null || !handle.IsRented)
                return;

            handle.IsRented = false;

            if (!_instancePools.TryGetValue(handle.Prefab, out Stack<PooledObjectHandle> handleStack))
            {
                RemoveTracking(handle);
                Object.Destroy(handle.GameObject);
                return;
            }

            if (handleStack.Count < _maxSize)
            {
                ReleaseToPool(handle);

                handleStack.Push(handle);

                handle.GameObject.SetActive(false);
                handle.GameObject.transform.SetParent(_parent);
            }
            else
            {
                RemoveTracking(handle);
                Object.Destroy(handle.GameObject);
            }
        }

        public void DestoryPool(GameObject prefab)
        {
            if (!_instancePools.TryGetValue(prefab, out Stack<PooledObjectHandle> handleStack))
                return;

            while (handleStack.Count > 0)
            {
                PooledObjectHandle handle = handleStack.Pop();

                RemoveTracking(handle);
                Object.Destroy(handle.GameObject);
            }

            _instancePools.Remove(prefab);
        }

        public T AddComponent<T>(PooledObjectHandle handle) where T : Component
        {
            if (!_instanceComponents.TryGetValue(handle, out List<Component> components))
            {
                components = new List<Component>();
                _instanceComponents.Add(handle, components);
            }

            T component = handle.GameObject.AddComponent<T>();
            components.Add(component);

            return component;
        }

        public void RegisterPoolReleasable(PooledObjectHandle handle, IPoolReleasable poolReleasable)
        {
            if (!_poolReleasables.TryGetValue(handle, out List<IPoolReleasable> releasables))
            {
                releasables = new List<IPoolReleasable>();
                _poolReleasables.Add(handle, releasables);
            }

            if (poolReleasable != null && !releasables.Contains(poolReleasable))
                releasables.Add(poolReleasable);
        }

        private void CreateParent()
        {
            if (_parent != null)
                return;

            _parent = new GameObject("ObjectPool").transform;
            _parent.Reset();
            Object.DontDestroyOnLoad(_parent.gameObject);
        }

        private void ReleaseToPool(PooledObjectHandle handle)
        {
            if (_instanceComponents.TryGetValue(handle, out List<Component> components))
            {
                foreach (Component component in components)
                    Object.Destroy(component);

                components.Clear();
            }

            if (_poolReleasables.TryGetValue(handle, out List<IPoolReleasable> releasables))
            {
                foreach (IPoolReleasable poolReleasable in releasables)
                    poolReleasable.OnPoolReleased();
            }
        }

        private void RemoveTracking(PooledObjectHandle handle)
        {
            _instanceComponents.Remove(handle);
            _poolReleasables.Remove(handle);
        }
    }
}