using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Structs__Enums___Interfaces
{
    public interface IPackingStep
    {
        void Initialize();
        void ProcessStep();
        void FinalizeStep();
    }
}
