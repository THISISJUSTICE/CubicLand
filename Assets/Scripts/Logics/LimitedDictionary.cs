using System.Collections.Generic;

namespace CustomTIJI
{
    public class LimitedDictionary<TKey, TValue>
    {
        private int _maxSize;
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly LinkedList<TKey> _orderList;

        public LimitedDictionary(int maxSize = 100)
        {
            if (maxSize <= 0)
                maxSize = 1;
            _maxSize = maxSize;
            _dictionary = new Dictionary<TKey, TValue>();
            _orderList = new LinkedList<TKey>();
        }

        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out var value))
                    return value;
                Debug.LogError($"Key Error");
                return default(TValue);
            }
            set => Add(key, value);
        }

        public int Count => _dictionary.Count;

        public void Add(TKey key, TValue value)
        {
            if (_dictionary.ContainsKey(key))
            {
                _dictionary[key] = value;
                MoveToMostRecent(key);
            }
            else
            {
                _dictionary[key] = value;
                _orderList.AddLast(key);

                if (_dictionary.Count > _maxSize)
                {
                    RemoveOldest();
                }
            }
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public bool ContainsValue(TValue value) => _dictionary.ContainsValue(value);

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out value))
            {
                MoveToMostRecent(key);
                return true;
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.ContainsKey(key))
            {
                return false;
            }

            _dictionary.Remove(key);
            _orderList.Remove(key);
            return true;
        }

        public void Clear()
        {
            _dictionary.Clear();
            _orderList.Clear();
        }

        private void MoveToMostRecent(TKey key)
        {
            _orderList.Remove(key);
            _orderList.AddLast(key);
        }

        private void RemoveOldest()
        {
            var oldestKey = _orderList.First.Value;
            _orderList.RemoveFirst();
            _dictionary.Remove(oldestKey);
        }

    }
}