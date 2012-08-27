using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace NETPack.Core.Injections
{
    public static class Resolver
    {
        public static void AddHandler()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;
        }

        public static Assembly ResolveHandler(object sender, ResolveEventArgs e)
        {
            byte[] asmBuff;
            var demangledName = DemangleName(e.Name.Split(',')[0]);

            using(var resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(demangledName))
            {
                asmBuff = new byte[resStream.Length];
                resStream.Read(asmBuff, 0, asmBuff.Length);

                asmBuff = Decompressor.D(asmBuff);
            }

            return Assembly.Load(asmBuff);
        }

        public static string DemangleName(string original)
        {
            char[] outStr = original.ToCharArray();

            for (var i = 0; i < outStr.Length; i++)
                outStr[i] = (char) (outStr[i] ^ 13);

            return new string(outStr);
        }
    }
}
