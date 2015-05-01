using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Utilities
    {
        public static Stream GetDataStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof (Utilities), name);
        }
    }
}
