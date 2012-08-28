using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    public abstract class Analyzer : IAnalyzer
    {
        public List<dynamic> LocalValues = new List<dynamic>();

        public abstract string AnalyzationKey { get; }

        public abstract AnalysisEntry Entry { get; set; }
        public abstract void Analyze(object param);
        public abstract void Output();
    }
}
