using Cysharp.Threading.Tasks;
using UnityEngine;

public class DVIntroManager : MonoBehaviour
{
    private void Awake()
    {
        DVConfigs.Setup();
    }

    private async void Start()
    {
        LimitFrameRate();

        await WaitIntroLoad();

        DVSceneConfigs.LoadScene(DVSceneConfigs.SceneList.Map);

        Physics.defaultContactOffset = 0.0001f;
    }

    private async UniTask WaitIntroLoad()
    {
        MonoBehaviour[] scripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        for (int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i] is IIntroLoadChecker introLoad)
                await UniTask.WaitUntil(() => introLoad.IsLoaded);
        }
    }

    private void LimitFrameRate()
    {

    }
}

public interface IIntroLoadChecker
{
    public abstract bool IsLoaded { get; }
}