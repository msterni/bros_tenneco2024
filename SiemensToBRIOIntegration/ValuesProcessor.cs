﻿using System;
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
        private WebCamWrapper _camera;
        private string _path;

        public ValuesProcessor()
        {
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
                    this._camera = null;
                }
            }
            return plcData;
        }
        public void Initialize()
        {
            this.ReadPath();
            if (this._camera != null && this._camera.StoragePath != this._path)
            {
                this._camera.StoragePath = this._path;
            }
            if (this._camera == null)
            {
                try
                {
                    this._camera = new WebCamWrapper(this._path);
                }
                catch (Exception)
                {
                    Console.WriteLine("Can't initialize the Camera");
                }
            }

        }
        private void IncorrectConfigContentError()
        {
            var example = "---\r\npath: C:\\\\Pictures";
            Console.WriteLine($"incorrect config.yaml content using default '{this._path}'. Example:\n\n{example}\n");
        }
        private void ReadPath()
        {
            var defaultPath = "C:\\Pictures";
            this._path = defaultPath;
            var filepath = "config.yaml";
            if (System.IO.File.Exists(filepath))
            {
                using (var reader = new StreamReader(filepath))
                {
                    // Load the stream
                    try
                    {
                        IDeserializer deserializer = new DeserializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                            .Build();
                        var loc = deserializer.Deserialize<Location>(reader);
                        if (loc == null)
                        {
                            IncorrectConfigContentError();
                            return;
                        }
                        if (loc != null && System.IO.Directory.Exists(loc.path))
                        {
                            this._path = loc.path;
                        }
                        else
                        {
                            IncorrectConfigContentError();
                        }
                    }
                    catch (YamlDotNet.Core.YamlException)
                    {
                        IncorrectConfigContentError();
                    }
                }
            } else
            {
                Console.WriteLine($"No config file 'config.yaml'. using default path '{this._path}'");
            }
        }
    }
}