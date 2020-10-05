// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/AboveAverageInspector         --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace instance.id.AAI.Extensions
{
    // -- https://github.com/TeamSirenix/odin-serializer/blob/ecd5ee6cf3d71b654b62926f85e699a3db47d9f6/OdinSerializer/Utilities/Extensions/LinqExtensions.cs
    public static class LinqExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source) action(item);
            return source;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var counter = 0;
            foreach (var item in source) action(item, counter++);
            return source;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, IEnumerable<T> append)
        {
            foreach (var item in source) yield return item;
            foreach (var item in append) yield return item;
        }
    }
}
