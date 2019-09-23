namespace Bytescout.PDF
{
    internal class PDF3DView
    {
	    private readonly PDFDictionary _dictionary;

	    internal PDFDictionary Dictionary
	    {
		    get
		    {
			    return _dictionary;
		    }
	    }

	    public PDF3DView()
        {
            _dictionary.AddItem("XN", new PDFString("Default"));
        }

        internal PDF3DView(PDFDictionary dict)
        {
            _dictionary = dict;
        }
    }
}
