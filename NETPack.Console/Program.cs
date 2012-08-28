using System;
using System.Collections.Generic;
using System.Text;
using NETPack.Core;

namespace NETPack.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Packer(args[0]);
            p.PackFile();
        }
    }
}
