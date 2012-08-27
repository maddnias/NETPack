using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

using NETPack.Core.Engine.Packing.Analysis;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;
using NETPack.Core.Engine.Utils.Extensions;

using ctx = NETPack.Core.Engine.PackerContext;

namespace NETPack.Core.Engine.Packing.Steps
{
    class InitializerStep : PackingStep
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
            _localAsmDef = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location);
            ctx.Injections = new Dictionary<string, IMemberDefinition>();

            base.Initialize();
        }

        public override void ProcessStep()
        {
            ctx.Injections.Add("Decompressor", _localAsmDef.GetInjection("Decompressor"));
            ctx.Injections.Add("Loader", _localAsmDef.GetInjection("Loader"));
            ctx.Injections.Add("Resolver", _localAsmDef.GetInjection("Resolver"));
        }
    }
}
