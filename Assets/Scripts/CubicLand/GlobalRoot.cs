namespace CustomTIJI.CubicLand
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
    }
}
