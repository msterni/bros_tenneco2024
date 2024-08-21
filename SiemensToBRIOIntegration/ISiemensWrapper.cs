using Sharp7;
using System;

namespace SiemensToBRIOIntegration
{
    public interface ISiemensWrapper : IDisposable
    {
        event SiemensEventDelegate DataReceived;
        S7Client Client { get; set; }
        ulong Reads { get; set; }
        ulong Writes { get; set; } 

        void WriteLiveBit(bool liveBit, bool error);
        void WritePhotoTaken(bool photoTaken);
        void Reconnect();

    }
}