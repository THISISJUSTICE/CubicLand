using UnityEngine;
using System.Collections;

public class DVIntroManager : MonoBehaviour
{

    #region Unity Functions
    private void Awake()
    {
        CreateSingletones();
        
    }

    private void Start()
    private IEnumerator Start()
    {
        LimitFrameRate();

        yield return StartCoroutine(DVResourceManager.Instance.LoadAssets((success) => 
        {
            if (!success) { 
                // TODO: ���� �˾� �� ��õ� Ȥ�� ����
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
    }

    private void LimitFrameRate() { 

    }
    #endregion
}
