using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    [Flags]
    public enum LogLevel : byte
    {
        Verbose = 0x0,
        Subtle = 0x1,
        Log = 0x2
    }
}
