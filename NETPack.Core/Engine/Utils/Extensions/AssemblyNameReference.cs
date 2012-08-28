using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NETPack.Core.Engine.Utils.Extensions
{
    public static class AssemblyNameReferenceExt
    {
        public static AssemblyDefinition ResolveReference(this AssemblyNameReference @ref)
        {
            var fixedPath = Path.Combine(Path.GetDirectoryName(PackerContext.InPath), @ref.Name + ".dll");
            return AssemblyDefinition.ReadAssembly(fixedPath);
        }

        public static AssemblyDefinition ResolveReference(this AssemblyNameReference @ref, out string path)
        {
            path = Path.Combine(Path.GetDirectoryName(PackerContext.InPath), @ref.Name + ".dll");
            return AssemblyDefinition.ReadAssembly(path);
        }
    }
}
