using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    public abstract class PackingStep : IPackingStep
    {
        public AssemblyDefinition AsmDef;

        protected PackingStep(AssemblyDefinition asmDef)
        {
            AsmDef = asmDef;
        }

        public virtual bool Delay { get { return false; } }
        public abstract string StepDescription { get; }
        public abstract string StepOutput { get; }

        public virtual void Output()
        {
            Logger.GLog(StepOutput);
        }

        public virtual void Initialize()
        {
            Logger.Indent();
        }
        public abstract void ProcessStep();
        public virtual void FinalizeStep()
        {
            Logger.DeIndent();
        }
    }
}
