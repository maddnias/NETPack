using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class SubsystemAnalyzer : IAnalyzer
    {
        public List<dynamic> LocalValues { get; set; }

        public string AnalyzationKey
        {
            get { return "Subsys"; }
        }

        public AnalysisEntry Entry { get; set; }

        public SubsystemAnalyzer()
        {
            LocalValues = new List<dynamic>();
        }

        public void Analyze(object param)
        {
            var asmDef = (param as AssemblyDefinition);
            var subSystem = asmDef.MainModule.Kind;

            if (subSystem == ModuleKind.Dll)
                throw new BadImageFormatException("Dll not supported");

            LocalValues.Add(subSystem);

            Entry = new AnalysisEntry(LocalValues[0]);
        }

        public void Output()
        {
            Globals.Context.UIProvider.VerboseLog("[Analyze(Subsystem)] -> Subsystem: " + LocalValues[0].ToString());
        }
    }
}
