using System.Threading;

namespace SiemensToBRIOIntegration
{
    class Program
    {
        static ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            var ip = "192.168.0.1"; //ConfigurationManager.AppSettings["ip"];
            var sw = new SiemensWrapper(ip, 0, 1); // 192.168.0.1
            new Core(sw, ip).Run();
            manualResetEvent.WaitOne();
        }
    }
}
