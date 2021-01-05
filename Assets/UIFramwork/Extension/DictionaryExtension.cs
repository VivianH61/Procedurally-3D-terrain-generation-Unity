using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension of Dictionary
/// </summary>
public static class DictionaryExtension
{

    /// <summary>
    /// Try to return value according to the key
    /// if the value is get then return it, otherwise return NULL
    /// this Dictionary<Tkey, Tvalue> dict. This is a dictionary we need to get the value
    /// </summary>
    public static Tvalue TryGet<Tkey, Tvalue>(this Dictionary<Tkey, Tvalue> dict, Tkey key)
    {
        Tvalue value;
        dict.TryGetValue(key, out value);
        return value;
    }

}
