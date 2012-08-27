using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class CLRVersionAnalyzer : Analyzer
    {
        public override string AnalyzationKey
        {
            get { return "CLRVer"; }
        }

        public override AnalysisEntry Entry { get; set; }

        public override void Analyze(object param)
        {
            var asmDef = (param as AssemblyDefinition);
            var targetRuntime = asmDef.MainModule.Runtime;

            LocalValues.Add(targetRuntime);

            Entry = new AnalysisEntry(LocalValues[0]);
        }

        public override void Output()
        {
            Logger.VLog("[Analyze(CLRVer)] -> Detected: " + LocalValues[0]);
        }
    }
}
