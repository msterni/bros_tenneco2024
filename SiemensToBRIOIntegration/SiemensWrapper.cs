using System;
using System.Threading;
using log4net;
using Sharp7;

namespace SiemensToBRIOIntegration
{
    /// archiveInterfaceDB[DB5]
    /// toCam (struct) 0.0.
    /// -> triger (bool) 0.0
    /// -> trayName (string) 2.0 
    /// res (word) 258.0
    /// res1 (word) 260.0
    /// res2 (word) 262.0
    /// res3 (word) 264.0
    /// res4 (word) 266.0
    /// res5 (word) 268 .0
    /// fromCam (struct) 270.0
    /// liveBit (bool) 270.0
    /// photoTaken (bool) 270.1
    /// error (bool) 270.2
    ///      // http://snap7.sourceforge.net/sharp7.html
    // https://www.mesta-automation.com/how-to-write-a-siemens-s7-plc-driver-with-c-and-sharp7/
    // https://www.youtube.com/watch?v=T4OECx2MDV0

    // phototaken 274 
    // liveBit 270.0
    // error 270.1 
    public class SiemensWrapper : IDisposable, ISiemensWrapper
    {
        public ulong Reads { get; set; } //= 0;
        public ulong Writes { get; set; }  // = 0;

        public event SiemensEventDelegate DataReceived;
        private static readonly object SyncObject = new object();
        public S7Client Client { get; set; }
        string _address;
        int _rack;
        int _slot;
        System.Timers.Timer _timer;
        public bool Alive = true;

        public SiemensWrapper(string address, int rack, int slot)
        {
            Reads = 0;
            Writes = 0;
            _address = address;
            _rack = rack;
            _slot = slot;
            _timer = new System.Timers.Timer()
            {
                AutoReset = false,
                Interval = 10,
                Enabled = true,
            };

            _timer.Elapsed += (x, y) =>
            {
                try
                {
                    if (!Alive)
                        return;

                    if (Client == null || Client.Connected == false)
                    {
                        Reconnect();
                    }

                    if (Monitor.TryEnter(SyncObject, 3000))
                    {
                        try
                        {
                            (bool trigger, string trayName) = Read();
                            DataReceived?.Invoke(trigger, trayName);
                        }
                        finally
                        {
                            Monitor.Exit(SyncObject);
                        }
                    }
                    else
                    {
                        _log.Error("Monitor didn't receive lock in timer loop");
                    }
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
        }

        public void WriteLiveBit(bool liveBit, bool error)
        {
            var lifeBit = 0;
            try
            {
                if (Monitor.TryEnter(SyncObject, 3000))
                {
                    try
                    {
                        var writeBuffer = new byte[2];
                        S7.SetBitAt(ref writeBuffer, 0, 0, liveBit);
                        S7.SetBitAt(ref writeBuffer, 0, 1, error);
                        int writeResult = Client.DBWrite(57, lifeBit, writeBuffer.Length, writeBuffer);
                        Writes++;
                        if (writeResult != 0)
                        {
                            _log.Error("Write error: " + writeResult);
                            Client.Disconnect();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message);
                        _log.Error(ex.InnerException);
                        _log.Error(ex.Data);
                    }
                    finally
                    {
                        Monitor.Exit(SyncObject);
                    }
                }
                else
                {
                    _log.Error("Monitor didnt receive lock in WriteLiveBit");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                _log.Error(ex.InnerException);
                _log.Error(ex.Data);
            }
        }

        public void WritePhotoTaken(bool photoTaken)
        {
            try
            {
                if (Monitor.TryEnter(SyncObject, 3000))
                {
                    try
                    {
                        var writeBuffer = new byte[1];
                        S7.SetBitAt(ref writeBuffer, 0, 0, photoTaken);
                        int writeResult = Client.DBWrite(5, 274, writeBuffer.Length, writeBuffer);
                        Writes++;
                        if (writeResult != 0)
                        {
                            _log.Error("Write error: " + writeResult);
                            Client.Disconnect();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message);
                        _log.Debug(ex.InnerException);
                        _log.Debug(ex.Data);
                    }
                    finally
                    {
                        Monitor.Exit(SyncObject);
                    }
                }
                else
                {
                    _log.Warn("Monitor didnt receive lock in WritePhotoTaken - this may cause a permanent lock.");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                _log.Error(ex.InnerException);
                _log.Error(ex.Data);
            }
        }

        public void Reconnect()
        {
            try
            {
                if (Client == null || Client.Connected == false)
                {
                    Client = null;
                    Client = new S7Client();
                    int result = Client.ConnectTo(_address, _rack, _slot);
                    if (result != 0)
                    {
                        _log.Error("Connection error: " + Client.ErrorText(result));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                _log.Error(ex.InnerException);
                _log.Error(ex.Data);
            }
        }

        private (bool, string) Read()
        {
            var buffer = new byte[274];
            int readResult = Client.DBRead(5, 0, buffer.Length, buffer);
            if (readResult != 0)
            {
                _log.Error("Read error: " + Client.ErrorText(readResult));
                Client.Disconnect();
                return (false, String.Empty);
            }
            bool triger = S7.GetBitAt(buffer, 0, 0);
            string trayName = S7.GetStringAt(buffer, 2);
            Reads++;
            //Console.WriteLine("Odczyt triger: " + triger + " Odczyt trayName:" + trayName);
            return (triger, trayName);
        }


        public void Dispose()
        {
            Alive = false;
            Client.Disconnect();
            Client = null;
        }

        private static readonly ILog _log = LogManager.GetLogger(nameof(SiemensWrapper));

    }
}
