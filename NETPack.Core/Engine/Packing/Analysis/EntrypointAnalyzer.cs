using System.Collections.Generic;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class EntrypointAnalyzer : IAnalyzer
    {
        public List<dynamic> LocalValues { get; set; }

        public string AnalyzationKey
        {
            get { return "Entrypoint"; }
        }

        public AnalysisEntry Entry { get; set; }

        public EntrypointAnalyzer()
        {
            LocalValues = new List<dynamic>();
        }

        public void Analyze(object param)
        {
            var asmDef = (param as AssemblyDefinition);
            var ep = asmDef.EntryPoint;

            var @params = ep.Parameters;

            LocalValues.Add(@params);
            Entry = new AnalysisEntry(LocalValues[0]);
        }

        public void Output()
        {
            Globals.Context.UIProvider.VerboseLog("[Analyze(Entrypoint)] -> " + (LocalValues[0].Count == 1 ? "(string[] args)" : "no param"));

        }
    }
}
