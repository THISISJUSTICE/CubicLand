using UnityEngine;

namespace CustomTIJI
{
    public class YieldCache
    {
        private readonly LimitedDictionary<float, WaitForSeconds> _waitforSeconds;

        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

        public YieldCache(int count)
        {
            _waitforSeconds = new LimitedDictionary<float, WaitForSeconds>(count);
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