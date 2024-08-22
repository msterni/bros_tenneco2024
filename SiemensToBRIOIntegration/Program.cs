using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiemensToBRIOIntegration
{
    class Program
    {
        static ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            var task1 = Task.Factory.StartNew(() => { MainThread(); });
            Task.WaitAll(new[] { task1 });

        }
        private static void MainThread()
        {
            var counter = 0;
            var plc = new PlcClient();
            while (true)
            {
                Thread.Sleep(100);
                plc.Run();
                counter++;
            }
        }
    }
}
