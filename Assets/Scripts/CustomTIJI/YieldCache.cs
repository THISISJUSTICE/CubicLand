using UnityEngine;

namespace CustomTIJI
{
    public class YieldCache
    {
        private readonly LimitedDictionary<float, WaitForSeconds> _waitforSeconds = new LimitedDictionary<float, WaitForSeconds>(1000);
        private static YieldCache _instance;

        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

        public static YieldCache Instance
        {
            get
            { 
                if(_instance == null)
                    _instance = new YieldCache();

                return _instance;
            }
        }

        public WaitForSeconds GetWaitForSeconds(float time)
        {
            if (_waitforSeconds.TryGetValue(time, out WaitForSeconds waitForSeconds))
            {
                return waitForSeconds;
            }

            waitForSeconds = new WaitForSeconds(time);
            _waitforSeconds.Add(time, waitForSeconds);

            return waitForSeconds;
        }
    }
}