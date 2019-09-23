using System.IO;

namespace Bytescout.PDF
{
    internal class PDFBoolean : IPDFObject
    {
	    private bool _value;

	    public int ObjNo
	    {
		    get { return -1; }
		    set { }
	    }

	    public PDFBoolean(bool value)
        {
            _value = value;
        }

	    public bool GetValue()
        {
            return _value;
        }

        public IPDFObject Clone()
        {
            return new PDFBoolean(_value);
        }

        public void Write(SaveParameters param)
        {
            Stream stream = param.Stream;
            if (_value)
            {
                stream.WriteByte((byte)'t');
                stream.WriteByte((byte)'r');
                stream.WriteByte((byte)'u');
                stream.WriteByte((byte)'e');
            }
            else
            {
                stream.WriteByte((byte)'f');
                stream.WriteByte((byte)'a');
                stream.WriteByte((byte)'l');
                stream.WriteByte((byte)'s');
                stream.WriteByte((byte)'e');
            }
            
        }
        
        public override string ToString()
        {
            if (_value)
                return "true";
            return "false";
        }

        public void Collect(XRef xref)
        {
        }
    }
}