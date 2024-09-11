using System;
using System.Diagnostics;
using System.IO;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using static log4net.Appender.RollingFileAppender;

namespace SiemensToBRIOIntegration
{
    internal class Location
    {
        public string path { get; set; } = null;
    }
    internal class ValuesProcessor
    {
        private string _lastLifeBit;
        private WebCamWrapper _camera;
        private bool _initialized;
        private string _path;

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
                    this._camera = new WebCamWrapper(this._path);
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
            var defaultPath = "C:\\Pictures";
            this._path = defaultPath;
            var filepath = "config.yaml";
            if (System.IO.File.Exists(filepath)){
                using (var reader = new StreamReader(filepath))
                {
                    // Load the stream
                    IDeserializer deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    var loc = deserializer.Deserialize<Location>(reader);
                    if (System.IO.Directory.Exists(loc.path))
                    {
                        this._path = loc.path;
                    }
                    else
                    {
                        Console.WriteLine($"Can't find given directory '{loc.path}' using default '{this._path}'");
                    }
                }
            } else
            {
                Console.WriteLine($"No config file using default path '{this._path}'");
            }
        }
    }
}