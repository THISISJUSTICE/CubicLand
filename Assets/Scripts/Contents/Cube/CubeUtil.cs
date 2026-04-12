using System.Collections;
using UnityEngine;

namespace CustomTIJI.CubicLand.Cube
{
    public static class CubeUtil
    {
        public static float CalculateBasicCubeObjectMass()
        {
            return CalcualteCubeObjectMass(new StatusValue(new StatusPoint()));
        }

        public static float CalcualteCubeObjectMass(StatusValue statusValue)
        {
            return CubeConfig.ONE_CUBE_MASS *
                ((float)statusValue.HP / statusValue.MaxHP * statusValue.Armor * CubeConfig.MASS_RATE + 1f);
        }
    }
}