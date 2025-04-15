using UnityEngine;
using System.Collections.Generic;

public class DVEffectManager : SingletonMonoBehaviour<DVEffectManager>
{
    #region Variables
    private Dictionary<string, UnityEngine.Object> _effects = null;
    #endregion

    #region Utils
    public void MakeEffect(string effectName, Vector3 position) {
        if (_effects == null)
        {
            if (!DVResourceManager.Instance.TryGetAssetDictionary(DVAssets.AssetType.Effect, out _effects))
                return;
        }

        if (!_effects.ContainsKey(effectName)) {
            Debug.Log($"'{effectName}' is invalid name");
            return;
        }

        GameObject effect = (GameObject)_effects[effectName];
        var instance = DVObjectManager.Instance.InstanitateObject(effect);
        instance.transform.position = position;
        var particle = instance.GetComponent<ParticleSystem>();
        StartCoroutine(DVHelper.In.WaitTimeActCor(particle.main.startLifetime.constantMax, () => DVObjectManager.Instance.DestroyObject(instance)));
    }
    #endregion
}
