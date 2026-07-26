namespace Commar.CubicLand
{
    public class GlobalRoot
    {
        private static GlobalRoot _instance;

        public static GlobalRoot Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GlobalRoot();
                return _instance;
            }
        }

        public YieldCache YieldCache { get; private set; }

        private GlobalRoot()
        {
            YieldCache = new YieldCache(1000);
        }
    }
}
