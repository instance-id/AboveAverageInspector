using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace instance.id.AAI.Extensions
{
    public static class ArrayExtensions
    {
        public static int IndexOf<T>(this Array array, T value)
        {
            return Array.IndexOf(array, value);
        }

        public static int FindIndex<T>(this Array array, Predicate<T> match)
        {
            int index = -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (match((T) array.GetValue(i)))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public static T GetValue<T>(this Array array, int index)
        {
            return (T) array.GetValue(index);
        }

        public static void Clear<T>(this T[] array)
        {
            Array.Clear(array, 0, array.Length);
        }

        public static T Pop<T>(this T[] array, int index, out T[] remaining)
        {
            var list = new List<T>(array);

            T item = list.Pop(index);
            remaining = list.ToArray();

            return item;
        }

        public static T Pop<T>(this T[] array, T element, out T[] remaining)
        {
            return array.Pop(Array.IndexOf(array, element), out remaining);
        }

        public static T Pop<T>(this T[] array, out T[] remaining)
        {
            return array.Pop(0, out remaining);
        }

        public static T PopRandom<T>(this T[] array, out T[] remaining)
        {
            return array.Pop(Random.Range(0, array.Length - 1), out remaining);
        }

        public static T[] PopRange<T>(this T[] array, int startIndex, int count, out T[] remaining)
        {
            List<T> list = new List<T>(array);
            T[] popped = list.PopRange(startIndex, count).ToArray();
            remaining = list.ToArray();

            return popped;
        }

        public static T[] PopRange<T>(this T[] array, int count, out T[] remaining)
        {
            return array.PopRange(0, count, out remaining);
        }

        public static T[] Joined<T>(this T[] array, IList<T> other)
        {
            var joined = new T[array.Length + other.Count];

            array.CopyTo(joined, 0);
            other.CopyTo(joined, array.Length);

            return joined;
        }

        public static T[] Joined<T>(this T[] array, params T[] other)
        {
            if (array == null)
                return other;
            else if (other == null) return array;

            var joined = new T[array.Length + other.Length];

            array.CopyTo(joined, 0);
            other.CopyTo(joined, array.Length);

            return joined;
        }

        public static T[] Slice<T>(this T[] array, int startIndex)
        {
            return array.Slice(startIndex, array.Length - startIndex);
        }

        public static T[] Slice<T>(this T[] array, int startIndex, int count)
        {
            T[] slicedArray = new T[count];

            for (int i = 0; i < count; i++) slicedArray[i] = array[i + startIndex];

            return slicedArray;
        }

        public static U[] Convert<T, U>(this Array array, Func<T, U> conversion)
        {
            return array.Convert(conversion, 0, array.Length);
        }

        public static U[] Convert<T, U>(this Array array, Func<T, U> conversion, int startIndex, int count)
        {
            var converted = new U[array.Length];

            for (int i = startIndex; i < Mathf.Min(startIndex + count, array.Length); i++) converted[i] = conversion((T) array.GetValue(i));

            return converted;
        }

        public static U[] Convert<T, U>(this T[] array, Func<T, U> conversion)
        {
            return array.Convert(conversion, 0, array.Length);
        }

        public static U[] Convert<T, U>(this T[] array, Func<T, U> conversion, int startIndex, int count)
        {
            var converted = new U[array.Length];

            for (int i = startIndex; i < Mathf.Min(startIndex + count, array.Length); i++) converted[i] = conversion(array[i]);

            return converted;
        }

        public static U[] Convert<T, U>(this IList<T> array, Func<T, U> conversion)
        {
            return array.Convert(conversion, 0, array.Count);
        }

        public static U[] Convert<T, U>(this IList<T> array, Func<T, U> conversion, int startIndex, int count)
        {
            var converted = new U[array.Count];

            for (int i = startIndex; i < Mathf.Min(startIndex + count, array.Count); i++) converted[i] = conversion(array[i]);

            return converted;
        }

        public static T[] Reversed<T>(this T[] array)
        {
            T[] reversedArray = new T[array.Length];

            for (int i = 0; i < array.Length; i++) reversedArray[i] = array[array.Length - i - 1];

            return reversedArray;
        }

        public static void Reverse<T>(this IList<T> array)
        {
            for (int i = 0; i < array.Count / 2; i++) array.Switch(i, array.Count - i - 1);
        }

        public static T First<T>(this IList<T> array)
        {
            return array != null && array.Count > 0 ? array[0] : default(T);
        }

        public static T Last<T>(this IList<T> array)
        {
            return array != null && array.Count > 0 ? array[array.Count - 1] : default(T);
        }

        public static void Fill<T>(this IList<T> array, Func<int, T> getValue)
        {
            array.Fill(getValue, 0, array.Count);
        }

        public static void Fill<T>(this IList<T> array, Func<int, T> getValue, int startIndex, int count)
        {
            for (int i = startIndex; i < Mathf.Min(startIndex + count, array.Count); i++) array[i] = getValue(i);
        }

        public static void Fill<T>(this IList<T> array, T value)
        {
            array.Fill(value, 0, array.Count);
        }

        public static void Fill<T>(this IList<T> array, T value, int startIndex, int count)
        {
            for (int i = startIndex; i < Mathf.Min(startIndex + count, array.Count); i++) array[i] = value;
        }

        public static Type[] GetTypes(this IList array)
        {
            var types = new Type[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                var element = array[i];
                types[i] = element == null ? null : element.GetType();
            }

            return types;
        }

        public static string[] GetTypeNames(this IList array)
        {
            var typeNames = new string[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                var element = array[i];
                typeNames[i] = element == null ? "" : element.GetType().Name;
            }

            return typeNames;
        }

        public static T GetRandom<T>(this IList<T> array)
        {
            if (array == null || array.Count == 0) return default(T);

            return array[Random.Range(0, array.Count - 1)];
        }

        public static void Switch<T>(this IList<T> array, int sourceIndex, int targetIndex)
        {
            var temp = array[sourceIndex];
            array[sourceIndex] = array[targetIndex];
            array[targetIndex] = temp;
        }

        public static void Switch<T>(this IList<T> array, T source, T target)
        {
            if (array.Contains(source) && array.Contains(target)) array.Switch(array.IndexOf(source), array.IndexOf(target));
        }

        public static int FindSmallestIndex<T>(this IList<T> array) where T : IComparable<T>
        {
            if (array.Count == 0) return -1;

            int index = 0;

            for (int i = 1; i < array.Count; i++)
            {
                if (array[i].CompareTo(array[index]) < 0) index = i;
            }

            return index;
        }

        public static int FindSmallestIndex<T>(this IList<T> array, Comparison<T> comparison)
        {
            if (array.Count == 0) return -1;

            int index = 0;

            for (int i = 1; i < array.Count; i++)
            {
                if (comparison(array[i], array[index]) < 0) index = i;
            }

            return index;
        }

        public static int FindBiggestIndex<T>(this IList<T> array) where T : IComparable<T>
        {
            if (array.Count == 0) return -1;

            int index = 0;

            for (int i = 1; i < array.Count; i++)
            {
                if (array[i].CompareTo(array[index]) > 0) index = i;
            }

            return index;
        }

        public static int FindBiggestIndex<T>(this IList<T> array, Comparison<T> comparison)
        {
            if (array.Count == 0) return -1;

            int index = 0;

            for (int i = 1; i < array.Count; i++)
            {
                if (comparison(array[i], array[index]) > 0) index = i;
            }

            return index;
        }

        public static T FindSmallest<T>(this IList<T> array) where T : IComparable<T>
        {
            int index = array.FindSmallestIndex();

            return index < 0 ? default(T) : array[index];
        }

        public static T FindSmallest<T>(this IList<T> array, Comparison<T> comparison)
        {
            int index = array.FindSmallestIndex(comparison);

            return index < 0 ? default(T) : array[index];
        }

        public static T FindBiggest<T>(this IList<T> array) where T : IComparable<T>
        {
            int index = array.FindBiggestIndex();

            return index < 0 ? default(T) : array[index];
        }

        public static T FindBiggest<T>(this IList<T> array, Comparison<T> comparison)
        {
            int index = array.FindBiggestIndex(comparison);

            return index < 0 ? default(T) : array[index];
        }

        public static bool ContentEquals<T>(this IList<T> array, IList<T> other, Func<T, T, bool> comparison)
        {
            if (array == other)
                return true;
            else if (array == null || other == null)
                return false;
            else if (array.Count != other.Count) return false;

            for (int i = 0; i < array.Count; i++)
            {
                if (!comparison(array[i], other[i])) return false;
            }

            return true;
        }

        public static string[] ToStringArray(this IList array)
        {
            var stringArray = new string[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                var element = array[i];

                if (element is ValueType || element != null) stringArray[i] = array[i].ToString();
            }

            return stringArray;
        }

        public static int Count<T>(this IList<T> array, Predicate<T> match)
        {
            int count = 0;

            for (int i = 0; i < array.Count; i++)
            {
                if (match(array[i])) count++;
            }

            return count;
        }
    }
}
