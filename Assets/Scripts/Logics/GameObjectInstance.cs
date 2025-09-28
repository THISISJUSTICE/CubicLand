using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomTIJI.CubicLand
{
    public class GameObjectInstance
    {
        private GameObject _gameObject;

        public GameObjectInstance(GameObject gameObject)
        { 
            _gameObject = gameObject;
        }

        public static GameObjectInstance InstanitateObject(GameObject prefab, Transform parent = null, bool useInstanceMaterial = false)
        {
            return ObjectManager.Instance.InstanitateGameObject(prefab, parent, useInstanceMaterial);
        }

        #region GameObject Properties
        public string Name
        { 
            get => _gameObject.name;
            set => _gameObject.name = value;
        }

        public Vector3 Position
        {
            get => _gameObject.transform.position;
            set => _gameObject.transform.position = value;
        }

        public Quaternion Rotation
        {
            get => _gameObject.transform.rotation;
            set => _gameObject.transform.rotation = value;
        }

        public Vector3 Scale
        {
            get => _gameObject.transform.localScale;
            set => _gameObject.transform.localScale = value;
        }

        public bool ActiveSelf => _gameObject.activeSelf;

        public bool ActiveInHierarchy => _gameObject.activeInHierarchy;

        public Scene Scene => _gameObject.scene;

        public string Tag
        { 
            get => _gameObject.tag;
            set => _gameObject.tag = value;
        }

        public int Layer
        {
            get => _gameObject.layer;
            set => _gameObject.layer = value;
        }

        public Transform Parent => _gameObject.transform.parent;
        #endregion

        public T GetComponent<T>() where T : Component
        { 
            return _gameObject.GetComponent<T>();
        }

        public T AddComponent<T>() where T : Component
        {
            return ObjectManager.Instance.AddComponent<T>(this);
        }

        public T GetOrAddComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if(component == null)
                component = AddComponent<T>();  

            return component;
        }

        public bool CompareTag(string tag)
        { 
            return _gameObject.CompareTag(tag);
        }

        public void Destroy()
        { 
            ObjectManager.Instance.DestroyGameObject(this);
        }
    }
}