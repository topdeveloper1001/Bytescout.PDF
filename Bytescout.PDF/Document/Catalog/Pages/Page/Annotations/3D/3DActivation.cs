namespace Bytescout.PDF
{
    internal class ThreeDActivation
    {
	    private readonly PDFDictionary _dictionary;

	    internal ThreeDActivation()
        {
            _dictionary = new PDFDictionary();
        }

        internal ThreeDActivation(PDFDictionary dict)
        {
            _dictionary = dict;
        }

        internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }
    }
}
