using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NETPack.Core.Engine.Structs__Enums___Interfaces;

namespace NETPack.Core.Engine
{
    public class ConsoleLogger : DefaultUIProvider
    {
        public ConsoleLogger(LogLevel logLevel)
            : base(logLevel)
        {
        }

        public override Action<string> GlobalLog
        {
            get { return msg => Console.WriteLine(Indentation + msg); }
        }

        public override Action<string> VerboseLog
        {
            get { return msg => { if (Globals.Context.Options.LogLevel == LogLevel.Verbose) Console.WriteLine(Indentation + msg); }; }
        }

        public override Action<string> SubtleLog
        {
            get { return msg => { if (Globals.Context.Options.LogLevel == LogLevel.Subtle) Console.WriteLine(Indentation + msg); }; }
        }

        public override Action<string> GlobalLogNoNewline
        {
            get { return msg => Console.Write(Indentation + msg); }
        }

        public override Action<string> VerboseLogNoNewline
        {
            get { return msg => { if (Globals.Context.Options.LogLevel == LogLevel.Verbose) Console.Write(Indentation + msg); }; }
        }

        public override Action<string> SubtleLogNoNewline
        {
            get { return msg => { if (Globals.Context.Options.LogLevel == LogLevel.Subtle) Console.Write(Indentation + msg); }; }
        }
    }
}
