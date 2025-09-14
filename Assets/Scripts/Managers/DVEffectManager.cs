using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class DVEffectManager : SingletonMonoBehaviour<DVEffectManager>
{
    private Dictionary<string, UnityEngine.Object> _effects;

    protected override async void Awake()
    {
        base.Awake();

        await UniTask.WaitUntil(() => DVResourceManager.Instance.IsLoaded);
        DVResourceManager.Instance.TryGetAssetDictionary("Effects", out _effects);
    }

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
        DVHelper.WaitTimeAct(particle.main.startLifetime.constantMax,
            () => DVObjectManager.Instance.DestroyObject(instance));

        return effect;
    }

    public void MakeCubeDestroyEffect(Vector3 position, Color color) {
        GameObject effect = MakeEffect("CubeDestroyEffect", position);
        effect.GetComponent<ParticleSystemRenderer>().sharedMaterial.color = color;
    }
}
