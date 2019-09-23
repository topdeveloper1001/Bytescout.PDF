using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class OptionalContentGroupLabel: OptionalContentGroupItem
#else
	/// <summary>
    /// Represents a label for optional content group.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class OptionalContentGroupLabel: OptionalContentGroupItem
#endif
	{
	    private readonly PDFString _value;

	    /// <summary>
	    /// Gets the text of this label.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string Text { get { return _value.GetValue(); } }

	    /// <summary>
	    /// Gets the Bytescout.PDF.OptionalContentGroupItemType value that specifies the type of this item.
	    /// </summary>
	    /// <value cref="OptionalContentGroupItemType"></value>
	    public override OptionalContentGroupItemType Type
	    {
		    get { return OptionalContentGroupItemType.Label; }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.OptionalContentGroupLabel class.
        /// </summary>
        /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The text label.</param>
        public OptionalContentGroupLabel(string text)
        {
            _value = new PDFString(text);
        }

        internal OptionalContentGroupLabel(PDFString str)
        {
            _value = str;
        }

	    internal override IPDFObject GetObject()
        {
            return _value;
        }
    }
}
