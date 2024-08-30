using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;
using Sharp7;

namespace SiemensToBRIOIntegration
{
    public struct PlcData
    {
        public bool LifeBitFromCamera;
        public ushort CameraStatus;
        public bool LifeBitToCamera;
        public bool CameraTrigger;
        public double FileName;
    }

    internal class PlcConnector
    {
        private string _ip;
        private int _dbNo;
        private S7Client _client;
        private bool _connected;

        public PlcConnector(string ip, int dbNo)
        {
            this._ip = ip;
            this._dbNo = dbNo;
            this._client = new S7Client();
        }

        public void Connect()
        {
            if (_connected) { return; }
            var connection = _client.ConnectTo(_ip, 0, 1);
            if (connection != 0)
            {
                Console.WriteLine("connection failed");
                _connected = false;
                throw new ConnectionFailedException("Can't connect to PLC");
            }
            else {
                _connected = true;
                Console.WriteLine("connection ok"); }
        }

        public PlcData ReadValues()
        {
            Connect();
            var readbuffer = new byte[14];
            int readResult = _client.DBRead(_dbNo, 0, readbuffer.Length, readbuffer);
            var data = new PlcData();
            data.LifeBitFromCamera = S7.GetBitAt(readbuffer, 0, 0);
            data.CameraStatus = S7.GetWordAt(readbuffer, 2);
            data.LifeBitToCamera = S7.GetBitAt(readbuffer, 4, 0);
            data.CameraTrigger = S7.GetBitAt(readbuffer, 4, 1);
            data.FileName = S7.GetLRealAt(readbuffer, 6);
            if (readResult !=0)
                Console.WriteLine($"Can't read from PLC: read result = {readResult}");
            return data;
        }
        public bool WriteValues(PlcData data)
        {
            Connect();
            var writebuffer = new byte[14];
            S7.SetBitAt(ref writebuffer, 0, 0, data.LifeBitFromCamera);
            S7.SetWordAt(writebuffer, 2, data.CameraStatus);
            S7.SetBitAt(ref writebuffer, 4, 0, data.LifeBitToCamera);
            S7.SetBitAt(ref writebuffer, 4, 1, data.CameraTrigger);
            S7.SetLRealAt(writebuffer, 6, data.FileName);
            var writeresult = _client.DBWrite(_dbNo, 0, writebuffer.Length, writebuffer);
            if (writeresult != 0) Console.WriteLine($"Can't write to PLC: write result = {writeresult}");
            return writeresult == 0;
        }
    }
}
