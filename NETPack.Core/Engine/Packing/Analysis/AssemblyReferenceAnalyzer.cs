using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;
using NETPack.Core.Linker.Mono.Linker;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class AssemblyReferenceAnalyzer : Analyzer
    {
        public override string AnalyzationKey { get { return "AsmRef"; } }
        public override AnalysisEntry Entry { get; set; }

        public override void Analyze(object param)
        {
            var asmDef = (param as AssemblyDefinition);

            foreach (var @ref in asmDef.MainModule.AssemblyReferences.Where(x => !LinkContext.IsCore(x)))
                LocalValues.Add(@ref);

            Entry = new AnalysisEntry(LocalValues.ToArray());
        }

        public override void Output()
        {
            foreach(var @ref in LocalValues)
                Logger.VLog("[Analyze(AsmRef)] -> Non-core ref:" + @ref.Name);
        }
    }
}
