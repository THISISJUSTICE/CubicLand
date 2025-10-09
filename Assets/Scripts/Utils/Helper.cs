namespace CustomTIJI.CubicLand
{
    public class Helper : SingletonMonoBehaviour<Helper>
    {
        private YieldCache _yieldCache;
        public static YieldCache YieldCache { get { return Instance._yieldCache; } }

        protected override void Awake()
        {
            base.Awake();

            _yieldCache = new YieldCache(200);
        }
    }
}