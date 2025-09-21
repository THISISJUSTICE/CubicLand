using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePersistor : MonoBehaviour
{
    #region Variables
    [SerializeField] private int _flag;
    public int Flag { get => _flag; }

    private static Dictionary<int, ScenePersistor> _instances;

    private string _sceneName;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        if (_instances == null)
            _instances = new Dictionary<int, ScenePersistor>();

        if (CheckInstanceDifferent(this))
            return;

        _sceneName = SceneManager.GetActiveScene().name;
        _instances[_flag] = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (CheckInstanceDifferent(this))
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        _instances[_flag] = null;
        _instances.Remove(_flag);
    }
    #endregion

    private bool CheckInstanceDifferent(ScenePersistor persistor) {
        return _instances.ContainsKey(_flag) && _instances[_flag] != null && _instances[_flag] != persistor;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != _sceneName)
        {
            var persistors = GameObject.FindObjectsByType<ScenePersistor>(FindObjectsSortMode.None);

            if (persistors.Length < 2) {
                Destroy(gameObject);
                return;
            }

            bool check = false;
            foreach (var persistor in persistors) {
                if (persistor != null && persistor.Flag == _flag && persistor != this) {
                    check = true;
                    Destroy(persistor.gameObject);
                }
            }

            if (!check) {
                Destroy(gameObject);
            }
        }
    }

}
