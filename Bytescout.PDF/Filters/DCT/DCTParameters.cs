namespace Bytescout.PDF
{
    internal struct DCTParameters
    {
	    private readonly int _colorTransform;

	    public int ColorTransform { get { return _colorTransform; } }

	    public DCTParameters(PDFDictionary dict)
	    {
		    _colorTransform = getColorTransform(dict);
	    }

	    private static int getColorTransform(PDFDictionary dict)
        {
            if (dict == null)
                return 1;

            PDFNumber colorTransform = dict["ColorTransform"] as PDFNumber;
            if (colorTransform == null)
                return 1;

            if (colorTransform.GetValue() == 0)
                return 0;
            return 1;
        }
    }
}
