using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class OptionalContentGroupItem
#else
	/// <summary>
    /// Represents an abstract class for all optional content group items.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class OptionalContentGroupItem
#endif
    {
	    /// <summary>
	    /// Gets the Bytescout.PDF.OptionalContentGroupItemType value that specifies the type of this item.
	    /// </summary>
	    /// <value cref="OptionalContentGroupItemType" />
	    public abstract OptionalContentGroupItemType Type { get; }

        internal abstract IPDFObject GetObject();
    }
}
