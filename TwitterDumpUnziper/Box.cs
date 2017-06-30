using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace TwitterDumpUnziper
{
    class Box
    {
        public FileInfo[] files { get; set; }
        public AutoResetEvent auto { get; set; }
    }
}
