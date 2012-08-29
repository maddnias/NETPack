using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Packing.Analysis;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils.Extensions;

namespace NETPack.Core.Engine.Packing.Steps
{
    public class AnalysisStep : PackingStep
    {
        private Dictionary<Analyzer, dynamic> _analyzers;

        public AnalysisStep(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override string StepDescription
        {
            get { return "Analyze assembly in order to optimize packing process"; }
        }
        public override string StepOutput
        {
            get { return "Analyzing assembly..."; }
        }

        public override void Initialize()
        {
            _analyzers = new Dictionary<Analyzer, dynamic>
                             {
                                 {new CLRVersionAnalyzer(), AsmDef},
                                 {new AssemblyReferenceAnalyzer(), AsmDef},
                                 {new SubsystemAnalyzer(), AsmDef},
                                 {new ReflectionAnalyzer(), AsmDef},
                                 {new ArchitectureAnalyzer(), AsmDef},
                                 {new EntrypointAnalyzer(), AsmDef}
                             };

            base.Initialize();
        }

        public override void ProcessStep()
        {
            // KVP
            // Key = IAnalyzer
            // Value = dynamic param

            _analyzers.ForEach(kvp => kvp.Key.Analyze(kvp.Value));
            _analyzers.ForEach(kvp =>
                                   {
                                       Globals.Context.AnalysisDatabase.Add(kvp.Key.AnalyzationKey,
                                                                          kvp.Key.Entry);
                                       kvp.Key.Output();
                                   });
        }
    }
}
