using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETPack.Core.Engine.Compression;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Steps
{
    public class CompressingStep : PackingStep
    {
        private AssemblyDefinition _stubAssembly;

        private TypeDefinition _decompressor, _loader;

        public CompressingStep(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override string StepDescription
        {
            get { return ""; }
        }
        public override string StepOutput
        {
            get { return "Compressing and moving data..."; }
        }

        public override void Initialize()
        {
            _stubAssembly = StubWorker.GenerateStub();

            _decompressor = CecilHelper.Inject(_stubAssembly.MainModule, Globals.Context.Injections["Decompressor"] as TypeDefinition);
            _loader = CecilHelper.Inject(_stubAssembly.MainModule, Globals.Context.Injections["Loader"] as TypeDefinition);

            base.Initialize();
        }

        public override void ProcessStep()
        {
            TypeDefinition resolver;

            StubWorker.PopulateStub(ref _stubAssembly, _decompressor, _loader, out resolver);
            StubWorker.StripCoreDependency(ref _stubAssembly, _decompressor, resolver);

            using(var ms = new MemoryStream())
            {
                Globals.Context.TargetAssembly.Write(ms);
                var tmpBuff = QuickLZ.compress(ms.ToArray(), Globals.Context.Options.CompressionLevel);

                _stubAssembly.MainModule.Resources.Add(new EmbeddedResource("X", ManifestResourceAttributes.Public,
                                                                            tmpBuff));

                Globals.Context.UIProvider.VerboseLog(string.Format("[Pack(Main)] -> Packed assembly: {0}", Globals.Context.TargetAssembly.Name.Name));
            }
        }

        public override void FinalizeStep()
        {
            Globals.Context.TargetAssembly = _stubAssembly;
            base.FinalizeStep();
        }
    }
}
