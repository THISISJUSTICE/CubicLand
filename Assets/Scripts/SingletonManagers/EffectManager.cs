using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace CustomTIJI.CubicLand
{
    public class EffectManager : SingletonMonoBehaviour<EffectManager>, IIntroInitializer
    {
        private readonly Dictionary<string, GameObject> _effects = new Dictionary<string, GameObject>();

        private bool _isLoaded = false;
        public bool IsLoaded => _isLoaded;

        protected override void Awake()
        {
            base.Awake();
        }

        public async UniTask Initialize()
        {
            string[] keys = new string[]
            {
            "CubeDestroyEffect",
            };

            foreach (string key in keys)
            {
                GameObject effect = await AddresableManager.Instance.LoadAsset<GameObject>(key);
                _effects.Add(key, effect);
            }

            _isLoaded = true;
        }

        public GameObject MakeEffect(string effectName, Vector3 position)
        {
            if (!_effects.ContainsKey(effectName))
            {
                Debug.Log($"'{effectName}' is invalid name");
                return null;
            }

            GameObject effect = (GameObject)_effects[effectName];
            GameObjectInstance instance = ObjectManager.Instance.InstanitateGameObject(effect, useInstanceMaterial: true);
            instance.Position = position;
            var particle = instance.GetComponent<ParticleSystem>();
            Helper.WaitTimeAct(particle.main.startLifetime.constantMax,
                () => instance.Destroy());

            return effect;
        }

        public void MakeCubeDestroyEffect(Vector3 position, Color color)
        {
            GameObject effect = MakeEffect("CubeDestroyEffect", position);
            effect.GetComponent<ParticleSystemRenderer>().sharedMaterial.color = color;
        }
    }
}