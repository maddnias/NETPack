using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace NETPack.Core.Injections
{
    public class Loader
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var buff = new byte[0];

            using (var asmStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("X"))
            {
                buff = new byte[asmStream.Length];
                asmStream.Read(buff, 0, buff.Length);

                buff = Decompressor.D(buff);
            }
            var T = new Thread(run);
            T.SetApartmentState(ApartmentState.STA);
            T.Start(new[] {(object)buff, args});
        }

        public static void run(object param)
        {
            var data = ((object[])param)[0] as byte[];
            var args = ((object[])param)[1] as string[];

            var asm = Assembly.Load(data);
            asm.EntryPoint.Invoke(null, new object[] { args });
        }
    }
}
