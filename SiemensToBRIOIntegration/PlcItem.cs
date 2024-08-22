using System.Runtime.Serialization.Formatters.Binary;
using S7.Net;
using S7.Net.Types;

namespace siemens.Types
{
    public class PlcItem
    {
        private string _name;
        private string _address;
        private Plc _plc;
        private object _value;
        public string Name
        {
            get
            {
                return _name;
            }
        }
        public PlcItem(Plc plc, string name, string directAddress)
        {
            _plc = plc;
            _name = name;
            _address = directAddress;
        }
        public void Read()
        {
            if (_name == "Barcode")
            {
                _value = _plc.Read(DataType.DataBlock, 2, 2, VarType.String, 16).ToString().Replace("\0", "");
            }
            else
            {   
                _value = _plc.Read(_address).ToString();
            }
        }
        public void Write()
        {
            var bools = new string[] { "True", "False" };
            if (bools.Contains(_value))
            {
                int val = 0;
                if ((string)_value == "True") val = 1;
                _plc.Write(_address, val);
            }
            else
            {
                var x = DataItem.FromAddress(_address);
                if (x.VarType == VarType.Word)
                    x.Value = UInt16.Parse(_value.ToString());
                if (x.VarType == VarType.DWord)
                    x.Value = UInt32.Parse(_value.ToString());
                if (x.VarType == VarType.Bit)
                    x.Value = _value;
                _plc.Write(new DataItem[] { x });
            }
        }
        public object Value
        { 
            get 
            {
                return _value; 
            }
            set
            {
                _value = value;
                if (_name == "Barcode") _writeBarcode();
                else { Write(); }
            } 
        }
        // the only string is Barcode
        private void _writeBarcode()
        {
            //byte 0 is max length
            //byte 1 is actual length
            byte[] values = ToByteArray(_value.ToString());
            values = values.Where((source, index) => index != 0).ToArray();
            values = values.Where((source, index) => index != 1).ToArray();
            _plc.WriteBytes(DataType.DataBlock, 2, 2, values);
        }
        private static byte[] ToByteArray(string value)
        {
            string txt = value;
            char[] ca = txt.ToCharArray();
            byte[] bytes = new byte[2 + txt.Length];
            bytes[0] = bytes[1] = (byte)value.Length;
            for (int cnt = 0; cnt <= ca.Length - 1; cnt++)
                bytes[cnt + 2] = _asc(ca[cnt].ToString());
            return bytes;
        }
        private static byte _asc(string v)
        {
            byte[] b = System.Text.Encoding.ASCII.GetBytes(v);
            if (b.Length > 0) return b[0];
            return 0;
        }
    }
}