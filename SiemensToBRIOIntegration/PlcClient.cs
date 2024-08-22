using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S7.Net;
using siemens.Types;

namespace SiemensToBRIOIntegration
{
    public struct FlagsPLC
    {
        public bool Lifebit_from_camera;
        public string Cam_status;
        public bool Lifebit_to_camera;
        public bool Cam_trig;
        public bool File_name;
    }

    internal class PlcClient
    {
        private string _ip;
        private bool _initialized;
        private Plc _plc;
        private readonly int _plcOffset;
        private readonly int _pcOffset;
        private readonly int _db;

        public PlcClient(string ip = "192.168.0.1")
        {
            this._ip = ip;
            this._plcOffset = 82;
            this._pcOffset = 84;
            this._db = 57;
        }
        public void Run()
        {
            this.Init();
            var x = this.ReadLifeBit();
            Console.WriteLine(x);
        }
        private FlagsPLC ReadLifeBit()
        {
            var result = new FlagsPLC();
            result.Lifebit_to_camera = ((FlagsPLC)_plc.ReadStruct(typeof(FlagsPLC), _db, _pcOffset)).Lifebit_to_camera;
            return result;
        }
        private void Init()
        {
            if (!_initialized)
            {
                _plc = new Plc(CpuType.S71200, _ip, 0, 1);
                try
                {
                    _plc.Open();
                    _initialized = true;
                }
                catch (PlcException e)
                {
                    Console.WriteLine($"Can't connect to PLC, IP: {_ip}");
                    Environment.Exit(0);
                }
            }
        }


    }
}
