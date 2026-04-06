using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    [RequireComponent(typeof(CubeObject), typeof(Rigidbody))]
    public class GolemCore : MonoBehaviour
    {
        private ICubeCollisionResolver _cubeCollisionResolver;
        private IOrphanedCubeHandler _orphanedCubeHandler;

        private readonly Dictionary<Vector3Int, CubeObject> _cubes = new Dictionary<Vector3Int, CubeObject>();

        internal Rigidbody Rigidbody { get; private set; }
        public GolemData GolemData { get; private set; }
        public Vector3 MoveVelocity { get; internal set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            _cubes[CubeConfig.CORE_POSITION] = GetComponent<CubeObject>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
                return;

            _cubeCollisionResolver.OnCollision(gameObject, collision);
            UpdateGolemMass();
        }

        public void Initialize(GolemData golemData, IList<CubeObject> cubes, ICubeCollisionResolver cubeCollisionResolver, IOrphanedCubeHandler orphanedCubeHandler)
        {
            _cubeCollisionResolver = cubeCollisionResolver;
            _orphanedCubeHandler = orphanedCubeHandler;
            Initialize(golemData, cubes);
        }

        public void Initialize(GolemData golemData, IList<CubeObject> cubes)
        {
            GolemData = golemData;

            _cubes.Clear();
            foreach (CubeObject cube in cubes)
            {
                _cubes[cube.CubeData.ShapePoisition] = cube;
                cube.Initialize(golemData.CubeDatas[cube.CubeData.ShapePoisition]);
                cube.onCubeDestoried += OnParentCubeDestoried;
            }

            UpdateGolemMass();
        }

        public CubeObject FindCube(Vector3Int position)
        {
            if (_cubes.TryGetValue(position, out CubeObject cube))
                return cube;
            return null;
        }

        public float CalculateMoveTime(float initTime, float minTime, float maxTime)
        {
            float weightFactor = Mathf.Pow(Rigidbody.mass, CubeConfig.WEIGHT_EXPONENT) / Mathf.Pow(CubeUtil.CalculateBasicCubeObjectMass(), CubeConfig.WEIGHT_EXPONENT);
            float speedFactor = Mathf.Pow((float)CubeConfig.Status.INIT_MOVE_SPEED / GolemData.MoveSpeed, CubeConfig.SPEED_EXPONENT);
            float moveTime = initTime * weightFactor * speedFactor;

            return Mathf.Clamp(moveTime, minTime, maxTime);
        }

        public void SetAttackMode(bool attackMode)
        {
            foreach (CubeObject cube in _cubes.Values)
                cube.CubeData.IsAttackMode = attackMode;
        }

        private void OnParentCubeDestoried(CubeObject cube)
        {
            _cubes.Remove(cube.CubeData.ShapePoisition);
            cube.onCubeDestoried -= OnParentCubeDestoried;

            List<CubeData> children = GolemData.FindChildren(cube.CubeData.ShapePoisition);
            foreach (CubeData child in children)
            {
                if (!_cubes.ContainsKey(child.ShapePoisition))
                    continue;

                _orphanedCubeHandler.HandleOrphanedCube(_cubes[child.ShapePoisition]);
                _cubes[child.ShapePoisition].onCubeDestoried -= OnParentCubeDestoried;
                _cubes.Remove(child.ShapePoisition);
            }

            UpdateGolemMass();
        }

        private void UpdateGolemMass()
        {
            float mass = 0f;
            foreach (CubeObject cube in _cubes.Values)
            {
                mass += cube.Mass;
            }
            Rigidbody.mass = mass;
        }
    }
}