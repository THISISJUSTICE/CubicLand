using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public sealed class GolemFactory : IGolemFactory
    {
        private readonly ICubeFactory _coreFactory;
        private readonly ICubeFactory _cubeFactory;

        public GolemFactory(ICubeFactory coreFactory, ICubeFactory cubeFactory)
        {
            _coreFactory = coreFactory;
            _cubeFactory = cubeFactory;
        }

        public GolemCore CreateGolem(string name, GolemData golemData, CubeSpawnOptions options)
        {
            if (golemData == null)
                throw new ArgumentNullException(nameof(golemData));

            GolemData instanceData = golemData.Clone();
            if (!instanceData.CubeDatas.TryGetValue(CubeConfig.CORE_POSITION, out CubeData coreData))
            {
                Debug.LogError($"GolemData must contain core cube data.");
                return null;
            }

            CubeObject coreCube = _coreFactory.CreateCube(coreData, options);
            GolemCore golemCore = coreCube.GetComponent<GolemCore>();
            if (golemCore == null)
            {
                _coreFactory.DestoryCube(coreCube);
                Debug.LogError("The core factory must create a cube with GolemCore.");
                return null;
            }

            coreCube.name = name;
            coreCube.transform.SetParent(null);
            coreCube.transform.position = Vector3.zero;
            coreCube.transform.rotation = Quaternion.identity;
            coreCube.transform.localScale = Vector3.one;

            List<CubeObject> cubes = new List<CubeObject>() { coreCube };
            CreateChildCubes(instanceData, coreData, coreCube, cubes, options);

            golemCore.Initialize(instanceData, cubes);
            return golemCore;
        }

        private void CreateChildCubes(GolemData golemData, CubeData parentData, CubeObject parentCube, List<CubeObject> cubes, CubeSpawnOptions options)
        {
            if (!golemData.Children.TryGetValue(parentData.ShapePoisition, out List<Vector3Int> childPositions)
                || childPositions == null || childPositions.Count == 0)
                return;

            foreach (Vector3Int childPosition in childPositions)
            {
                if (!golemData.CubeDatas.TryGetValue(childPosition, out CubeData childData)
                    || childData.IsBreaked)
                    continue;

                CubeObject childCube = _cubeFactory.CreateCube(childData, options);
                GolemCore.AttachCube(parentCube, childCube);
                cubes.Add(childCube);
                CreateChildCubes(golemData, childData, childCube, cubes, options);
            }
        }
    }
}