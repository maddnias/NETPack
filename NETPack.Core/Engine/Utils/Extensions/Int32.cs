using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Utils.Extensions
{
    public static class Int32Ext
    {
        public static string GetSuffix(this int size)
        {
            if (size >= 0 && size <= 1048576)
                return Convert.ToString(size/1024) + "kb";
            if (size >= 1048576 && size <= 1073741824)
                return Convert.ToString(size/1048576) + "mb";

            return Convert.ToString(size / 1073741824) + "gb";
        }
    }
}
