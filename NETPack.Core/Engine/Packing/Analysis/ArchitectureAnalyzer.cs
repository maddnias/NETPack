using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class ArchitectureAnalyzer : IAnalyzer
    {
        public List<dynamic> LocalValues { get; set; }

        public string AnalyzationKey
        {
            get { return "Architecture"; }
        }

        public AnalysisEntry Entry { get; set; }

        public ArchitectureAnalyzer()
        {
            LocalValues = new List<dynamic>();
        }

        public void Analyze(object param)
        {
            var asmDef = (param as AssemblyDefinition);
            var targetArchitecture = asmDef.MainModule.Architecture;

            LocalValues.Add(targetArchitecture);

            Entry = new AnalysisEntry(LocalValues[0]);
        }

        public void Output()
        {
            Globals.Context.UIProvider.VerboseLog("[Analyze(Architecture)] -> Detected: " + LocalValues[0]);
        }
    }
}
