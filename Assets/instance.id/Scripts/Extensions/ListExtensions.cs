using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace instance.id.AAI.Extensions
{
	public static class ListExtensions
	{
		public static void Resize<T>(this List<T> list, int length)
		{
			int count = list.Count;

			if (count > length)
				list.RemoveRange(length, count - length);
			else if (count < length)
			{
				list.Capacity = length;
				list.AddRange(Enumerable.Repeat(default(T), length - count));
			}
		}

		public static bool RemoveRange<T>(this List<T> list, T[] elements)
		{
			bool success = true;

			for (int i = 0; i < elements.Length; i++)
				success &= list.Remove(elements[i]);

			return success;
		}

		public static T Pop<T>(this List<T> list, int index = 0)
		{
			T item = list[index];
			list.RemoveAt(index);

			return item;
		}

		public static T Pop<T>(this List<T> list, T element)
		{
			return list.Pop(list.IndexOf(element));
		}

		public static T PopLast<T>(this List<T> list)
		{
			return list.Pop(list.Count - 1);
		}

		public static T PopRandom<T>(this List<T> list)
		{
			return list.Pop(Random.Range(0, list.Count));
		}

		public static List<T> PopRange<T>(this List<T> list, int startIndex, int count)
		{
			var popped = new List<T>(count);

			for (int i = 0; i < count; i++)
				popped[i] = list.Pop(i + startIndex);

			return popped;
		}

		public static List<T> PopRange<T>(this List<T> list, int startIndex)
		{
			return list.PopRange(startIndex, list.Count - startIndex);
		}

		public static List<T> Slice<T>(this List<T> list, int startIndex)
		{
			return list.Slice(startIndex, list.Count - startIndex);
		}

		public static List<T> Slice<T>(this List<T> list, int startIndex, int count)
		{
			List<T> slicedArray = new List<T>(count);

			for (int i = 0; i < count; i++)
				slicedArray[i] = list[i + startIndex];

			return slicedArray;
		}

		/// <summary>
		/// A Fisher-Yates shuffle with a set Random
		/// </summary>
		public static void Shuffle<T>(this IList<T> list, System.Random random)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = random.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}
