using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NETPack.Core.Engine.Structs__Enums___Interfaces;

namespace NETPack.Core
{
    public class PackerOptionSet
    {
        public bool MoveReferences = true;
        public int CompressionLevel = 3;
        public ApartmentState ApmtState = ApartmentState.STA;
        public bool VerifyOutput = true;
        public LogLevel @LogLevel = LogLevel.Subtle;
    }
}
