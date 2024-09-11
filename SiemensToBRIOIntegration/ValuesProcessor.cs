using System;
using System.Diagnostics;
using static log4net.Appender.RollingFileAppender;

namespace SiemensToBRIOIntegration
{
    internal class ValuesProcessor
    {
        private string _lastLifeBit;
        private WebCamWrapper _camera;
        private bool _initialized;

        public ValuesProcessor()
        {
            this._initialized = false;
        }
        public PlcData ProcessValues(PlcData plcData)
        {
            this.Initialize();
            plcData.LifeBitFromCamera = !plcData.LifeBitFromCamera;
            if (plcData.CameraStatus == 2) plcData.CameraStatus = 1;
            if (plcData.CameraTrigger)
            {
                try
                {
                    this._camera.ShootPhoto(plcData.FileName.ToString());
                    plcData.CameraStatus = 2;
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                    plcData.CameraStatus = 4;
                }
            }
            return plcData;
        }
        public void Initialize()
        {
            this.ReadPath();
            if (!this._initialized) 
            {
                try
                {
                    this._camera = new WebCamWrapper("c://Pictures");
                    this._initialized = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Can't initialize the Camera");
                }
            }

        }
        private void ReadPath()
        {
            if (System.IO.File.Exists("config.json")){
                return;
            };
        }
    }
}