using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Commar.CubicLand.Compositions
{
    public class IntroManager : MonoBehaviour
    {
        [SerializeField] private SceneList.Scene _nextScene;

        private OperationWaiter _operationHandleHandler;

        [Inject]
        public void Initialize(OperationWaiter operationHandleHandler)
        {
            _operationHandleHandler = operationHandleHandler;
        }

        private void Start()
        {
            _operationHandleHandler.OnCompleted += OnOperationHandlesCompleted;
            _operationHandleHandler.WaitOperationHandlesAsync().Forget();
        }

        private void OnDestroy()
        {
            _operationHandleHandler.OnCompleted -= OnOperationHandlesCompleted;
        }

        private async void OnOperationHandlesCompleted(OperationResult result)
        {
            if (result.IsSuccess)
            {
                Scene currentScene = gameObject.scene;
                string nextSceneName = SceneList.GetSceneName(_nextScene);

                await SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextSceneName));
                _ = SceneManager.UnloadSceneAsync(currentScene);
            }
            else
            {
                Debug.LogError(result.ErrorMessage);
                Utils.QuitApplication();
            }
        }
    }
}