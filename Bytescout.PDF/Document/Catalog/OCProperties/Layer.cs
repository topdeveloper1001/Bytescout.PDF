using System;
using System.Runtime.InteropServices;
using Bytescout.PDF;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Layer
#else
	/// <summary>
    /// Represents a layer in a PDF document.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Layer
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
	    /// Gets or sets the layer name.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string Name
	    {
		    get
		    {
			    PDFString name = _dictionary["Name"] as PDFString;
			    if (name == null)
				    return "";
			    return name.GetValue();
		    }
		    set
		    {
			    if (string.IsNullOrEmpty(value))
				    _dictionary.AddItem("Name", new PDFString(""));
			    else
				    _dictionary.AddItem("Name", new PDFString(value));
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Layer.
        /// </summary>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The layer name.</param>
        public Layer(string name)
        {
            if (name == null)
                throw new NullReferenceException();

            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Name", new PDFString(name));
            _dictionary.AddItem("Type", new PDFName("OCG"));
            _dictionary.Tag = this;
        }

        private Layer(PDFDictionary dict)
        {
            _dictionary = dict;
            _dictionary.Tag = this;
        }

        internal static Layer Instance(PDFDictionary dict)
        {
            if (dict.Tag is Layer)
                return dict.Tag as Layer;

            Layer layer = new Layer(dict);
            return layer;
        }

	    /// <summary>
        /// Returns a System.String that represents the current Bytescout.PDF.Layer.
        /// </summary>
        /// <returns cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A System.String that represents the current Bytescout.PDF.Layer.</returns>
        public override string ToString()
        {
            return Name;
        }

        internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }
    }
}
