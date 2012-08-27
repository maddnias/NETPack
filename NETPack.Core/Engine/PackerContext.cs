using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;

namespace NETPack.Core.Engine
{
    public static class PackerContext
    {
        public static bool VerifyContext()
        {
            return !@LogLevel.HasFlag(LogLevel.Subtle) || !@LogLevel.HasFlag(LogLevel.Verbose);
        }

        public static AssemblyDefinition TargetAssembly;
        public static LogLevel @LogLevel;
        public static Dictionary<string, AnalysisEntry> AnalysisDatabase = new Dictionary<string, AnalysisEntry>();
        public static StreamWriter LogWriter;
        public static string LocalPath;

        public static Dictionary<string, IMemberDefinition> Injections;

        public static string InPath;
        public static string OutPath;
    }
}
