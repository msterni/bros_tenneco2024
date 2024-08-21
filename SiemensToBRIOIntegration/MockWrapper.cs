using log4net;
using Sharp7;
using System;
using System.Threading.Tasks;

namespace SiemensToBRIOIntegration
{
    public class MockWrapper : ISiemensWrapper
    {
        public S7Client Client { get; set; }
        public ulong Reads { get; set; }
        public ulong Writes { get; set; }
        public event SiemensEventDelegate DataReceived;

        private static readonly ILog _log = LogManager.GetLogger(nameof(MockWrapper));
        private static readonly object SyncObject = new object();

        private int _name = 0;

        public MockWrapper()
        {
            Reads = 0;
            Writes = 0;

            var tHold = Task.Run(() =>
            {

                while (true)
                {

                    var keyInfo = Console.ReadKey(true);


                    if (keyInfo.Key == ConsoleKey.N)
                    {
                        _name++;
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.T)
                    {
                        _log.Info("Reading mock data with trigger");
                        try
                        {
                            var trigger = true;
                            var trayName = _name.ToString();
                            DataReceived?.Invoke(trigger, trayName);
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            _log.Debug(ex.InnerException);
                            _log.Debug(ex.Data);
                        }
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.R)
                    {
                        _log.Info("Reading mock data");
                        try
                        {
                            var trigger = false;
                            var trayName = _name.ToString();
                            DataReceived?.Invoke(trigger, trayName);
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            _log.Debug(ex.InnerException);
                            _log.Debug(ex.Data);
                        }
                        continue;
                    }
                }

            });


            //_timer = new System.Timers.Timer()
            //{
            //    AutoReset = false,
            //    Interval = 10,
            //    Enabled = true,
            //};

            //_timer.Elapsed += (x, y) =>
            //{
            //    try
            //    {
            //        if (!Alive)
            //            return;

            //        if (Client == null || Client.Connected == false)
            //        {
            //            Reconnect();
            //        }
            //        Monitor.Enter(SyncObject);
            //        (bool trigger, string trayName) = Read();
            //        Monitor.Exit(SyncObject);
            //        DataReceived?.Invoke(trigger, trayName);
            //    }
            //    catch (Exception ex)
            //    {
            //        _log.Error(ex.Message);
            //        _log.Debug(ex.InnerException);
            //        _log.Debug(ex.Data);
            //    }
            //    finally
            //    {
            //        _timer.Start();
            //    }
            //};
            //_timer.Start();
        }

        public void Dispose()
        {

        }

        public void Reconnect()
        {

        }

        public void WriteLiveBit(bool liveBit, bool error)
        {
            _log.Info("WriteLiveBit " + liveBit + error);
        }

        public void WritePhotoTaken(bool photoTaken)
        {
            _log.Info("WritePhotoTaken " + photoTaken);
        }

    }
}
