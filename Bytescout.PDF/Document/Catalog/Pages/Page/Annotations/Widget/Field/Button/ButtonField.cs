using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class ButtonField: Field
#else
	/// <summary>
    /// Represents an abstract class for button fields.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class ButtonField: Field
#endif
	{
	    internal bool NoToggleToOff
        {
            get { return getFlagAttribute(15); }
            set { setFlagAttribute(15, value); }
        }

        internal bool Radio
        {
            get { return getFlagAttribute(16); }
            set { setFlagAttribute(16, value); }
        }

        internal bool Pushbutton
        {
            get { return getFlagAttribute(17); }
            set { setFlagAttribute(17, value); }
        }

        internal bool RadiosInUnison
        {
            get { return getFlagAttribute(26); }
            set { setFlagAttribute(26, value); }
        }

	    internal ButtonField(float left, float top, float width, float height, string name, IDocumentEssential owner)
		    : base(left, top, width, height, name, owner)
	    {
		    Dictionary.AddItem("FT", new PDFName("Btn"));
	    }

	    internal ButtonField(PDFDictionary dict, IDocumentEssential owner)
		    : base(dict, owner) { }

	    private bool getFlagAttribute(byte bytePosition)
        {
            return (Ff >> bytePosition - 1) % 2 != 0;
        }

        private void setFlagAttribute(byte bytePosition, bool value)
        {
            if (value)
                Ff = Ff | (uint)(1 << (bytePosition - 1));
            else
                Ff = Ff & (0xFFFFFFFF ^ (uint)(1 << (bytePosition - 1)));
        }
    }
}
