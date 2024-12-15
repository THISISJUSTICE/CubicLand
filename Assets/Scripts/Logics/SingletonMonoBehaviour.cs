using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool _quit = false;
    private static Object _lock = new Object();
    private static T _instance;

    public static T Instance {
        get {
            if (_quit) {

                return null;
            }

            lock (_lock) {
                if (_instance == null) {
                    _instance = (T)GameObject.FindAnyObjectByType<T>();
                    if (_instance == null) {
                        GameObject go = new GameObject($"(Singleton){typeof(T).ToString().Replace("DV", "")}");
                        _instance = go.AddComponent<T>();

                        DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }
    }

    private void OnApplicationQuit()
    {
        _quit = true;
    }

    private void OnDestroy()
    {
        _quit = true;
    }
}
