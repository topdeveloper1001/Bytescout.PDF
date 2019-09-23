using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ColorICC : Color
#else
	/// <summary>
    /// Represents class for an ICC color profile.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class ColorICC : Color
#endif
	{
	    private ICCBasedColorspace _colorspace;
	    private DeviceColor _color;

	    /// <summary>
	    /// Gets the colorspace associated with this Bytescout.PDF.ColorICC.
	    /// </summary>
	    /// <value cref="PDF.Colorspace"></value>
	    public override Colorspace Colorspace { get { return _colorspace; } }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.ColorICC class.
        /// </summary>
        /// <param name="icc">The ICC color profile for color space.</param>
        /// <param name="color">The Bytescout.PDF.DeviceColor to create Bytescout.PDF.ColorICC from.</param>
        public ColorICC(ICCBasedColorspace icc, DeviceColor color)
        {
            if (icc.N != color.Colorspace.N)
                throw new PDFNotEqualColorspacesException();
            _colorspace = icc;
            _color = color;
        }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.ColorICC.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.ColorICC.</returns>
        public override object Clone()
        {
            ColorICC p = this.MemberwiseClone() as ColorICC;
            p._colorspace = _colorspace.Clone() as ICCBasedColorspace;
            p._color = _color.Clone() as DeviceColor;
            return p;
        }

        /// <summary>
        /// Returns a System.String that represents the current Bytescout.PDF.ColorICC.
        /// </summary>
        /// <returns cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A System.String that represents the current Bytescout.PDF.ColorICC.</returns>
        public override string ToString()
        {
            return _color.ToString();
        }

        /// <summary>
        /// Determines whether the specified Bytescout.PDF.ColorICC is equal to the current Bytescout.PDF.ColorICC.
        /// </summary>
        /// <param name="obj" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">The System.Object to compare with the current Bytescout.PDF.ColorICC.</param>
        /// <returns cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">true if the specified System.Object is equal to the current Bytescout.PDF.ColorICC; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ColorICC))
                return false;
            return this.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">A hash code for the current Bytescout.PDF.ColorICC.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal override float[] ToArray()
        {
            return _color.ToArray();
        }
    }
}
