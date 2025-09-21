using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace CustomTIJI.CubicLand
{
    public class Helper : SingletonMonoBehaviour<Helper>
    {
        private string _dataPath;
        private YieldCache _yieldCache;

        public static string DataPath { get { return Instance._dataPath; } }
        public static YieldCache YieldCache { get { return Instance._yieldCache; } }

        protected override void Awake()
        {
            base.Awake();

            _yieldCache = new YieldCache();
            _dataPath = Application.persistentDataPath;
        }

        public static async void WaitTimeAct(float waitTime, Action callback)
        {
            await UniTask.WaitForSeconds(waitTime);
            callback?.Invoke();
        }

        public static async void WaitFrameAct(int frame, Action callback)
        {
            for (int i = 0; i < frame; i++)
                await UniTask.NextFrame();
            callback?.Invoke();
        }
    }
}