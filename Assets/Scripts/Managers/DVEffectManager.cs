using UnityEngine;
using System.Collections.Generic;

public class DVEffectManager : SingletonMonoBehaviour<DVEffectManager>, IIntroInitializable
{
    #region Variables
    private Dictionary<string, UnityEngine.Object> _effects;
    #endregion

    #region Override
    public void OnIntroInit()
    {
        DVResourceManager.Instance.TryGetAssetDictionary(DVAssets.AssetType.Effect, out _effects);
    }
    #endregion

    #region Utils
    public GameObject MakeEffect(string effectName, Vector3 position) {
        if (!_effects.ContainsKey(effectName)) {
            Debug.Log($"'{effectName}' is invalid name");
            return null;
        }

        GameObject effect = (GameObject)_effects[effectName];
        var instance = DVObjectManager.Instance.InstanitateObject(effect, instMat:true);
        instance.transform.position = position;
        instance.transform.SetParent(transform);
        var particle = instance.GetComponent<ParticleSystem>();
        StartCoroutine(DVHelper.In.WaitTimeActCor(particle.main.startLifetime.constantMax, () => DVObjectManager.Instance.DestroyObject(instance)));

        return effect;
    }

    public void MakeCubeDestroyEffect(Vector3 position, Color color) {
        GameObject effect = MakeEffect("CubeDestroyEffect", transform.position);
        effect.GetComponent<ParticleSystemRenderer>().sharedMaterial.color = color;
    }
    #endregion
}
