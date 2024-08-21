using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using SiemensToBRIOIntegration;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            new Core(new MockWrapper(), "").Run();
        }

        private static readonly ILog _log = LogManager.GetLogger(nameof(Core));
    }
}
