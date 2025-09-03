using UnityEngine;
using System.Collections;

public class DVIntroManager : MonoBehaviour
{
    #region Unity Functions
    private void Awake()
    {
        CreateSingletones();

        DVConfigs.Setup();
    }

    private async void Start()
    {
        LimitFrameRate();

        await DVResourceManager.Instance.LoadAssets();

        var monoBehaviours = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var monoBehaviour in monoBehaviours) {
            if (monoBehaviour is IIntroInitializable init)
                init.OnIntroInit();
        }

        DVSceneConfigs.LoadScene(DVSceneConfigs.SceneList.MAP);

        Physics.defaultContactOffset = 0.0001f;
    }
    #endregion

    #region Utils
    private void CreateSingletones() {
        _ = DVAddresableManager.Instance;
        _ = DVKeyboardManager.Instance;
        _ = DVObjectManager.Instance;
        _ = DVHelper.Instance;
        _ = DVDataManager.Instance;
        _ = DVResourceManager.Instance;
        _ = DVEffectManager.Instance;
        _ = DVCubeCreator.Instance;
    }

    private void LimitFrameRate() { 

    }
    #endregion
}
