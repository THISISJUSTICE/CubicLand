using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace CustomTIJI
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static Transform singletonParent = null;

        private static bool _quit = false;
        private static Object _lock = new Object();
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_quit)
                    return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Test가 아닌 이상 Find 했을 시 null이 나오면 안 됨
                        _instance = FindFirstObjectByType<T>();

#if UNITY_EDITOR
                        // Editor 내 Test
                        if (_instance == null)
                        {
                            MethodInfo mainMethod = ExternableEditor.FindStaticMethod("DVSingletonCreator", "LoadSingleton");
                            MethodInfo method = mainMethod.MakeGenericMethod(typeof(T));

                            _instance = (T)method.Invoke(null, null);
                        }
#endif

                        SetSingletonParent(_instance.transform);
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            SetSingleton();
        }

        protected virtual void OnApplicationQuit()
        {
            _quit = true;
        }

        protected virtual void OnDestroy()
        {
            _quit = true;
        }

        public static void SetSingletonParent(Transform singleton)
        {
            if (singletonParent == null)
            {
                const string singletonName = "Singletones";

                GameObject parent = GameObject.Find(singletonName);
                if (parent == null)
                {
                    parent = new GameObject(singletonName);
                    parent.transform.Reset();
                }

#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                    DontDestroyOnLoad(parent);
#else
            DontDestroyOnLoad(parent);
#endif
                singletonParent = parent.transform;
            }

            singleton.SetParent(singletonParent);
            singleton.Reset();
        }

        private void SetSingleton()
        {
            if (_instance == null || (_instance != null && _instance.gameObject == gameObject))
            {
                _instance = this as T;
                SetSingletonParent(gameObject.transform);
            }
            else
                Destroy(gameObject);
        }
    }
}