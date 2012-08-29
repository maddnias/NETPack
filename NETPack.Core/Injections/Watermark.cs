using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETPack.Core.Injections
{
    public class Watermark : Attribute
    {
        public Version @Version;

        public Watermark()
        {
            @Version = new Version(0, 1, 0, 0); 
        }
    }
}
