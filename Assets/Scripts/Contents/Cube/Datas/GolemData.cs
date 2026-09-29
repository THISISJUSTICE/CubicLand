using System;
using System.Collections.Generic;
using UnityEngine;

namespace Commar.CubicLand.Cube
{
    public class GolemData
    {
        /*TODO: 저장 대상*/
        public readonly GolemStructureData structureData;

        private readonly List<GolemCubeData> _tempCubeDatas = new List<GolemCubeData>();

        public event Action onStatusChanged;
        public event Action<GolemCubeData> onCubeCreated;

        /*TODO: 저장 대상*/
        public int MoveSpeedPoint { get; private set; } // TODO: Enhance 추가
        public int MoveSpeed => CubeConfig.Status.INIT_MOVE_SPEED + MoveSpeedPoint * CubeConfig.Status.ADD_MOVE_SPEED;
        public StatusPoint StatusPoint
        {
            get
            {
                if (structureData == null || structureData.CubeDatas == null
                    || !structureData.CubeDatas.TryGetValue(CubeConfig.CORE_POSITION, out GolemCubeData cubeData))
                    return new StatusPoint(0, 0, 0);

                return cubeData.StatusPoint;
            }
        }
        public int CubeCount
        {
            get
            {
                if (structureData == null || structureData.CubeDatas == null)
                    return 0;

                return structureData.CubeDatas.Count;
            }
        }

        public GolemData(int moveSpeedPoint = 0, GolemStructureData structureData = null)
        {
            MoveSpeedPoint = moveSpeedPoint;

            if (structureData != null)
                this.structureData = structureData;
            else
                this.structureData = new GolemStructureData();
        }

        public GolemData Clone()
        {
            return new GolemData(MoveSpeedPoint, structureData.Clone());
        }

        internal void EnhanceStatus(StatusPoint statusPoint)
        {
            if (!structureData.CubeDatas.TryGetValue(CubeConfig.CORE_POSITION, out GolemCubeData cubeData))
                return;

            cubeData.EnhanceStatus(statusPoint);
            EnhanceChildStatus(cubeData);

            onStatusChanged?.Invoke();
        }

        internal void Heal(int heal)
        {
            if (!structureData.CubeDatas.TryGetValue(CubeConfig.CORE_POSITION, out GolemCubeData cubeData))
                return;

            heal = ApplyHeal(heal, cubeData);
            DistributeHeal(heal, new GolemCubeData[] { cubeData });

            onStatusChanged?.Invoke();
        }

        internal bool AddCube(Vector3Int parentPosition, Enums.Direction3D direction)
        {
            if (!structureData.TryAddCube(parentPosition, direction, out Vector3Int position)
                || !structureData.CubeDatas.TryGetValue(position, out GolemCubeData cubeData))
                return false;

            if (structureData.CubeDatas.TryGetValue(parentPosition, out GolemCubeData parent)
                && parent.IsBreaked)
            {
                cubeData.IsBreaked = true;
                cubeData.StatusValue.ApplyDamage(1);

                GolemCubeData creatableCube = structureData.FindCreatableCube(position);

                if (creatableCube != null && creatableCube.shapePoisition != CubeConfig.CORE_POSITION
                    && structureData.ChildDepths.TryGetValue(creatableCube.shapePoisition, out int parentDepth)
                    && structureData.ChildDepths.TryGetValue(position, out int childDepth))
                {
                    int heal = Mathf.RoundToInt((float)cubeData.StatusValue.MaxHP / (childDepth - parentDepth + 1));
                    creatableCube.StatusValue.Heal(Mathf.Max(1, heal));
                    creatableCube.IsBreaked = false;
                    onCubeCreated?.Invoke(creatableCube);
                }
            }
            else
                onCubeCreated?.Invoke(cubeData);

            return true;
        }

        internal void SetAttackMode(bool isAttackMode)
        {
            foreach (GolemCubeData cubeData in structureData.CubeDatas.Values)
                cubeData.IsAttackMode = isAttackMode;
        }

        private int ApplyHeal(int heal, GolemCubeData cubeData)
        {
            StatusValue statusValue = cubeData.StatusValue;
            if (!statusValue.IsFullHP() && heal > 0)
            {
                heal = statusValue.Heal(heal);

                if (heal <= 0)
                    return 0;
            }

            return heal;
        }

        private void DistributeHeal(int heal, IList<GolemCubeData> cubeDatas)
        {
            if (heal <= 0 || cubeDatas == null || cubeDatas.Count == 0)
                return;

            List<GolemCubeData> children = new List<GolemCubeData>();

            foreach (GolemCubeData cubeData in cubeDatas)
                FindChildren(cubeData.shapePoisition, children);

            if (children.Count == 0)
                return;

            List<GolemCubeData> targets = _tempCubeDatas;
            targets.Clear();
            targets.AddRange(children);

            while (heal > 0 && targets.Count > 0)
            {
                int baseShare = heal / targets.Count;
                int remainder = heal % targets.Count;

                for (int i = 0; i < targets.Count && heal > 0; i++)
                {
                    int offer = 1;
                    if (baseShare > 0)
                        offer = baseShare + (i < remainder ? 1 : 0);
                    heal += ApplyHeal(offer, targets[i]) - offer;
                }

                for (int i = targets.Count - 1; i >= 0; i--)
                {
                    if (targets[i].StatusValue.IsFullHP())
                    {
                        if (targets[i].IsBreaked)
                        {
                            targets[i].IsBreaked = false;
                            onCubeCreated?.Invoke(targets[i]);
                        }
                        targets.RemoveAt(i);
                    }
                }
            }

            DistributeHeal(heal, children);
        }

        private void EnhanceChildStatus(GolemCubeData parent)
        {
            if (!structureData.Children.TryGetValue(parent.shapePoisition, out List<Vector3Int> childList))
                return;

            foreach (Vector3Int position in childList)
            {
                if (!structureData.CubeDatas.TryGetValue(position, out GolemCubeData cubeData))
                    continue;

                cubeData.EnhanceChildStatus(parent);
                EnhanceChildStatus(cubeData);
            }
        }

        private void FindChildren(Vector3Int position, IList<GolemCubeData> cubeDatas)
        {
            if (cubeDatas == null || !structureData.Children.TryGetValue(position, out List<Vector3Int> children) || children.Count == 0)
                return;

            foreach (Vector3Int childPosition in children)
            {
                if (structureData.CubeDatas.TryGetValue(childPosition, out GolemCubeData cubeData))
                    cubeDatas.Add(cubeData);
            }
        }
    }
}