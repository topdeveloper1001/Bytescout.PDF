namespace Bytescout.PDF
{
    internal class PDFGroup
    {
	    private readonly PDFDictionary _dictionary;

	    //TODO Colorspace
        internal IPDFObject ColorSpace
        {
            get { return _dictionary["CS"]; }
            set { _dictionary.AddItem("CS", value); }
        }

        internal bool Isolated
        {
            get
            {
                PDFBoolean i = _dictionary["I"] as PDFBoolean;
                if (i == null)
                    return false;
                return i.GetValue();
            }
            set { _dictionary.AddItem("I", new PDFBoolean(value)); }
        }

        internal bool Knockout
        {
            get
            {
                PDFBoolean k = _dictionary["K"] as PDFBoolean;
                if (k == null)
                    return false;
                return k.GetValue(); 
            }
            set { _dictionary.AddItem("K", new PDFBoolean(value)); }
        }

	    public PDFGroup()
	    {
		    _dictionary = new PDFDictionary();
		    _dictionary.AddItem("S", new PDFName("Transparency"));
		    _dictionary.AddItem("CS", new PDFName("DeviceRGB"));
	    }

	    public PDFGroup(PDFDictionary dictionary)
	    {
		    _dictionary = dictionary;
	    }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }
    }
}
