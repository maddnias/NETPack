using System;
using System.Collections.Generic;
using System.Drawing;
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
            var watermark = CecilHelper.Inject(Globals.Context.TargetAssembly.MainModule,
                                               (Globals.Context.Injections["Watermark"] as TypeDefinition));

            Globals.Context.TargetAssembly.MainModule.Types.Add(watermark);
            Globals.Context.TargetAssembly.MainModule.CustomAttributes.Add(new CustomAttribute(watermark.Methods[0]));

            if (Globals.Context.Options.MoveReferences)
            {
                foreach (var asm in Globals.Context.MarkedReferences)
                {
                    asm.Key.Write(Path.Combine(Path.GetDirectoryName(Globals.Context.OutPath),
                                               asm.Key.Name.Name + ".dll"));

                    Globals.Context.UIProvider.VerboseLog(
                        string.Format("[Finalize(MoveRef)] -> Moved reference ({0}) to output", asm.Key.Name.Name));
                }
            }

            Globals.Context.TargetAssembly.Write(Globals.Context.OutPath);
            Globals.Context.UIProvider.VerboseLog("[Finalize(Writer)] -> Output written to disk");

            if (Globals.Context.Options.PreserveIcon)
            {
                var data = Globals.Context.AnalysisDatabase["Icon"].Values[0] as Tuple<Icon, int>;

                if (!NativeHelper.UpdateIcon(Globals.Context.OutPath, data.Item1, data.Item2))
                    Globals.Context.UIProvider.VerboseLog("[Finalize(Icon)] -> Failed to update icon");
                else
                    Globals.Context.UIProvider.VerboseLog("[Finalize(Icon)] -> Successfully updated icon");
            }

            Globals.Context.UIProvider.VerboseLog("[Finalize(Logger)] -> Disposed logger stream");
            Globals.Context.LogWriter.Flush();
            Globals.Context.LogWriter.Close();
            Globals.Context.LogWriter.Dispose();
        }
    }
}

