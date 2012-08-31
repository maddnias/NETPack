using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

using NETPack.Core.Engine.Packing.Analysis;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;
using NETPack.Core.Engine.Utils.Extensions;

namespace NETPack.Core.Engine.Packing.Steps
{
    public class InitializerStep : PackingStep
    {
        private AssemblyDefinition _localAsmDef;

        public InitializerStep(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override string StepDescription
        {
            get { return ""; }
        }
        public override string StepOutput
        {
            get { return "Initializing..."; }
        }

        public override void Initialize()
        {
            if (!File.Exists(Path.Combine(Globals.Context.LocalPath, "1033\\PEVerify.exe")))
                throw new FileNotFoundException("PEVerify missing");

            _localAsmDef = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location);
            Globals.Context.Injections = new Dictionary<string, IMemberDefinition>();

            base.Initialize();
        }

        public override void ProcessStep()
        {
            Globals.Context.Injections.Add("Decompressor", _localAsmDef.GetInjection("Decompressor"));
            Globals.Context.Injections.Add("Loader", _localAsmDef.GetInjection("Loader"));
            Globals.Context.Injections.Add("Resolver", _localAsmDef.GetInjection("Resolver"));
            Globals.Context.Injections.Add("Watermark", _localAsmDef.GetInjection("Watermark"));
        }
    }
}
