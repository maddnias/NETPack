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

        public Packer(string inPath)
        {
            Logger.FLog(Header + "\r\n\r\n");
            Logger.VLog(ConsoleHeader + "\r\n\r\n");

            PackerContext.InPath = inPath;
            PackerContext.OutPath = Path.Combine(Path.GetDirectoryName(inPath), "NETPack_Output", Path.GetFileName(inPath) + "_packed.exe");
            PackerContext.LogLevel = LogLevel.Verbose | LogLevel.Log;
            PackerContext.LocalPath = Assembly.GetExecutingAssembly().Location.GetPath();
            PackerContext.LogWriter = new StreamWriter(Path.Combine(PackerContext.LocalPath, "log.txt"));
            PackerContext.TargetAssembly = AssemblyDefinition.ReadAssembly(PackerContext.InPath);

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
            Directory.CreateDirectory(Path.GetDirectoryName(PackerContext.InPath) + "\\NETPack_Output");
            InternalPack();
        }

        public void UnpackFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PackerContext.InPath) + "\\NETPack_Output");
            InternalUnpack();
        }

        private void InternalUnpack()
        {
            foreach(var res in PackerContext.TargetAssembly.MainModule.Resources)
            {
                if (res.Name == "X")
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(PackerContext.OutPath), "Main.exe"),
                                       QuickLZ.decompress((res as EmbeddedResource).GetResourceData()));
                else
                    File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(PackerContext.OutPath), res.Name.MangleName() + ".dll"),
                                       QuickLZ.decompress((res as EmbeddedResource).GetResourceData()));

                Logger.VLog("Unpacked file: " + (res.Name == "X" ? "X (Main assembly)" : res.Name.MangleName() + ".dll"));
            }

            Console.ReadLine();
        }

        private void InternalPack()
        {
            var steps = new List<PackingStep>
                            {
                                new LinkerStep(PackerContext.TargetAssembly),
                                new AnalysisStep(PackerContext.TargetAssembly),
                                new InitializerStep(PackerContext.TargetAssembly),
                                new CompressingStep(PackerContext.TargetAssembly),
                                new FinalizerStep(PackerContext.TargetAssembly),
                            };
            var initSize = new FileInfo(PackerContext.InPath).Length;
            var endSize = 0;

            var sw = new Stopwatch();
            sw.Start();

            Logger.VLog(string.Format("Initialized packing process at [{0}]\r\nTarget: [{1}]\r\n\r\n",
                                      DateTime.Now.ToString("HH:mm:ss"), PackerContext.TargetAssembly.FullName));

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
                Convert.ToInt32(-(100 - initSize*100/(endSize = (int) new FileInfo(PackerContext.OutPath).Length)));

            Logger.VLog(string.Format("\nFile size reduced by "), false);

            if (ratio >= 0 && ratio <= 20)
                Console.ForegroundColor = ConsoleColor.Yellow;
            if(ratio >= 21)
                Console.ForegroundColor = ConsoleColor.Green;
            if(ratio <= -1)
                Console.ForegroundColor = ConsoleColor.Red;

            Logger.VLog("~" + ratio + "%", false);

            Console.ForegroundColor = ConsoleColor.Gray;

            Logger.VLog(string.Format(" ({0} -> {1})", ((int)initSize).GetSuffix(), endSize.GetSuffix()));
            Logger.VLog(string.Format("\r\nPacking process finished at [{0}]\r\nTotal time: [{1}]", DateTime.Now.ToString("HH:mm:ss"), sw.Elapsed));

            Console.ReadLine();
        }

        private void GlobalExcHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var error = e.ExceptionObject as Exception;
            var errorInformation = new StringBuilder();

            errorInformation.Append("Error message:\r\n\t" + error.Message);
            errorInformation.Append("\r\n\r\nTarget site:\r\n\t" + error.TargetSite);
            errorInformation.Append("\r\n\r\nInner Exception:\r\n\t" + error.InnerException);
            errorInformation.Append("\r\n\r\nStack trace:\r\n\r\n");

            foreach (var obj in error.StackTrace)
                errorInformation.Append(obj.ToString());

            try
            {
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "error.txt"),
                                  errorInformation.ToString());
            }
            catch
            {
                // Don't want to go into an endless loop haha
                Console.WriteLine("Could not write error information file!");
            }

            Console.WriteLine(
                "An exception has been thrown which was not handled!\n\nMessage:\n{0}\n\nA text file containing the error information" +
                " has been generated in NETPack's directory, please send the information to ubbelolhfb@gmail.com",
                error.Message);
            Console.ReadLine();
        }

    }
}
