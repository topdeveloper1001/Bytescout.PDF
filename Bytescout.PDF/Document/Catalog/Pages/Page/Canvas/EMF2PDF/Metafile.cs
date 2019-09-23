namespace Bytescout.PDF
{
    internal class Metafile
    {
	    private readonly EMF2PDF _emf2pdf;

	    internal GraphicsTemplate GraphicsTemplate
	    {
		    get
		    {
			    return _emf2pdf.GraphicsTemplate;
		    }
	    }

	    public Metafile(string filename)
        {
            _emf2pdf = new EMF2PDF(filename);
        }
    }
}
