using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class OptionalContentGroupLayer: OptionalContentGroupItem
#else
	/// <summary>
    /// Represents a layer for optional content group.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class OptionalContentGroupLayer: OptionalContentGroupItem
#endif
	{
	    private readonly Layer _layer;

	    /// <summary>
		/// Gets the layer of this Bytescout.PDF.OptionalContentGroupLayer.
        /// </summary>
        public Layer Layer { get { return _layer; } }

        /// <summary>
		/// Gets the Bytescout.PDF.OptionalContentGroupItemType value that specifies the type of this item.
        /// </summary>
        /// <value cref="OptionalContentGroupItemType"></value>
        public override OptionalContentGroupItemType Type
        {
            get { return OptionalContentGroupItemType.Layer; }
        }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.OptionalContentGroupLayer class.
	    /// </summary>
	    /// <param name="layer">The layer.</param>
	    public OptionalContentGroupLayer(Layer layer)
	    {
		    _layer = layer;
	    }

	    internal override IPDFObject GetObject()
        {
            return _layer.GetDictionary();
        }
    }
}
