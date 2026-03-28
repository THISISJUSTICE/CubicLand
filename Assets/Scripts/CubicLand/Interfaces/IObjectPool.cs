using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public interface IObjectPool
    {
        public PooledObjectHandle Instantiate(GameObject prefab, Transform parent = null);
        public void Destroy(PooledObjectHandle handle);
        public void DestoryPool(GameObject prefab);
        public T AddComponent<T>(PooledObjectHandle handle) where T : Component;
    }
}