using UnityEngine;

namespace CustomTIJI
{
    public class YieldCache
    {
        private LimitedDictionary<float, WaitForSeconds> _waitforSeconds
            = new LimitedDictionary<float, WaitForSeconds>(200);

        public readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

        public YieldCache()
        {

        }

        public WaitForSeconds GetWaitForSeconds(float time)
        {
            if (_waitforSeconds.TryGetValue(time, out var waitForSeconds))
            {
                return waitForSeconds;
            }

            waitForSeconds = new WaitForSeconds(time);
            _waitforSeconds[time] = waitForSeconds;

            return waitForSeconds;
        }
    }
}