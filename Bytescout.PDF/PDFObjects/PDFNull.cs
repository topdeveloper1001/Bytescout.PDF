using System.IO;

namespace Bytescout.PDF
{
    internal class PDFNull : IPDFObject
    {
	    public int ObjNo
        {
            get { return -1; }
            set { }
        }

        public void Write(SaveParameters param)
        {
            Stream stream = param.Stream;
            stream.WriteByte((byte)'n');
            stream.WriteByte((byte)'u');
            stream.WriteByte((byte)'l');
            stream.WriteByte((byte)'l');
        }

        public override string ToString()
        {
            return "null";
        }

        public IPDFObject Clone()
        {
            return new PDFNull();
        }

        public void Collect(XRef xref)
        {
        }
    }
}
