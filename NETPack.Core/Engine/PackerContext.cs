using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;

namespace NETPack.Core.Engine
{
    public abstract class PackerContext
    {
        public virtual bool VerifyContext()
        {
            return !@LogLevel.HasFlag(LogLevel.Subtle) || !@LogLevel.HasFlag(LogLevel.Verbose);
        }

        public List<string> MarkedAssemblies = new List<string>();
        public AssemblyDefinition TargetAssembly;
        public LogLevel @LogLevel = LogLevel.Subtle;
        public Dictionary<string, AnalysisEntry> AnalysisDatabase = new Dictionary<string, AnalysisEntry>();
        public StreamWriter LogWriter;
        public string LocalPath;

        public Dictionary<string, IMemberDefinition> Injections;

        public string InPath;
        public string OutPath;
    }
}
