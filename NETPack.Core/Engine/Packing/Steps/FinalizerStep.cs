using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Steps
{
    public class FinalizerStep : PackingStep
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
            var watermark = CecilHelper.Inject(Globals.Context.TargetAssembly.MainModule, (Globals.Context.Injections["Watermark"] as TypeDefinition));

            Globals.Context.TargetAssembly.MainModule.Types.Add(watermark);
            Globals.Context.TargetAssembly.MainModule.CustomAttributes.Add(new CustomAttribute(watermark.Methods[0]));

            if ((Globals.Context as StandardContext).MoveReferences)
            {
                foreach (var asm in (Globals.Context as StandardContext).MarkedReferences)
                {
                    asm.Key.Write(Path.Combine(Path.GetDirectoryName(Globals.Context.OutPath),
                                               asm.Key.Name.Name + ".dll"));

                    Logger.VLog(string.Format("[Finalize(MoveRef)] -> Moved reference ({0}) to output", asm.Key.Name.Name));
                }
            }

            Globals.Context.TargetAssembly.Write(Globals.Context.OutPath);
            Logger.VLog("[Finalize(Writer)] -> Output written to disk");

            Logger.VLog("[Finalize(Logger)] -> Disposed logger stream");
            Globals.Context.LogWriter.Flush();
            Globals.Context.LogWriter.Close();
            Globals.Context.LogWriter.Dispose();
       }
    }
}
