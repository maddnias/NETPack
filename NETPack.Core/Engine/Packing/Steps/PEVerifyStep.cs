using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;
using NETPack.Core.Engine.Utils.Extensions;

namespace NETPack.Core.Engine.Packing.Steps
{
    public class PEVerifyStep : PackingStep
    {
        private PEVerifyWrapper _peverifier;

        public PEVerifyStep(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override bool NewLine { get { return false; } }

        public override string StepDescription
        {
            get { return "Verifies packed output file..."; }
        }

        public override string StepOutput
        {
            get { return "Verifying output..."; }
        }

        public override void Initialize()
        {
            _peverifier = new PEVerifyWrapper();
        }

        public override void ProcessStep()
        {
            _peverifier.VerifyPE(Globals.Context.OutPath);

            if(_peverifier.IsSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Globals.Context.UIProvider.GlobalLog(" 0 errors!");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Globals.Context.UIProvider.GlobalLog(string.Format(" {0} errors!", _peverifier.Errors.Count));
                Console.ForegroundColor = ConsoleColor.Gray;

                var errorBuilder = new StringBuilder();

                foreach(var error in _peverifier.Errors)
                {
                    errorBuilder.Append(string.Format(
                        "PEVerify error:\n" +
                        "\tType: {0}\n" +
                        "\tMDToken: {1}\n" +
                        "\tFile: {2}\n" +
                        "\tMethod: {3}\n" +
                        "\tOffset: {4}\n" +
                        "\tMessage: {5}\n\n", error.ErrorType, error.MDToken, error.File, error.Method, error.Offset,
                        error.Message));
                }

                Globals.Bugster.ManualReport(errorBuilder.ToString(), "PEVerify failure");
            }
        }
    }
}
