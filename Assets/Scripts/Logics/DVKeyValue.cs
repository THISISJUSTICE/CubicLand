using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct DVKeyValue<K, V>
{
    public K key;
    public V value;

    public DVKeyValue(K key, V value) { 
        this.key = key;
        this.value = value;
    }
}

public static class DVKeyValueUtil 
{
    public static Dictionary<K, V> MakeDictionary<K, V>(List<DVKeyValue<K, V>> list) {
        Dictionary<K, V> dic = new Dictionary<K, V>();
        foreach (var kv in list)
        {
            dic[kv.key] = kv.value;
        }

        return dic;
    }
}
