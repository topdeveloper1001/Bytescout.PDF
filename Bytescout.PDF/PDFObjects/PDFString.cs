using System.IO;

namespace Bytescout.PDF
{
    internal class PDFString : IPDFObject
    {
	    private readonly byte[] _string;
	    private readonly bool _hexadecimal;

	    public int ObjNo
	    {
		    get { return -1; }
		    set { }
	    }

	    public PDFString(byte[] str, bool hexadecimal)
        {
            _string = str;
            _hexadecimal = hexadecimal;
        }

        public PDFString(string str)
        {
            _string = new byte[str.Length * 2 + 2];
            _string[0] = 254;
            _string[1] = 255;
            System.Text.Encoding.BigEndianUnicode.GetBytes(str, 0, str.Length, _string, 2);
        }

	    public void Write(SaveParameters param)
        {
            byte[] buf = _string;
            int length = _string.Length;
            if (param.Encryptor != null)
            {
                param.Encryptor.ResetObjectReference(param.ObjNo, param.GenNo, DataType.String);
                MemoryStream ms = param.StringBuffer;
                ms.SetLength(0);
                param.Encryptor.Encrypt(_string, 0, _string.Length, ms, DataType.String);
                length = (int)ms.Length;
                buf = ms.GetBuffer();
            }
            
            if (_hexadecimal)
                writeHexString(param.Stream, buf, 0, length);
            else
                writeString(param.Stream, buf, 0, length);
        }

        public IPDFObject Clone()
        {
            return new PDFString(_string, _hexadecimal);
        }

        public byte[] GetBytes()
        {
            return _string;
        }

        public string GetValue()
        {
            if (_string.Length >= 2 && _string[0] == 254 && _string[1] == 255)
                return System.Text.Encoding.BigEndianUnicode.GetString(_string, 2, _string.Length - 2);
            else
                return Encoding.GetString(_string);
        }

        public override string ToString()
        {
            return GetValue();
        }

        public void Collect(XRef xref)
        {
        }

        private void writeString(Stream stream, byte[] buf, int offset, int length)
        {
            stream.WriteByte((byte)'(');
            for (int i = offset; i < length; ++i)
            {
                if (buf[i] == '(' || buf[i] == ')' || buf[i] == '\\' || buf[i] == '\r' || buf[i] == '\n')
                    stream.WriteByte((byte)'\\');

                if (buf[i] == '\r')
                    stream.WriteByte((byte)'r');
                else if (buf[i] == '\n')
                    stream.WriteByte((byte)'n');
                else
                    stream.WriteByte(buf[i]);
            }
            stream.WriteByte((byte)')');
        }

        private void writeHexString(Stream stream, byte[] buf, int offset, int length)
        {
            stream.WriteByte((byte)'<');
            byte hi, lo;
            for (int i = offset; i < length; ++i)
            {
                hi = (byte)(buf[i] / 16);
                lo = (byte)(buf[i] - hi * 16);

                if (hi >= 10)
                    hi = (byte)('A' - 10 + hi);
                else
                    hi += (byte)'0';
                if (lo >= 10)
                    lo = (byte)('A' - 10 + lo);
                else
                    lo += (byte)'0';

                stream.WriteByte(hi);
                stream.WriteByte(lo);
            }
            stream.WriteByte((byte)'>');
        }
    }
}
