using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    public const string SingletoneParentName = "Singletones";

    private static bool _quit = false;
    private static Object _lock = new Object();
    private static T _instance;

    public static T Instance {
        get {
            if (_quit) 
                return null;

            lock (_lock) {
                if (_instance == null) {
                    _instance = (T)GameObject.FindAnyObjectByType<T>();
                    if (_instance == null) {
                        string goName = typeof(T).ToString();
                        if(goName.Substring(0, 2) == "DV")
                            goName = goName.Substring(2);

                        GameObject go = new GameObject(goName);
                        _instance = go.AddComponent<T>();

                        GameObject parent = GameObject.Find(SingletoneParentName);
                        if (parent == null)
                        {
                            parent = new GameObject(SingletoneParentName);
                            DontDestroyOnLoad(parent);
                        }
                        go.transform.SetParent(parent.transform);
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
