using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand
{
    public class IntroManager : MonoBehaviour
    {
        private void Awake()
        {
            Configs.Setup();
        }

        private async void Start()
        {
            LimitFrameRate();

            await Initialize();

            Physics.defaultContactOffset = 0.0001f;

            SceneConfigs.LoadScene(SceneConfigs.SceneList.Map);
        }

        private async UniTask Initialize()
        {
            MonoBehaviour[] scripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            List<UniTask> initializers = new List<UniTask>();

            for (int i = 0; i < scripts.Length; i++)
            {
                if (scripts[i] is IIntroInitializer initializer)
                    initializers.Add(initializer.Initialize());
            }

            await UniTask.WhenAll(initializers);
        }

        private void LimitFrameRate()
        {

        }
    }

    internal interface IIntroInitializer
    {
        public abstract UniTask Initialize();
    }
}