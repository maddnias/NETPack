using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Utils.Extensions
{
    public static class ArrayExt
    {
        public static T[] From<T>(this T[] arr, int index)
        {
            return GetFrom(arr, index).ToArray();
        }

        private static IEnumerable<T> GetFrom<T>(T[] arr, int index)
        {
            for (var i = index; i < arr.Length; i++)
                yield return arr[i];
        }
    }
}
