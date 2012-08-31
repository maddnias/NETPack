using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mono.Cecil;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    public class StandardPackerContext : PackerContext
    {
        public StandardPackerContext(IUserInterfaceProvider uiProvider)
        {
            MarkedAssemblies = new List<string>();
            MarkedReferences = new Dictionary<AssemblyDefinition, string>();
            AnalysisDatabase = new Dictionary<string, AnalysisEntry>();
            UIProvider = uiProvider;
            PackingSteps = new List<IPackingStep>();
        }

        public override bool VerifyContext()
        {
            return true;
        }
    }
}
