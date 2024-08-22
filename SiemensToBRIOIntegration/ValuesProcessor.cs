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
            if (plcData.CameraTrigger && plcData.CameraStatus == 1)
            {
                _camera.ShootPhoto(DateTime.Now.ToString($"MM-dd-yyyy-h-mm-tt-{plcData.FileName.ToString()}"));
                plcData.CameraStatus = 2;
            }
            return plcData;
        }
    }
}