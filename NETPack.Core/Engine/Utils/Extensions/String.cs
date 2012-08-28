using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Utils.Extensions
{
    public static class StringExt
    {
        public static string GetPath(this string fullPath)
        {
            return Path.GetDirectoryName(fullPath);
        }

        public static string MangleName(this string original)
        {
            return new string(original.Select(x => (char) (x ^ 13)).ToArray());
        }
    }
}
