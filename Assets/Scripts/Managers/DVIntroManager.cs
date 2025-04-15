using UnityEngine;
using System.Collections;

public class DVIntroManager : MonoBehaviour
{
    #region Unity Functions
    private void Awake()
    {
        CreateSingletones();
        
    }

    private IEnumerator Start()
    {
        LimitFrameRate();

        yield return StartCoroutine(DVResourceManager.Instance.LoadAssets((success) => 
        {
            if (!success) { 
                // TODO: 실패 팝업 후 재시도 혹은 종료
            }
        }));

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
    }

    private void LimitFrameRate() { 

    }
    #endregion
}
