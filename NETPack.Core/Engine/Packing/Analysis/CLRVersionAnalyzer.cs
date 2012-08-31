using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class CLRVersionAnalyzer : IAnalyzer
    {
        public List<dynamic> LocalValues { get; set; }

        public string AnalyzationKey { get { return "CLRVer"; } }
        public AnalysisEntry Entry { get; set; }

        public CLRVersionAnalyzer()
        {
            LocalValues = new List<dynamic>();
        }

        public void Analyze(object param)
        {
            var asmDef = (param as AssemblyDefinition);
            var targetRuntime = asmDef.MainModule.Runtime;

            LocalValues.Add(targetRuntime);

            Entry = new AnalysisEntry(LocalValues[0]);
        }

        public void Output()
        {
            Globals.Context.UIProvider.VerboseLog("[Analyze(CLRVer)] -> Detected: " + LocalValues[0]);
        }

    }
}
