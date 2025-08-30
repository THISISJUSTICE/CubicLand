using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DVYieldCache
{
    private LimitedDictionary<float, WaitForSeconds> _waitforSeconds
        = new LimitedDictionary<float, WaitForSeconds>(200);

    public readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

    public DVYieldCache() 
    {

    }

    public WaitForSeconds GetWaitForSeconds(float time) {
        if (_waitforSeconds.TryGetValue(time, out var waitForSeconds)) { 
            return waitForSeconds;
        }

        waitForSeconds = new WaitForSeconds(time);
        _waitforSeconds[time] = waitForSeconds;

        return waitForSeconds;
    }
}
