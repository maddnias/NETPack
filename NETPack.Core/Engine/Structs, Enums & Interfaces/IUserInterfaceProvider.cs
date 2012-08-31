using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    public interface IUserInterfaceProvider
    {
        void Indent();
        void DeIndent();

        LogLevel @LogLevel { get; set; }

        Action<string> GlobalLog { get; }
        Action<string> VerboseLog { get; }
        Action<string> SubtleLog { get; }

        Action<string> GlobalLogNoNewline { get; }
        Action<string> VerboseLogNoNewline { get; }
        Action<string> SubtleLogNoNewline { get; }
    }

    public abstract class DefaultUIProvider: IUserInterfaceProvider
    {
        protected DefaultUIProvider(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }

        private static int _indendationLevel;
        public static string Indentation
        {
            get
            {
                var str = "";

                for (var i = 0; i < _indendationLevel; i++)
                    str += '\t';

                return str;
            }
        }

        public void Indent()
        {
            _indendationLevel++;
        }
        public void DeIndent()
        {
            _indendationLevel--;
        }

        public LogLevel @LogLevel { get; set; }

        public abstract Action<string> GlobalLog { get; }
        public abstract Action<string> VerboseLog { get; }
        public abstract Action<string> SubtleLog { get; }

        public abstract Action<string> GlobalLogNoNewline { get; }
        public abstract Action<string> VerboseLogNoNewline { get; }
        public abstract Action<string> SubtleLogNoNewline { get; }
    }
}
