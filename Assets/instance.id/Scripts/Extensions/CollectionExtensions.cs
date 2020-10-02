using System;
using System.Collections.Generic;

namespace instance.id.AAI.Extensions
{
    public static class CollectionExtensions
    {
        // ------------------------------------------------------------------- Dictionary Functions
        // -- Dictionary Functions ----------------------------------------------------------------

        public static T GetOrAdd<T, TKey>(this Dictionary<TKey, T> dictionary, TKey key, Func<TKey, T> func)
        {
            if (dictionary.TryGetValue(key, out var result))
            {
                return result;
            }

            result = func(key);
            dictionary.Add(key, result);
            return result;
        }

        public static bool TryAddValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }

        public static void TryAddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys, TValue value)
        {
            foreach (var k in keys)
            {
                dictionary.TryAddValue(k, value);
            }
        }

        // ------------------------------------------------------------------------- List Functions
        // -- List Functions ----------------------------------------------------------------------
        public static bool TryAddValue<TValue>(this List<TValue> list, TValue value)
        {
            if (list.Contains(value)) return false;
            list.Add(value);
            return true;
        }
    }
}
