using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ColorRGB : DeviceColor
#else
	/// <summary>
    /// Represents a color in an RGB color space.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class ColorRGB : DeviceColor
#endif
    {
	    private readonly byte _r;
	    private readonly byte _g;
	    private readonly byte _b;
	    private Colorspace _colorspace;

	    /// <summary>
        /// Gets the red component value.
        /// </summary>
        /// <value cref="byte" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx"></value>
        public byte R { get { return _r; } }

        /// <summary>
        /// Gets the green component value.
        /// </summary>
        /// <value cref="byte" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx"></value>
        public byte G { get { return _g; } }

        /// <summary>
        /// Gets the blue component value.
        /// </summary>
        /// <value cref="byte" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx"></value>
        public byte B { get { return _b; } }

	    /// <summary>
        /// Gets the colorspace associated with this Bytescout.PDF.ColorRGB.
        /// </summary>
        /// <value cref="PDF.Colorspace"></value>
        public override Colorspace Colorspace { get { return _colorspace; } }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.ColorRGB class.
	    /// </summary>
	    /// <param name="red" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx">The red component value.</param>
	    /// <param name="green" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx">The green component value.</param>
	    /// <param name="blue" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx">The blue component value.</param>
	    public ColorRGB(byte red, byte green, byte blue)
	    {
		    _r = red;
		    _g = green;
		    _b = blue;
		    _colorspace = new DeviceRGBColorspace();
	    }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.ColorRGB.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.ColorRGB.</returns>
        public override object Clone()
        {
            ColorRGB p = this.MemberwiseClone() as ColorRGB;
            p._colorspace = _colorspace.Clone() as Colorspace;
            return p;
        }

        /// <summary>
        /// Returns a System.String that represents the current Bytescout.PDF.ColorRGB.
        /// </summary>
        /// <returns cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A System.String that represents the current Bytescout.PDF.ColorRGB.</returns>
        public override string ToString()
        {
            string member = StringUtility.GetString(_r * 1.0f / 255) + ' ' +
                            StringUtility.GetString(_g * 1.0f / 255) + ' ' +
                            StringUtility.GetString(_b * 1.0f / 255);
            return member;
        }

        /// <summary>
        /// Determines whether the specified Bytescout.PDF.ColorRGB is equal to the current Bytescout.PDF.ColorRGB.
        /// </summary>
        /// <param name="obj" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">The System.Object to compare with the current Bytescout.PDF.ColorRGB.</param>
        /// <returns cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">true if the specified System.Object is equal to the current Bytescout.PDF.ColorRGB; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ColorRGB))
                return false;
            ColorRGB color = obj as ColorRGB;
            return (color._r == _r && color._g == _g && color._b == _b);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">A hash code for the current Bytescout.PDF.ColorRGB.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal override float[] ToArray()
        {
            float[] result = new float[3];
            result[0] = _r / 255f;
            result[1] = _g / 255f;
            result[2] = _b / 255f;
            return result;
        }
    }
}
