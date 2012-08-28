using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Utils.Extensions
{
    public static class DictionaryExt
    {
        public static void ForEach<T, TU>(this Dictionary<T, TU> dictionary, Action<KeyValuePair<T, TU>> func)
        {
            foreach (var itm in dictionary)
                func(itm);
        }
    }
}
