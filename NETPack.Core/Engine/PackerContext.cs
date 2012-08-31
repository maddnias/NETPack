using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;

namespace NETPack.Core.Engine
{
    public interface IPackerContext
    {
        bool VerifyContext();

        IUserInterfaceProvider UIProvider { get; set; }
        List<string> MarkedAssemblies { get; set; }
        AssemblyDefinition TargetAssembly { get; set; }

        Dictionary<string, AnalysisEntry> AnalysisDatabase { get; set; }
        StreamWriter LogWriter { get; set; }
        string LocalPath { get; set; }
        PackerOptionSet Options { get; set; }

        List<IPackingStep> PackingSteps { get; set; }
        Dictionary<AssemblyDefinition, string> MarkedReferences { get; set; }

        Dictionary<string, IMemberDefinition> Injections { get; set; }

        string InPath { get; set; }
        string OutPath { get; set; }
    }

    public abstract class PackerContext : IPackerContext
    {
        public IUserInterfaceProvider UIProvider { get; set; }

        public abstract bool VerifyContext();

        public List<IPackingStep> PackingSteps { get; set; }
        public Dictionary<AssemblyDefinition, string> MarkedReferences { get; set; }

        public List<string> MarkedAssemblies { get; set; }
        public AssemblyDefinition TargetAssembly { get; set; }
        public Dictionary<string, AnalysisEntry> AnalysisDatabase { get; set; }
        public StreamWriter LogWriter { get; set; }

        public string LocalPath { get; set; }
        public PackerOptionSet Options { get; set; }

        public Dictionary<string, IMemberDefinition> Injections { get; set; }

        public string InPath { get; set; }
        public string OutPath { get; set; }
    }
}
