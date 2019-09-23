using System;

namespace Bytescout.PDF
{
    internal delegate void ChangeGroupEventHandler (object sender, ChangeGroupEventArgs e);
    
    internal class ChangeGroupEventArgs : EventArgs
    {
	    readonly IPDFObject _colorspace;

	    public IPDFObject Colorspace
	    {
		    get
		    {
			    return _colorspace;
		    }
	    }

	    public ChangeGroupEventArgs(IPDFObject colorspace)
        {
            _colorspace = colorspace;
        }
    }
}
