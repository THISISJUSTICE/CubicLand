namespace Commar.CubicLand.DataStorage
{
    public class LocalDataStorage : IDataStorage
    {
        public void Save<T>(string key, T value)
        {
            // TODO: 추후 구현
        }

        public bool TryLoad<T>(string key, out T value)
        {
            // TODO: 추후 구현
            value = default;
            return false;
        }
    }
}