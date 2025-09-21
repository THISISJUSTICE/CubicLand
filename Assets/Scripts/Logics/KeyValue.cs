using System;
using System.Collections.Generic;

namespace CustomTIJI
{
    [Serializable]
    public struct KeyValue<K, V>
    {
        public K key;
        public V value;

        public KeyValue(K key, V value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public static class DVKeyValueUtil
    {
        public static Dictionary<K, V> MakeDictionary<K, V>(List<KeyValue<K, V>> list)
        {
            Dictionary<K, V> dic = new Dictionary<K, V>();
            foreach (var kv in list)
            {
                dic[kv.key] = kv.value;
            }

            return dic;
        }
    }
}