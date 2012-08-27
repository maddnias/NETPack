using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    public interface IAnalyzer
    {
        AnalysisEntry Entry { get; set; }
        void Analyze(object param);
    }
}
