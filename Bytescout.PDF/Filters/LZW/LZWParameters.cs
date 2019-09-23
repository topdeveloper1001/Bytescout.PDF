namespace Bytescout.PDF
{
    internal struct LZWParameters
    {
	    private int _earlyChange;
	    public int EarlyChange { get { return _earlyChange; } }

	    public LZWParameters(PDFDictionary dict)
        {
            _earlyChange = getEarlyChange(dict);
        }

	    private static int getEarlyChange(PDFDictionary dict)
        {
            if (dict == null)
                return 1;

            PDFNumber earlyChange = dict["EarlyChange"] as PDFNumber;
            if (earlyChange == null)
                return 1;

            if (earlyChange.GetValue() == 0)
                return 0;
            return 1;
        }
    }
}
