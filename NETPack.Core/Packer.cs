using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using NETPack.Core.Engine;
using NETPack.Core.Engine.Packing.Steps;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Structs__Enums___Interfaces.Bugster;
using NETPack.Core.Engine.Utils;
using NETPack.Core.Engine.Utils.Extensions;

namespace NETPack.Core
{
    public class Packer
    {
#region headers

        public const string Header =
            @"
 _        _______ _________ _______  _______  _______  _          _______     __       _______ 
( (    /|(  ____ \\__   __/(  ____ )(  ___  )(  ____ \| \    /\  (  __   )   /  \     (  __   )
|  \  ( || (    \/   ) (   | (    )|| (   ) || (    \/|  \  / /  | (  )  |   \/) )    | (  )  |
|   \ | || (__       | |   | (____)|| (___) || |      |  (_/ /   | | /   |     | |    | | /   |
| (\ \) ||  __)      | |   |  _____)|  ___  || |      |   _ (    | (/ /) |     | |    | (/ /) |
| | \   || (         | |   | (      | (   ) || |      |  ( \ \   |   / | |     | |    |   / | |
| )  \  || (____/\   | |   | )      | )   ( || (____/\|  /  \ \  |  (__) | _ __) (_ _ |  (__) |
|/    )_)(_______/   )_(   |/       |/     \|(_______/|_/    \/  (_______)(_)\____/(_)(_______) 0.1.0 BETA
________________________________________________________________

Created by UbbeLoL | hackforums.net | board.b-at-s.info 
________________________________________________________________";

        public const string ConsoleHeader =
            @" 
 _        _______ _________ _______  _______  _______  _       
( (    /|(  ____ \\__   __/(  ____ )(  ___  )(  ____ \| \    /\
|  \  ( || (    \/   ) (   | (    )|| (   ) || (    \/|  \  / /
|   \ | || (__       | |   | (____)|| (___) || |      |  (_/ / 
| (\ \) ||  __)      | |   |  _____)|  ___  || |      |   _ (  
| | \   || (         | |   | (      | (   ) || |      |  ( \ \ 
| )  \  || (____/\   | |   | )      | )   ( || (____/\|  /  \ \
|/    )_)(_______/   )_(   |/       |/     \|(_______/|_/    \/  0.1.0 BETA
________________________________________________________________

Created by UbbeLoL | hackforums.net | board.b-at-s.info 
________________________________________________________________";

#endregion

        public Packer(PackerContext ctx)
        {
            Globals.Context = ctx;

            ctx.OutPath = ctx.OutPath ?? Path.Combine(Path.GetDirectoryName(ctx.InPath), "NETPack_Output", Path.GetFileName(ctx.InPath) + "_packed.exe");
            ctx.LocalPath = Assembly.GetExecutingAssembly().Location.GetPath();
            ctx.LogWriter = new StreamWriter(Path.Combine(ctx.LocalPath, "log.txt"));
            ctx.TargetAssembly = AssemblyDefinition.ReadAssembly(ctx.InPath);

            if (!ctx.VerifyContext())
                throw new Exception("Failed to verify context!");

            Logger.FLog(Header + "\r\n\r\n");
            Logger.GLog(ConsoleHeader + "\r\n\r\n");

            var bugster = new BugReporter("5351ddb5009c5b025fd1a89409b3f262", new NETPackExceptionFormatter());

            AppDomain.CurrentDomain.UnhandledException += bugster.UnhandledExceptionHandler;
            bugster.ReportCompleted += (o, e) =>
                                           {
                                               if (e.WasSuccesful)
                                               {
                                                   Console.WriteLine(
                                                       "An unhandled exception have occured and caused NETPack to terminate!\n\nAn automatic report have been sent to the author.");
                                               }
                                               else
                                                   Console.WriteLine("Contact author!");
                                           };
        }

        public void PackFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Globals.Context.InPath) + "\\NETPack_Output");
            InternalPack();
        }

        public void UnpackFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Globals.Context.InPath) + "\\NETPack_Output");
            InternalUnpack();
        }

        private void InternalUnpack()
        {
            foreach (var res in Globals.Context.TargetAssembly.MainModule.Resources)
            {
                if (res.Name == "X")
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Globals.Context.OutPath), "Main.exe"),
                                       QuickLZ.decompress((res as EmbeddedResource).GetResourceData()));
                else
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Globals.Context.OutPath), res.Name.MangleName() + ".dll"),
                                       QuickLZ.decompress((res as EmbeddedResource).GetResourceData()));

                Logger.GLog("Unpacked file: " + (res.Name == "X" ? "X (Main assembly)" : res.Name.MangleName() + ".dll"));
            }

            Console.ReadLine();
        }

        private void InternalPack()
        {
            var steps = (Globals.Context as StandardContext).PackingSteps;
            var initSize = new FileInfo(Globals.Context.InPath).Length;
            var endSize = 0;

            var sw = new Stopwatch();
            sw.Start();

            Logger.GLog(string.Format("Initialized packing process at [{0}]\r\nTarget: [{1}]\r\n\r\n",
                                      DateTime.Now.ToString("HH:mm:ss"), Globals.Context.TargetAssembly.FullName));

            foreach(var step in steps.FindAll(x => !x.Delay))
            {
                step.Output();
                step.Initialize();
                step.ProcessStep();
                step.FinalizeStep();
            }

            foreach (var step in steps.FindAll(x => x.Delay))
            {
                step.Output();
                step.Initialize();
                step.ProcessStep();
                step.FinalizeStep();
            }

            sw.Stop();

            var ratio =
                Convert.ToInt32(-(100 - initSize * 100 / (endSize = (int)new FileInfo(Globals.Context.OutPath).Length)));

            Logger.GLog(string.Format("\nFile size reduced by "), false);

            if (ratio >= 0 && ratio <= 20)
                Console.ForegroundColor = ConsoleColor.Yellow;
            if(ratio >= 21)
                Console.ForegroundColor = ConsoleColor.Green;
            if(ratio <= -1)
                Console.ForegroundColor = ConsoleColor.Red;

            Logger.GLog("~" + ratio + "%", false);

            Console.ForegroundColor = ConsoleColor.Gray;

            Logger.GLog(string.Format(" ({0} -> {1})", ((int)initSize).GetSuffix(), endSize.GetSuffix()));
            Logger.GLog(string.Format("\r\nPacking process finished at [{0}]\r\nTotal time: [{1}]", DateTime.Now.ToString("HH:mm:ss"), sw.Elapsed));

            Console.ReadLine();
        }
    }
}
