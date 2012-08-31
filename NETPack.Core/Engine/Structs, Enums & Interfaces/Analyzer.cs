using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    public interface IAnalyzer
    {
        string AnalyzationKey { get; }
        AnalysisEntry Entry { get; set; }
        
        void Analyze(object param);
        void Output();
    }

    public abstract class Analyzer : IAnalyzer
    {
        public abstract string AnalyzationKey { get; }

        public abstract void Output();
        public List<dynamic> LocalValues { get; set; }

        public abstract AnalysisEntry Entry { get; set; }
        public abstract void Analyze(object param);
    }
}
