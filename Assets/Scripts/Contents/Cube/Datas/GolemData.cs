namespace Commar.CubicLand.Cube
{
    public class GolemData
    {
        /*TODO: 저장 대상*/
        public readonly GolemStructureData structureData;

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
    }
}