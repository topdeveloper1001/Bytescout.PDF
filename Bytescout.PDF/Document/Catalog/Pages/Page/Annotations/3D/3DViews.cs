namespace Bytescout.PDF
{
    internal class PDF3DViews
    {
	    private readonly PDFArray _views;

	    internal PDFArray Array
	    {
		    get
		    {
			    return _views;
		    }
	    }

	    internal PDF3DViews()
        {
            _views = new PDFArray();
        }

        internal PDF3DViews(PDFArray array)
        {
            _views = array;
        }
    }
}
