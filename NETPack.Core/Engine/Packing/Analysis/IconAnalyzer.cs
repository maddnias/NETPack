using System;
using System.Collections.Generic;
using System.Drawing;
using Mono.Cecil;
using NETPack.Core.Engine.Structs__Enums___Interfaces;
using NETPack.Core.Engine.Utils;

namespace NETPack.Core.Engine.Packing.Analysis
{
    public class IconAnalyzer : IAnalyzer
    {
        public List<dynamic> LocalValues { get; set; }

        public string AnalyzationKey { get { return "Icon"; } }
        public AnalysisEntry Entry { get; set; }

        public IconAnalyzer()
        {
            LocalValues = new List<dynamic>();
        }

        public void Analyze(object param)
        {
            int size;
            var ico = NativeHelper.ExtractIcon(param as string, out size);

            LocalValues.Add(Tuple.Create(ico, size));
            Entry = new AnalysisEntry(LocalValues[0]);
        }

        public void Output()
        {
            Globals.Context.UIProvider.VerboseLog("[Analyze(Icon)] -> Extracted icon");

        }
    }
}
