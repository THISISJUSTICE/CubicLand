namespace Commar.CubicLand.DataStorage
{
    public interface IDataStorage
    {
        void Save<T>(string key, T value);
        bool TryLoad<T>(string key, out T value);
    }
}