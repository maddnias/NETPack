using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;
using NETPack.Core.Engine.Utils.Extensions;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class ReflectionAnalyzer : Analyzer
    {
        public override string AnalyzationKey
        {
            get { return "Reflection"; }
        }

        public override AnalysisEntry Entry { get; set; }

        public override void Analyze(object param)
        {
            var asmDef = (param as AssemblyDefinition);

            foreach (var tDef in asmDef.MainModule.GetAllTypes())
            {
                foreach (var mDef in tDef.Methods.Where(x => x.HasBody))
                    foreach (var instr in mDef.Body.Instructions)
                        if (instr.OpCode.OperandType == Mono.Cecil.Cil.OperandType.InlineMethod)
                            if (instr.Operand.ToString().Contains("System.Reflection"))
                            {
                                LocalValues.Add(true);
                                break;
                            }
            }

            if (LocalValues.Count == 0)
                LocalValues.Add(false);

            Entry = new AnalysisEntry(LocalValues[0]);
        }

        public override void Output()
        {
            Logger.VLog("[Analyze(Reflection)] -> " + LocalValues[0].ToString());
        }
    }
}
