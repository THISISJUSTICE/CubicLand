using UnityEngine;

public class DVIntroManager : MonoBehaviour
{

    #region Unity Functions
    private void Awake()
    {
        CreateSingletones();
        
    }

    private void Start()
    {
        DVSceneConfigs.LoadScene(DVSceneConfigs.SceneList.MAP);
    }
    #endregion

    #region Utils
    private void CreateSingletones() {
        _ = DVKeyboardManager.Instance;
    }
    #endregion
}
