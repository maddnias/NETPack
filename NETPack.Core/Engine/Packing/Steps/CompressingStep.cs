using System.IO;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Steps
{
    class CompressingStep : PackingStep
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

            _decompressor = CecilHelper.Inject(_stubAssembly.MainModule, PackerContext.Injections["Decompressor"] as TypeDefinition);
            _loader = CecilHelper.Inject(_stubAssembly.MainModule, PackerContext.Injections["Loader"] as TypeDefinition);

            base.Initialize();
        }

        public override void ProcessStep()
        {
            TypeDefinition resolver;

            StubWorker.PopulateStub(ref _stubAssembly, _decompressor, _loader, out resolver);
            StubWorker.StripCoreDependency(ref _stubAssembly, _decompressor, resolver);

            using(var ms = new MemoryStream())
            {
                PackerContext.TargetAssembly.Write(ms);
                var tmpBuff = QuickLZ.compress(ms.ToArray(), 3);

                _stubAssembly.MainModule.Resources.Add(new EmbeddedResource("X", ManifestResourceAttributes.Public,
                                                                            tmpBuff));

                Logger.VLog(string.Format("[Pack(Ref)] -> Packed assembly: {0}", PackerContext.TargetAssembly.Name.Name));
            }
        }

        public override void FinalizeStep()
        {
            PackerContext.TargetAssembly = _stubAssembly;
            base.FinalizeStep();
        }
    }
}
