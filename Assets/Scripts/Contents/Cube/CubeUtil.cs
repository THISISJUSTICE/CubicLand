namespace CustomTIJI.CubicLand.Cube
{
    public static class CubeUtil
    {
        public static float CalculateBasicCubeObjectMass()
        {
            StatusPoint statusPoint = new StatusPoint();
            statusPoint.Initialize();
            StatusValue statusValue = new StatusValue();
            statusValue.Initialize(statusPoint);

            return CalcualteCubeObjectMass(statusValue);
        }

        public static float CalcualteCubeObjectMass(StatusValue statusValue)
        {
            return CubeConfig.ONE_CUBE_MASS *
                ((float)statusValue.HP / statusValue.MaxHP * statusValue.Armor * CubeConfig.MASS_RATE + 1f);
        }
    }
}