using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class EntrypointAnalyzer : Analyzer
    {
        public override string AnalyzationKey
        {
            get { return "Entrypoint"; }
        }

        public override AnalysisEntry Entry { get; set; }

        public override void Analyze(object param)
        {
            var asmDef = (param as AssemblyDefinition);
            var ep = asmDef.EntryPoint;

            var @params = ep.Parameters;

            LocalValues.Add(@params);
            Entry = new AnalysisEntry(LocalValues[0]);
        }

        public override void Output()
        {
            Logger.VLog("[Analyze(Entrypoint)] -> " + (LocalValues[0].Count == 1 ? "(string[] args)" : "no param"));

        }
    }
}
