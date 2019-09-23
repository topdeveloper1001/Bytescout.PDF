using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class OptionalContentConfiguration
#else
	/// <summary>
    /// Represents an optional content configuration for PDF processing of applications or features.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class OptionalContentConfiguration
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private LayerCollection _on;
	    private LayerCollection _off;
	    private OptionalContentGroup _order;

	    /// <summary>
        /// Gets or sets the name for the configuration; suitable for presentation in a user interface.
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
                    _dictionary.RemoveItem("Name");
                else
                    _dictionary.AddItem("Name", new PDFString(value));
            }
        }

        /// <summary>
        /// Gets or sets the name of the application or feature that created this configuration.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Creator
        {
            get
            {
                PDFString creator = _dictionary["Creator"] as PDFString;
                if (creator == null)
                    return "";
                return creator.GetValue();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _dictionary.RemoveItem("Creator");
                else
                    _dictionary.AddItem("Creator", new PDFString(value));
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates the status of all the optional content groups in a document.
        /// </summary>
        /// <value cref="OptionalContentState"></value>
        public OptionalContentState BaseState
        {
            get { return TypeConverter.PDFNameToPDFOptionalContentState(_dictionary["BaseState"] as PDFName); }
            set { _dictionary.AddItem("BaseState", TypeConverter.PDFOptionalContentStateToPDFName(value)); }
        }

        /// <summary>
        /// Gets the collection of optional content groups whose state should be set to ON when this configuration is applied.
        /// </summary>
        /// <value cref="LayerCollection"></value>
        public LayerCollection ON
        {
            get
            {
                if (_on == null)
                    loadON();
                return _on;
            }
        }

        /// <summary>
        /// Gets the collection of optional content groups whose state should be set to OFF when this configuration is applied.
        /// </summary>
        /// <value cref="LayerCollection"></value>
        public LayerCollection OFF
        {
            get
            {
                if (_off == null)
                    loadOFF();
                return _off;
            }
        }

        /// <summary>
        /// Gets the Bytescout.PDF.OptionalContentGroup specifying the recommended order for presentation of optional content groups in a user interface.
        /// </summary>
        /// <value cref="OptionalContentGroup"></value>
        public OptionalContentGroup Order
        {
            get
            {
                if (_order == null)
                    loadOrder();
                return _order;
            }
        }

	    internal OptionalContentConfiguration()
	    {
	    }

	    internal OptionalContentConfiguration(PDFDictionary dict)
	    {
		    _dictionary = dict;
	    }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        private void loadON()
        {
            PDFArray on = _dictionary["ON"] as PDFArray;
            if (on == null)
            {
                on = new PDFArray();
                _dictionary.AddItem("ON", on);
            }
            _on = new LayerCollection(on);
        }

        private void loadOFF()
        {
            PDFArray off = _dictionary["OFF"] as PDFArray;
            if (off == null)
            {
                off = new PDFArray();
                _dictionary.AddItem("OFF", off);
            }
            _off = new LayerCollection(off);
        }

        private void loadOrder()
        {
            PDFArray order = _dictionary["Order"] as PDFArray;
            if (order == null)
            {
                order = new PDFArray();
                _dictionary.AddItem("Order", order);
            }
            _order = new OptionalContentGroup(order);
        }
    }
}
