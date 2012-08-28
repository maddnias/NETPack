using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    public class AnalysisEntry
    {
        public AnalysisEntry(params dynamic[] @params)
        {
            Values = new List<dynamic>();
            Values.AddRange(@params);
        }

        public List<dynamic> Values { get; set; }
    }
}
