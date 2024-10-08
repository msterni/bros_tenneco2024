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
                    PrintState(values);
                    values = processor.ProcessValues(values);
                    connector.WriteValues(values);
                    Thread.Sleep(1000);
                } catch (ConnectionFailedException)
                {
                    Console.WriteLine("Can't connect to plc");
                    Thread.Sleep(5000);
                } catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
        }
        private static void PrintState(PlcData values)
        {
            Console.WriteLine($"CamLife:{values.LifeBitFromCamera}\tCamStatus:{values.CameraStatus}\tLifeBitToCam:{values.LifeBitToCamera}\tCamTrig{values.CameraTrigger}\tFilename:{values.FileName}");
        }
    }
}
