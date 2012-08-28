using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NETPack.Core.Engine.Structs__Enums___Interfaces;

namespace NETPack.Core.Engine.Utils
{
    public static class Logger
    {
        private static int _indendationLevel;

        private static string Indentation
        {
            get
            {
                var str = "";

                for (var i = 0; i < _indendationLevel; i++)
                    str += '\t';

                return str;
            }
        }

        public static void Indent()
        {
            _indendationLevel++;
        }

        public static void DeIndent()
        {
            _indendationLevel--;
        }

        public static void VLog(string message, bool newLine = true)
        {
            if (PackerContext.@LogLevel.HasFlag(LogLevel.Verbose))
                if (newLine)
                    Console.WriteLine(Indentation + message);
                else
                    Console.Write(Indentation + message);

          //  if (PackerContext.@LogLevel.HasFlag(LogLevel.Log))
               // PackerContext.LogWriter.WriteLine(Indentation + message);
        }

        public static void SLog(string message)
        {
            if (PackerContext.@LogLevel.HasFlag(LogLevel.Subtle))
                Console.WriteLine(Indentation + message);

            if (PackerContext.@LogLevel.HasFlag(LogLevel.Log))
                PackerContext.LogWriter.WriteLine(Indentation + message);
        }

        public static void FLog(string message, bool newLine = true)
        {
            if (PackerContext.@LogLevel.HasFlag(LogLevel.Log))
                if(newLine)
                    PackerContext.LogWriter.WriteLine(Indentation + message);
                else
                    PackerContext.LogWriter.Write(Indentation + message);
        }

        public static void GLog(string message)
        {
            Console.WriteLine(Indentation + message);
            PackerContext.LogWriter.WriteLine(Indentation + message);
        }
    }
}
