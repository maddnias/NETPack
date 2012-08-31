using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    public interface IPackingStep
    {
        void Initialize();
        void ProcessStep();
        void FinalizeStep();
        void Output();

        bool Delay { get; }
    }

    public abstract class PackingStep : IPackingStep
    {
        public AssemblyDefinition AsmDef;

        protected PackingStep(AssemblyDefinition asmDef)
        {
            AsmDef = asmDef;
            NewLine = true;
        }

        public virtual bool NewLine { get; set; }
        public virtual bool Delay { get { return false; } }
        public abstract string StepDescription { get; }
        public abstract string StepOutput { get; }

        public virtual void Output()
        {
            if (NewLine)
                Globals.Context.UIProvider.GlobalLog(StepOutput);
            else
                Globals.Context.UIProvider.GlobalLogNoNewline(StepOutput);
        }

        public virtual void Initialize()
        {
            Globals.Context.UIProvider.Indent();
        }
        public abstract void ProcessStep();
        public virtual void FinalizeStep()
        {
            Globals.Context.UIProvider.DeIndent();
        }
    }
}
