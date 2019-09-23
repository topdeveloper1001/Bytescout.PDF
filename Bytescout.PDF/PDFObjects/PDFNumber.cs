namespace Bytescout.PDF
{
    internal class PDFNumber : IPDFObject
    {
	    private readonly double _number;

	    public int ObjNo
        {
            get { return -1; }
            set { }
        }

	    public PDFNumber(double number)
	    {
		    _number = number;
	    }

	    public double GetValue()
        {
            return _number;
        }

        public void Write(SaveParameters param)
        {
            StringUtility.WriteToStream(_number, param.Stream);
        }

        public override string ToString()
        {
            return _number.ToString();
        }

        public IPDFObject Clone()
        {
            return new PDFNumber(_number);
        }

        public void Collect(XRef xref)
        {
        }
    }
}
