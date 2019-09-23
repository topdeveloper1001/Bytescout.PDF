using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class OptionalContents
#else
	/// <summary>
    /// Represents the document's optional content properties.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class OptionalContents
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private LayerCollection _layers;
	    private OptionalContentConfiguration _configuration;

	    /// <summary>
	    /// Gets the collections of layers in the document.
	    /// Every optional content group must be included in this collection.
	    /// </summary>
	    /// <value cref="LayerCollection"></value>
	    public LayerCollection Layers
	    {
		    get
		    {
			    if (_layers == null)
				    loadLayers();
			    return _layers;
		    }
	    }

	    /// <summary>
	    /// Gets the optional content configuration or PDF processing applications or features.
	    /// </summary>
	    /// <value cref="OptionalContentConfiguration"></value>
	    public OptionalContentConfiguration Configuration
	    {
		    get
		    {
			    if (_configuration == null)
				    loadConfiguration();
			    return _configuration;
		    }
	    }

	    internal OptionalContents()
        {
            _dictionary = new PDFDictionary();
        }

        internal OptionalContents(PDFDictionary dict)
        {
            _dictionary = dict;
        }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        private void loadConfiguration()
        {
            PDFDictionary dict = _dictionary["D"] as PDFDictionary;
            if (dict == null)
            {
                dict = new PDFDictionary();
                _dictionary.AddItem("D", dict);
            }
            _configuration = new OptionalContentConfiguration(dict);
        }

        private void loadLayers()
        {
            PDFArray ocgs = _dictionary["OCGs"] as PDFArray;
            if (ocgs == null)
            {
                ocgs = new PDFArray();
                _dictionary.AddItem("OCGs", ocgs);
            }
            _layers = new LayerCollection(ocgs);
        }
    }
}
