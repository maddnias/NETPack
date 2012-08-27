using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETPack.Core.Engine.Utils.Extensions;

namespace NETPack.Core.Engine.Utils
{
    public static class StubWorker
    {
        public static AssemblyDefinition GenerateStub()
        {
            var stub = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("netpack", new Version(1, 0, 0, 0)), "netpack", ModuleKind.Console);

            stub.MainModule.Kind = PackerContext.AnalysisDatabase["Subsys"].Values[0];
            stub.MainModule.Architecture = PackerContext.AnalysisDatabase["Architecture"].Values[0];
            stub.MainModule.Runtime = PackerContext.AnalysisDatabase["CLRVer"].Values[0];

            return stub;
        }

        public static void PopulateStub(ref AssemblyDefinition stub, TypeDefinition decompressor, TypeDefinition loader, out TypeDefinition resolver)
        {
            resolver = null;

            stub.MainModule.Types.Add(decompressor);
            stub.MainModule.Types.Add(loader);
            stub.EntryPoint = loader.Methods.First(x => x.Name == "Main");

            if (PackerContext.AnalysisDatabase["Entrypoint"].Values[0].Count == 0)
                StripArguments(loader);

            if(PackerContext.AnalysisDatabase["AsmRef"].Values.Count >= 1)
                PackReferences(ref stub, out resolver);
        }

        private static void PackReferences(ref AssemblyDefinition stub, out TypeDefinition resolver)
        {
            var localAsmDef = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location);
            resolver = CecilHelper.Inject(stub.MainModule, localAsmDef.GetInjection("Resolver") as TypeDefinition);

            stub.MainModule.Types.Add(resolver);

            foreach (var @ref in PackerContext.AnalysisDatabase["AsmRef"].Values)
            {
                var fixedPath = Path.Combine(Path.GetDirectoryName(PackerContext.InPath), @ref.Name + ".dll");

                AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(fixedPath);
                var buff = File.ReadAllBytes(fixedPath);
                stub.MainModule.Resources.Add(new EmbeddedResource(asm.Name.Name.MangleName(), ManifestResourceAttributes.Public, QuickLZ.compress(buff, 3)));

                Logger.VLog(string.Format("[Pack(Ref)] -> Packed reference: {0}", asm.Name.Name));
            }

            var ilProc = stub.EntryPoint.Body.GetILProcessor();

            ilProc.InsertBefore(ilProc.Body.Instructions[0],
                                ilProc.Create(OpCodes.Call, resolver.Methods.First(x => x.Name == "AddHandler")));
        }

        private static void StripArguments(TypeDefinition loader)
        {
            var run = loader.Methods.First(x => x.Name == "run");
            var targetInstr = run.Body.Instructions.First(x => x.OpCode == OpCodes.Newarr);
            var ilProc = run.Body.GetILProcessor();

            // "new object[] { args }" -> "new object[0]"

            targetInstr.Previous.OpCode = OpCodes.Ldc_I4_0;                 //ldc.i4.1 -> ldc.i4.0 (array initializer)

            ilProc.Remove(targetInstr.Next.Next.Next.Next.Next.Next);       //ldloc.3
            ilProc.Remove(targetInstr.Next.Next.Next.Next.Next);            //stelem.ref
            ilProc.Remove(targetInstr.Next.Next.Next.Next);                 //ldloc.1
            ilProc.Remove(targetInstr.Next.Next);                           //ldloc.3
            ilProc.Remove(targetInstr.Next);                                //stloc.3
            ilProc.Remove(targetInstr.Next);                                //ldc.i4.0
        }

        public static void StripCoreDependency(ref AssemblyDefinition stub, TypeDefinition decompressor, TypeDefinition resolver)
        {
            // Update in entry point
            var targetInstr =
                stub.EntryPoint.Body.Instructions.First(
                    x => x.OpCode.OperandType == OperandType.InlineMethod && x.Operand.ToString().Contains("K::D"));

            targetInstr.Operand = decompressor.Methods.First(x => x.Name == "D");

            if (resolver != null)
            {
                // Update in resolver
                targetInstr =
                    resolver.Methods[1].Body.Instructions.First(
                        x =>
                        x.OpCode.OperandType == OperandType.InlineMethod &&
                        x.Operand.ToString().Contains("Decompressor::D"));

                targetInstr.Operand = decompressor.Methods.First(x => x.Name == "D");
            }

            stub.MainModule.AssemblyReferences.Remove(
                stub.MainModule.AssemblyReferences.First(x => x.Name == "NETPack.Core"));
        }
    }
}
