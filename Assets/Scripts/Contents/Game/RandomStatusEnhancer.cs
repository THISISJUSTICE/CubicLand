using Commar.CubicLand.Cube;
using UnityEngine;

namespace Commar.CubicLand.Game
{
    public class RandomStatusEnhancer
    {
        private const int STATUS_POINT_WEIGHT = 3;
        private const int CUBE_POINT_WEIGHT = 1;

        public void EnhanceRandomStatus(GolemData golemData, int point)
        {
            point = Mathf.Max(point, 0);

            int statusPoint = 0;
            int cubePoint = 0;

            for (int i = 0; i < point; i++)
            {
                if (Random.Range(0, STATUS_POINT_WEIGHT + CUBE_POINT_WEIGHT) < STATUS_POINT_WEIGHT)
                    statusPoint++;
                else
                    cubePoint++;
            }

            EnhanceStatusPoint(golemData, statusPoint);
            AddCube(golemData, golemData.structureData, cubePoint);
        }

        public void EnhanceRandomStatus(CubeData cubeData, int point)
        {
            // TODO: 구현
            throw new System.NotImplementedException();
        }

        public void ApplyRandomDamage(GolemData golemData, int totalDamage)
        {
            // TODO: 구현
            throw new System.NotImplementedException();
        }

        public void ApplyRandomDamage(CubeData cubeData, int totalDamage)
        {
            // TODO: 구현
            throw new System.NotImplementedException();
        }

        private void EnhanceStatusPoint(IGolemStatusPoint golemStatusPoint, int point)
        {
            if (golemStatusPoint == null || point <= 0)
                return;

            int hpPoint = 0;
            int armorPoint = 0;
            int attackPoint = 0;
            int moveSpeedPoint = 0;

            for (int i = 0; i < point; i++)
            {
                switch (Random.Range(0, 4))
                {
                    case 0:
                        hpPoint++;
                        break;

                    case 1:
                        armorPoint++;
                        break;

                    case 2:
                        attackPoint++;
                        break;

                    case 3:
                        moveSpeedPoint++;
                        break;
                }
            }

            golemStatusPoint.EnhanceStatusPoint(new StatusPoint(hpPoint, armorPoint, attackPoint), moveSpeedPoint);
        }

        private void AddCube(IGolemStatusPoint golemStatusPoint, GolemStructureData structureData, int addCount)
        {
            if (golemStatusPoint == null || structureData == null || addCount <= 0)
                return;

            for (int i = 0; i < addCount; i++)
            {
                int candidateCount = 0;

                foreach (Vector3Int parentPosition in structureData.CubeDatas.Keys)
                {
                    for (int directionIndex = 0; directionIndex < DirectionUtils.Direction3DLength; directionIndex++)
                    {
                        Direction3D direction = (Direction3D)directionIndex;
                        Vector3Int position = parentPosition + DirectionUtils.GetDirection3DValue(direction);

                        if (!structureData.CubeDatas.ContainsKey(position))
                            candidateCount++;
                    }
                }

                if (candidateCount == 0)
                    return;

                int selectedIndex = Random.Range(0, candidateCount);
                Vector3Int selectedParentPosition = default;
                Direction3D selectedDirection = default;
                bool isSelected = false;

                foreach (Vector3Int parentPosition in structureData.CubeDatas.Keys)
                {
                    for (int directionIndex = 0; directionIndex < DirectionUtils.Direction3DLength; directionIndex++)
                    {
                        Direction3D direction = (Direction3D)directionIndex;
                        Vector3Int position = parentPosition + DirectionUtils.GetDirection3DValue(direction);

                        if (structureData.CubeDatas.ContainsKey(position))
                            continue;

                        if (selectedIndex > 0)
                        {
                            selectedIndex--;
                            continue;
                        }

                        selectedParentPosition = parentPosition;
                        selectedDirection = direction;
                        isSelected = true;
                        break;
                    }

                    if (isSelected)
                        break;
                }

                if (!isSelected || !golemStatusPoint.AddCube(selectedParentPosition, selectedDirection))
                    return;
            }
        }
    }
}