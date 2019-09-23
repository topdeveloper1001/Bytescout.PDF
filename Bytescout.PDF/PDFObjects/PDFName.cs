using System.IO;

namespace Bytescout.PDF
{
    internal class PDFName : IPDFObject
    {
	    private readonly string _name;

	    public int ObjNo
        {
            get { return -1; }
            set { }
        }

	    public PDFName(string name)
	    {
		    _name = name;
	    }

	    public string GetValue()
        {
            return _name;
        }

        public void Write(SaveParameters param)
        {
            Write(param.Stream, _name);
        }

        public IPDFObject Clone()
        {
            return new PDFName(_name);
        }

        public override string ToString()
        {
            return _name;
        }

        public void Collect(XRef xref)
        {
        }

        public static void Write(Stream stream, string name)
        {
            stream.WriteByte((byte)'/');
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(name);
            int len = buf.Length;
            for (int i = 0; i < len; ++i)
            {
                if (buf[i] < 33 || buf[i] == '#' || Lexer.IsSpecialCharacter(buf[i]))
                {
                    stream.WriteByte((byte)'#');
                    byte hi = (byte)(buf[i] / 16);
                    byte lo = (byte)(buf[i] - hi * 16);

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
                else
                {
                    stream.WriteByte(buf[i]);
                }
            }
        }
    }
}
