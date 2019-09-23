namespace Bytescout.PDF
{
    internal class EndingStyles
    {
	    private readonly PDFArray _array;

	    public LineEndingStyle Start
	    {
		    get
		    {
			    return TypeConverter.PDFNameToPDFLineEndingStyle(_array[0] as PDFName);
		    }
		    set
		    {
			    if (_array.Count != 2)
			    {
				    setDefault();
			    }
			    _array.RemoveItem(0);
			    _array.Insert(0, TypeConverter.PDFLineEndingStyleToPDFName(value));
		    }
	    }

	    public LineEndingStyle End
	    {
		    get
		    {
			    return TypeConverter.PDFNameToPDFLineEndingStyle(_array[1] as PDFName);
		    }
		    set
		    {
			    if (_array.Count != 2)
			    {
				    setDefault();
			    }
			    _array.RemoveItem(1);
			    _array.AddItem(TypeConverter.PDFLineEndingStyleToPDFName(value));
		    }
	    }

	    internal PDFArray Array
	    {
		    get
		    {
			    return _array;
		    }
	    }

	    public EndingStyles()
        {
            _array = new PDFArray();
            _array.AddItem(new PDFName(LineEndingStyle.None.ToString()));
            _array.AddItem(new PDFName(LineEndingStyle.None.ToString()));
        }

        public EndingStyles(LineEndingStyle begin, LineEndingStyle end)
        {
            _array = new PDFArray();
            _array.AddItem(new PDFName(begin.ToString()));
            _array.AddItem(new PDFName(end.ToString()));
        }

        internal EndingStyles(PDFArray array)
        {
            _array = array;
        }

	    private void setDefault()
        {
            _array.Clear();
            _array.AddItem(new PDFName(LineEndingStyle.None.ToString()));
            _array.AddItem(new PDFName(LineEndingStyle.None.ToString()));
        }
    }
}
