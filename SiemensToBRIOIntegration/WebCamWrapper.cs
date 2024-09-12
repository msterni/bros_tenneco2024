using AForge.Video;
using AForge.Video.DirectShow;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SiemensToBRIOIntegration
{
    //var ok = 0;
    //foreach(var v in videoSource.VideoCapabilities)
    //{
    //    Console.WriteLine(ok++ + " " + v.FrameSize + " | " + v.AverageFrameRate);
    //}
    //0 { Width = 640, Height = 480} | 60
    //1 { Width = 160, Height = 120} | 30
    //2 { Width = 176, Height = 144} | 30
    //3 { Width = 320, Height = 180} | 30
    //4 { Width = 320, Height = 240} | 30
    //5 { Width = 352, Height = 288} | 30
    //6 { Width = 340, Height = 340} | 30
    //7 { Width = 424, Height = 240} | 30
    //8 { Width = 440, Height = 440} | 30
    //9 { Width = 480, Height = 270} | 30
    //10 { Width = 640, Height = 360} | 30
    //11 { Width = 800, Height = 448} | 30
    //12 { Width = 800, Height = 600} | 30
    //13 { Width = 848, Height = 480} | 30
    //14 { Width = 960, Height = 540} | 30
    //15 { Width = 1024, Height = 576} | 30
    //16 { Width = 1280, Height = 720} | 60
    //17 { Width = 1600, Height = 896} | 30
    //18 { Width = 1920, Height = 1080} | 30
    //19 { Width = 2560, Height = 1440} | 30
    //20 { Width = 3840, Height = 2160} | 30
    //21 { Width = 4096, Height = 2160} | 30
    public class WebCamWrapper : IDisposable
    {
        public event Action PhotoShooted;
        private int uid = 0;
        bool writeToDisk = false;
        string lastTrayName = ""; // nie zrobi zapisu 2x dla takiej samej tacki
        VideoCaptureDevice videoSource;
        private string _storagePath;

        public void ShootPhoto(string trayName)
        {
            if (!writeToDisk) { return; }
            // below code prevents from taking picture with same trayName
            //if (!writeToDisk && lastTrayName == trayName)
            //{
            //    _log.Debug($"Tray name {trayName} already used - ignoring");
            //    return;
            //}
            _log.Info($"Write to disk set for {trayName}");
            lastTrayName = trayName;
            writeToDisk = true;
        }

        public string Path { get { return _storagePath; } set { this._storagePath = value; } }

        public WebCamWrapper(string location)
        {
            _storagePath = location;

            _queue = new ObservableCollection<BitmapFile>();
            _queue.CollectionChanged += (x, y) =>
            {
                if (y.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    var bmpFile = _queue.Last();
                    bmpFile.Bitmap.Save(bmpFile.FileName, ImageFormat.Jpeg);
                    _log.Info($"File {bmpFile.FileName} saved to disk");
                    bmpFile.Dispose();
                    _queue.Remove(bmpFile);
                }
                _log.Debug(_queue.Count);

            };

            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            try
            {
                var videoResolution = videoSource.VideoCapabilities.First();
                if (videoSource.VideoCapabilities.Count() > 18)
                {
                    videoResolution = videoSource.VideoCapabilities[18];
                }
                videoSource.VideoResolution = videoResolution;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            videoSource.Start();
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (writeToDisk)
            {
                var time = DateTime.Now.ToString($"MM-dd-yyyy-h-mm-tt-ss");
                if (!Directory.Exists(_storagePath)) { Directory.CreateDirectory(_storagePath); }
                _queue.Add(new BitmapFile
                {
                    Bitmap = new Bitmap(eventArgs.Frame),
                    FileName = Path.Combine(_storagePath, $"{time}-{lastTrayName}.jpg") //$"c://Pictures/{lastTrayName}_{uid++}.jpg"
                });
                writeToDisk = false;
                PhotoShooted?.Invoke();
            }
        }

        public void Dispose()
        {
            videoSource.SignalToStop();
        }

        private ObservableCollection<BitmapFile> _queue;
        private static readonly ILog _log = LogManager.GetLogger(nameof(WebCamWrapper));
    }
}
