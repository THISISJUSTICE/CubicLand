using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public class PooledObjectHandle
    {
        public GameObject Prefab => _prefab;
        public GameObject GameObject => _gameObject;

        private readonly GameObject _prefab;
        private readonly GameObject _gameObject;
        private readonly IObjectPool _pool;

        public PooledObjectHandle(GameObject prefab, GameObject gameObject, IObjectPool pool)
        {
            _prefab = prefab;
            _gameObject = gameObject;
            _pool = pool;
        }

        public PooledObjectHandle Instantiate(Transform parent = null)
        {
            return _pool.Instantiate(_prefab, parent);
        }

        public void Destroy()
        {
            _pool.Destroy(this);
        }

        public void DestoryPool()
        { 
            _pool.DestoryPool(_prefab);
        }
    }
}