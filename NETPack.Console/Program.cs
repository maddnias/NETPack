using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Mono.Cecil;
using NETPack.Core;
using NETPack.Core.Engine.Packing.Steps;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils.Extensions;
using CConsole = System.Console;

namespace NETPack.Console
{
    class Program
    {
        private static StandardContext _ctx = new StandardContext();
        private static OptionSet _stdOptionSet;

        static void Main(string[] args)
        {
            if(args.Length == 0)
                throw new ArgumentNullException("args");

            if(!File.Exists(args[0]))
                throw new FileNotFoundException("in path");

            AssemblyDefinition asmDef = null;

            try
            {
                asmDef = AssemblyDefinition.ReadAssembly(args[0]);
            }
            catch(BadImageFormatException e)
            {
                CConsole.WriteLine("Invalid assembly");
                CConsole.ReadLine();
            }

            _ctx.InPath = args[0];
            _ctx.OutPath = null;

            _ctx.PackingSteps.Add(new AnalysisStep(asmDef));
            _ctx.PackingSteps.Add(new InitializerStep(asmDef));
            _ctx.PackingSteps.Add(new CompressingStep(asmDef));

            _stdOptionSet = new OptionSet
                                {
                                    {"o|out=", "Set output path manually | --out=path", path =>
                                                                                            {
                                                                                                _ctx.OutPath = Path.Combine(path, asmDef.Name.Name + "_packed.exe");
                                                                                            }},

                                    {"m|merge", "Decides wether to merge references or not | --merge", m =>
                                                                                                           {
                                                                                                               if (m != null) _ctx.PackingSteps.Add(new ReferencePackerStep(asmDef));
                                                                                                           }},

                                    {"v|verbose", "Verbose output | --verbose", v =>
                                                                                    {
                                                                                        if(v != null) 
                                                                                            _ctx.LogLevel = LogLevel.Verbose;
                                                                                    }},

                                    {"l|level=", "Sets compression level (1 or 3) | --level=(1/3)", (int l) =>
                                                                                                        {
                                                                                                            if (l != 1 && l != 3)
                                                                                                                throw new OptionException("Invalid compression level", "level");

                                                                                                            _ctx.CompressionLevel = l;
                                                                                                        }},

                                    {"h|help", "Displays info about commands | --help", h =>
                                                                                            {
                                                                                                if(h != null) 
                                                                                                    PrintHelp();
                                                                                            }},

                                    {"u|unpack", "Unpacks file | --unpack", u => {
                                                                                     if (u == null) return;
                                                                                     var up = new Packer(_ctx);

                                                                                     if (!IsPacked(asmDef))
                                                                                     {
                                                                                         CConsole.WriteLine("File not packed?");
                                                                                         CConsole.ReadLine();

                                                                                         Environment.Exit(-1);
                                                                                     }
                                                                                     up.UnpackFile();
                                    }},

                                    {"ap|apmntstate=", "Sets apartment state for loader | --apmntstate=(MTA/STA)", ap =>
                                                                                                                       {
                                                                                                                           if(ap != "STA" && ap != "MTA")
                                                                                                                               throw new OptionException("Invalid apartment state", "apmntstate");

                                                                                                                           _ctx.ApmtState = (ap == "STA" ? ApartmentState.STA : ApartmentState.MTA);
                                                                                                                       }}
                                };

            try
            {
                _stdOptionSet.Parse(args.From(1));
            }
            catch(OptionException e)
            {
                CConsole.Write("Invalid input: ");
                CConsole.WriteLine(e.Message);
            }

            _ctx.PackingSteps.Add(new FinalizerStep(asmDef));

            var p = new Packer(_ctx);
            p.PackFile();
        }

        public static void PrintHelp()
        {
            CConsole.WriteLine("Usage:\n");

            foreach(var cmd in _stdOptionSet)
                CConsole.WriteLine("{0}: {1}", cmd.Names[1], cmd.Description);

            CConsole.ReadLine();
            Environment.Exit(-1);
        }
      
        public static bool IsPacked(AssemblyDefinition asm)
        {
            return asm.MainModule.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "netpackAttrib") != null;
        }
    }
}
