using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;
using NETPack.Core.Engine.Utils.Extensions;
using NETPack.Core.Linker.Mono.Linker;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class AssemblyReferenceAnalyzer : IAnalyzer
    {
        public List<dynamic> LocalValues { get; set; }
        public string AnalyzationKey { get { return "AsmRef"; } }
        public AnalysisEntry Entry { get; set; }

        private AssemblyDefinition _asmDef;

        public void Analyze(object param)
        {
            _asmDef = (param as AssemblyDefinition);

            foreach (var @ref in RecursiveReferenceIdentifier())
                LocalValues.Add(@ref);

            Entry = new AnalysisEntry(LocalValues.ToArray());
        }

        public AssemblyReferenceAnalyzer()
        {
            LocalValues = new List<dynamic>();
        }

        public void Output()
        {
            foreach(var @ref in LocalValues)
                Globals.Context.UIProvider.VerboseLog("[Analyze(AsmRef)] -> Non-core ref:" + @ref.Name);
        }

        public AssemblyNameReference[] RecursiveReferenceIdentifier()
        {
            return GetReferences(_asmDef).ToArray();
        }

        public IEnumerable<AssemblyNameReference> GetReferences(AssemblyDefinition asmDef)
        {
            foreach (var @ref in asmDef.MainModule.AssemblyReferences.Where(x => !LinkContext.IsCore(x) && !Globals.Context.MarkedAssemblies.Contains(x.FullName)))
            {
                var resolved = @ref.ResolveReference();

                yield return @ref;

                if (resolved.MainModule.AssemblyReferences.Where(x => !LinkContext.IsCore(x)).ToArray().Length > 0)
                    foreach (var _ref in GetReferences(@ref.ResolveReference()))
                        yield return _ref;
            }
        }
    }
}
