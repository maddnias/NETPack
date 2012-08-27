using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NETPack.Core.Engine.Utils.Extensions
{
    public static class AssemblyDefinitionExt
    {
        public static IMemberDefinition GetInjection(this AssemblyDefinition asmDef, string name)
        {
            IMemberDefinition target =
                asmDef.MainModule.GetAllTypes().First(
                    t => (t.Namespace == "NETPack.Core.Injections" && t.Name == name));

            switch (name)
            {
                case "Decompressor":
                    (target as TypeDefinition).Namespace = "N";
                    target.Name = "K";

                    return target;

                case "Loader":
                    (target as TypeDefinition).Namespace = "npack";
                    target.Name = "netpack";

                    return target ;

                case "Resolver":
                    (target as TypeDefinition).Namespace = "R";
                    target.Name = "RS";

                    return target;
                    break;
            }

            throw new Exception("Missing injection");
        }
    }
}
