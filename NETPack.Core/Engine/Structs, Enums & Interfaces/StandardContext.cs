using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mono.Cecil;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    public class StandardContext : PackerContext
    {
        public List<PackingStep> PackingSteps;
        public Dictionary<AssemblyDefinition, string> MarkedReferences;
        public bool MoveReferences = true;
        public int CompressionLevel = 3;
        public ApartmentState ApmtState = ApartmentState.STA;
        public bool VerifyOutput = true;

        public override bool VerifyContext()
        {
            return !(CompressionLevel != 1 && CompressionLevel != 3);
        }

        public StandardContext()
        {
            PackingSteps = new List<PackingStep>();
            MarkedReferences = new Dictionary<AssemblyDefinition, string>();
        }
    }
}
