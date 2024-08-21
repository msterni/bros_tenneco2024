using System;
using System.Drawing;

namespace SiemensToBRIOIntegration
{
    public class BitmapFile : IDisposable
    {
        public Bitmap Bitmap { get; set; }
        public string FileName { get; set; }

        public void Dispose()
        {
            Bitmap.Dispose();
            FileName = null;
        }
    }
}
