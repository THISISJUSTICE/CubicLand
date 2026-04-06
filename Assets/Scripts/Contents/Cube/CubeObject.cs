using System;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    [RequireComponent(typeof(MeshRenderer), typeof(BoxCollider))]
    public class CubeObject : MonoBehaviour
    {
        private MeshRenderer _meshRenderer;
        private ICubeTrait _cubeTrait;

        public event Action<CubeObject> onCubeDestoried;

        public CubeData CubeData { get; private set; }
        public BoxCollider Collider { get; private set; }
        public float Mass { get; private set; }

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            Collider = GetComponent<BoxCollider>();
            Collider.size = Vector3.one * (CubeConfig.CUBE_BASE_LENGHT - Physics.defaultContactOffset * 4f);
        }

        public void Initialize(CubeData cubeData, ICubeTrait cubeTrait)
        {
            _cubeTrait = cubeTrait;
            _cubeTrait.SetMeshRenderer(_meshRenderer);
            Initialize(cubeData);
        }

        public void Initialize(CubeData cubeData)
        {
            CubeData = cubeData;
            UpdateCubeObject();
        }

        public void OnDamaged(float selfMass, Vector3 impulse, CubeData collider)
        {
            if (collider != null)
                CubeData.OnDamaged(selfMass, impulse, collider);
            else
                CubeData.OnDamaged(selfMass, impulse);

            UpdateCubeObject();

            if (CubeData.StatusValue.HP <= 0)
                onCubeDestoried.Invoke(this);
        }

        public void UpdateCubeObject()
        {
            Mass = _cubeTrait.CalculateMass(CubeData.StatusValue);
            _meshRenderer.SetPropertyBlock(_cubeTrait.BuildPropertyBlock(CubeData));
        }
    }
}