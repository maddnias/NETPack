using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Steps
{
    class FinalizerStep : PackingStep
    {
        public FinalizerStep(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override string StepDescription
        {
            get { return "Finalizes the packing process"; }
        }
        public override string StepOutput
        {
            get { return "Finalizing process..."; }
        }

        public override void ProcessStep()
        {
            PackerContext.TargetAssembly.Write(PackerContext.OutPath);
            Logger.VLog("[Finalize(writer)] -> Output written to disk");

            Logger.VLog("[Finalize(logger)] -> Disposed logger stream");
            PackerContext.LogWriter.Flush();
            PackerContext.LogWriter.Close();
            PackerContext.LogWriter.Dispose();
       }
    }
}
