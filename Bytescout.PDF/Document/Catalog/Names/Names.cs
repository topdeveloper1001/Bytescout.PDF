namespace Bytescout.PDF
{
    internal class Names
    {
	    private readonly PDFDictionary _dictionary;
	    private NameTree _dests;

	    internal NameTree Destinations
	    {
		    get
		    {
			    if (_dests == null)
				    loadDestinations();
			    return _dests;
		    }
	    }

	    internal Names()
        {
            _dictionary = new PDFDictionary();
        }

        internal Names(PDFDictionary dict)
        {
            _dictionary = dict;
        }

	    private void loadDestinations()
	    {
		    PDFDictionary dictionary = _dictionary["Dests"] as PDFDictionary;
		    _dests = dictionary == null ? new NameTree() : new NameTree(dictionary);
	    }
    }
}
