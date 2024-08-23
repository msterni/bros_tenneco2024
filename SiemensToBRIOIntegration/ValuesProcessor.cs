using System;
using System.Diagnostics;
using static log4net.Appender.RollingFileAppender;

namespace SiemensToBRIOIntegration
{
    internal class ValuesProcessor
    {
        private string _lastLifeBit;
        private WebCamWrapper _camera;

        public ValuesProcessor()
        {
            _camera = new WebCamWrapper("c://Pictures");
        }
        public PlcData ProcessValues(PlcData plcData)
        {
            plcData.LifeBitFromCamera = !plcData.LifeBitFromCamera;
            if (plcData.CameraTrigger)
            {
                try
                {
                    _camera.ShootPhoto(plcData.FileName.ToString());
                    plcData.CameraStatus = 2;
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                    plcData.CameraStatus = 4;
                }
            }
            return plcData;
        }
    }
}