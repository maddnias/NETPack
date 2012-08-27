using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class ArchitectureAnalyzer : Analyzer
    {
        public override string AnalyzationKey
        {
            get { return "Architecture"; }
        }

        public override AnalysisEntry Entry { get; set; }

        public override void Analyze(object param)
        {
            var asmDef = (param as AssemblyDefinition);
            var targetArchitecture = asmDef.MainModule.Architecture;

            LocalValues.Add(targetArchitecture);

            Entry = new AnalysisEntry(LocalValues[0]);
        }

        public override void Output()
        {
            Logger.VLog("[Analyze(Architecture)] -> Detected: " + LocalValues[0]);
        }
    }
}
