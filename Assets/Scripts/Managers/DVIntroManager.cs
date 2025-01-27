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
        LimitFrameRate();

        DVSceneConfigs.LoadScene(DVSceneConfigs.SceneList.MAP);
    }
    #endregion

    #region Utils
    private void CreateSingletones() {
        _ = DVKeyboardManager.Instance;
        _ = DVObjectManager.Instance;
        _ = DVHelper.Instance;
    }

    private void LimitFrameRate() { 

    }
    #endregion
}
