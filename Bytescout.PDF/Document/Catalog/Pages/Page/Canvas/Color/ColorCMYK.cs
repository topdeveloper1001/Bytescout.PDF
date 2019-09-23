using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ColorCMYK : DeviceColor
#else
	/// <summary>
    /// Represents a color in a CMYK color space.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class ColorCMYK : DeviceColor
#endif
	{
	    private readonly byte _c;
	    private readonly byte _m;
	    private readonly byte _y;
	    private readonly byte _k;
	    private Colorspace _colorspace;

	    /// <summary>
        /// Gets the cyan component value.
        /// </summary>
        /// <value cref="byte" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx"></value>
        public byte C { get { return _c; } }

        /// <summary>
        /// Gets the magenta component value.
        /// </summary>
        /// <value cref="byte" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx"></value>
        public byte M { get { return _m; } }

        /// <summary>
        /// Gets the yellow component value.
        /// </summary>
        /// <value cref="byte" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx"></value>
        public byte Y { get { return _y; } }

        /// <summary>
        /// Gets the key component value.
        /// </summary>
        /// <value cref="byte" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx"></value>
        public byte K { get { return _k; } }

        /// <summary>
        /// Gets the colorspace associated with this Bytescout.PDF.ColorCMYK.
        /// </summary>
        /// <value cref="PDF.Colorspace"></value>
        public override Colorspace Colorspace { get { return _colorspace; } }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.ColorCMYK class.
	    /// </summary>
	    /// <param name="cyan" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx">The cyan component value.</param>
	    /// <param name="magenta" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx">The magenta component value.</param>
	    /// <param name="yellow" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx">The yellow component value.</param>
	    /// <param name="key" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx">The key component value.</param>
	    public ColorCMYK(byte cyan, byte magenta, byte yellow, byte key)
	    {
		    _c = cyan;
		    _m = magenta;
		    _y = yellow;
		    _k = key;
		    _colorspace = new DeviceCMYKColorspace();
	    }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.ColorCMYK.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.ColorCMYK.</returns>
        public override object Clone()
        {
            ColorCMYK p = this.MemberwiseClone() as ColorCMYK;
            p._colorspace = _colorspace.Clone() as Colorspace;
            return p;
        }

        /// <summary>
        /// Returns a System.String that represents the current Bytescout.PDF.ColorCMYK.
        /// </summary>
        /// <returns cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A System.String that represents the current Bytescout.PDF.ColorCMYK.</returns>
        public override string ToString()
        {
            string member = StringUtility.GetString(_c * 1.0f / 255) + ' ' +
                            StringUtility.GetString(_m * 1.0f / 255) + ' ' +
                            StringUtility.GetString(_y * 1.0f / 255) + ' ' +
                            StringUtility.GetString(_k * 1.0f / 255);
            return member;
        }

        /// <summary>
        /// Determines whether the specified Bytescout.PDF.ColorCMYK is equal to the current Bytescout.PDF.ColorCMYK.
        /// </summary>
        /// <param name="obj" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">The System.Object to compare with the current Bytescout.PDF.ColorCMYK.</param>
        /// <returns cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">true if the specified System.Object is equal to the current Bytescout.PDF.ColorCMYK; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ColorCMYK))
                return false;
            ColorCMYK color = obj as ColorCMYK;
            return (color._c == _c && color._m == _m && color._y == _y && color._k == _k);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">A hash code for the current Bytescout.PDF.ColorCMYK.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal override float[] ToArray()
        {
            float[] result = new float[4];
            result[0] = _c / 255f;
            result[1] = _m / 255f;
            result[2] = _y / 255f;
            result[3] = _k / 255f;
            return result;
        }
    }
}
