namespace Bytescout.PDF
{
    internal class PDFLink : IPDFObject
    {
	    private int _objNo;
	    private XRef _xref;

	    public int ObjNo
        {
            get { return -1; }
            set { }
        }

        public int ObjectNumber
        {
            get { return _objNo; }
        }

	    public PDFLink(XRef xref, int objNo)
	    {
		    _xref = xref;
		    _objNo = objNo;
	    }

	    public IPDFObject GetValue()
        {
            if (_xref != null)
                return _xref.GetObject(_objNo);
            return new PDFNull();
        }

        public IPDFObject Clone()
        {
            return MemberwiseClone() as IPDFObject;
        }

        public void Write(SaveParameters param)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(_objNo.ToString() + " 0 R");
            param.Stream.Write(bytes, 0, bytes.Length);
        }

        public override string ToString()
        {
            return _objNo.ToString() + " 0 R";
        }

        public void Collect(XRef xref)
        {
        }
    }
}
