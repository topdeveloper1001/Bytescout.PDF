using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ColorGray : DeviceColor
#else
	/// <summary>
    /// Represents a color in a Gray color space.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class ColorGray : DeviceColor
#endif
	{
	    private readonly byte _g;
	    private Colorspace _colorspace;

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.ColorGray class.
        /// </summary>
        /// <param name="gray" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx">The gray level of color.</param>
        public ColorGray(byte gray)
        {
            _g = gray;
            _colorspace = new DeviceGrayColorspace();
        }

        /// <summary>
        /// Gets the gray level of color.
        /// </summary>
        /// <value cref="byte" href="http://msdn.microsoft.com/en-us/library/system.byte.aspx"></value>
        public byte G { get { return _g; } }

        /// <summary>
        /// Gets the colorspace associated with this Bytescout.PDF.ColorGray.
        /// </summary>
        /// <value cref="PDF.Colorspace"></value>
        public override Colorspace Colorspace { get { return _colorspace; } }

        /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.ColorGray.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.ColorGray.</returns>
        public override object Clone()
        {
            ColorGray p = this.MemberwiseClone() as ColorGray;
            p._colorspace = _colorspace.Clone() as Colorspace;
            return p;
        }

        /// <summary>
        /// Returns a System.String that represents the current Bytescout.PDF.ColorGray.
        /// </summary>
        /// <returns cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A System.String that represents the current Bytescout.PDF.ColorGray.</returns>
        public override string ToString()
        {
            return StringUtility.GetString(_g * 1.0f / 255);
        }

        /// <summary>
        /// Determines whether the specified Bytescout.PDF.ColorGray is equal to the current Bytescout.PDF.ColorGray.
        /// </summary>
        /// <param name="obj" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">The System.Object to compare with the current Bytescout.PDF.ColorGray.</param>
        /// <returns cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">true if the specified System.Object is equal to the current Bytescout.PDF.ColorGray; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ColorGray))
                return false;
            ColorGray color = obj as ColorGray;
            return (color._g == _g);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">A hash code for the current Bytescout.PDF.ColorGray.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal override float[] ToArray()
        {
            float[] result = new float[1];
            result[0] = _g / 255f;
            return result;
        }
    }
}
