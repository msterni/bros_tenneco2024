﻿using AForge;
using Sharp7;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace SiemensToBRIOIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            var task1 = Task.Factory.StartNew(() => { MainThread(); });
            Task.WaitAll(new[] { task1 });
        }
        private static void MainThread()
        {
            var ip = "192.168.0.1"; //ConfigurationManager.AppSettings["ip"];
            int dbNo = 57;
            var connector = new PlcConnector(ip, dbNo);
            var processor = new ValuesProcessor();
            while (true)
            {
                try
                {
                    PlcData values = connector.ReadValues();
                    Console.WriteLine($"{values.LifeBitFromCamera}-{values.CameraStatus}-{values.LifeBitToCamera}-{values.CameraTrigger}-{values.FileName}");
                    values = processor.ProcessValues(values);
                    connector.WriteValues(values);
                    Thread.Sleep(1000);
                } catch (ConnectionFailedException)
                {
                    Console.WriteLine("Can't connect to plc");
                    Thread.Sleep(10000);
                }
            }
        }
    }
}
