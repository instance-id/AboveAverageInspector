// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/AboveAverageInspector         --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

        // ------------------------------------------------------------------------- List Functions
        // -- List Functions ----------------------------------------------------------------------
        public static bool TryAddCategory<TValue>(this List<TValue> list, TValue value)
        {
            Debug.Log($"TValue: {typeof(TValue).ToString()} value: {value.GetType().ToString()} ");

            if (value.GetType() == typeof(VisualElement))
                if (list.Exists(x => x.Cast<VisualElement>().name == value.Cast<VisualElement>().name))
                    return false;
                else
                {
                    list.Add(value);
                    return true;
                }

            else if (value.GetType() == typeof(Foldout))
                if (list.Exists(x => x.Cast<Foldout>().name == value.Cast<Foldout>().name))
                    return false;
                else
                {
                    list.Add(value);
                    return true;
                }

            return false;
        }
    }
}
