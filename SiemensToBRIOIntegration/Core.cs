using log4net;
using System;

namespace SiemensToBRIOIntegration
{
    public delegate void SiemensEventDelegate(bool trigger, string trayName);

    public class Core : IDisposable
    {
        private ISiemensWrapper _siemens;
        private WebCamWrapper _webcam;
        System.Timers.Timer _timer;
        System.Timers.Timer _countTimer;
        string _ip = string.Empty;
        bool liveBit = false;
        private static readonly ILog _log = LogManager.GetLogger(nameof(Core));

        public Core(ISiemensWrapper siemens, string ip)
        {
            _log.Info("---Started");
            _ip = ip;

            //_siemens = new SiemensWrapper(ip, 0, 1); // 192.168.0.1
            _siemens = siemens;
            _webcam = new WebCamWrapper();

            _timer = new System.Timers.Timer()
            {
                AutoReset = false,
                Enabled = true,
                Interval = 500,
            };
            _timer.Elapsed += (x, y) =>
            {
                try
                {
                    liveBit = !liveBit;
                    _siemens.WriteLiveBit(liveBit, false);
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                    _log.Error(ex.InnerException);
                    _log.Error(ex.Data);
                }
                finally
                {
                    _timer.Start();
                }
            };
            _timer.Start();

            _countTimer = new System.Timers.Timer()
            {
                AutoReset = false,
                Interval = 5000,
                Enabled = true
            };
            _countTimer.Elapsed += (i, j) =>
            {
                try
                {
                    if (_siemens != null && _siemens.Client != null && _siemens.Client.Connected == false)
                    {
                        // it's not mockable
                        _siemens = new SiemensWrapper(_ip, 0, 1) as ISiemensWrapper;
                    }
                    _log.Info($"READS: {_siemens.Reads} WRITES: {_siemens.Writes}");
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                    _log.Error(ex.InnerException);
                    _log.Error(ex.Data);
                }
                finally
                {
                    _countTimer.Start();
                }
            };

        }

        public void Run()
        {
            _siemens.DataReceived += (trigger, trayName) =>
            {
                try
                {
                    if (trigger)
                    {
                        _log.Info($"Trigger dla: {trayName}");
                        _webcam.ShootPhoto(trayName);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                    _log.Error(ex.InnerException);
                    _log.Error(ex.Data);
                }
            };

            _webcam.PhotoShooted += () =>
            {
                _siemens.WritePhotoTaken(true);
            };

        }

        public void Dispose()
        {
         
        }
    }
}
