using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class DeviceCMYKColorspace : Colorspace
#else
	/// <summary>
    /// Represents Device CMYK color space.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class DeviceCMYKColorspace : Colorspace
#endif
	{
	    private readonly int _n;
	    private readonly string _name;

	    /// <summary>
        /// Gets the name of this color space.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public override string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the number of components of this color space.
        /// </summary>
        /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
        public override int N
        {
            get 
            {
                return _n;
            }
        }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.DeviceCMYKColorspace class.
	    /// </summary>
	    public DeviceCMYKColorspace()
	    {
		    _n = 4;
		    _name = "DeviceCMYK";
	    }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.DeviceCMYKColorspace.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.DeviceCMYKColorspace.</returns>
        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        internal override void WriteColorSpaceForStroking(MemoryStream stream, Resources resources)
        {
            IPDFPageOperation operation = new ColorSpaceForStroking(_name);
            operation.WriteBytes(stream);
        }

        internal override void WriteColorSpaceForNotStroking(MemoryStream stream, Resources resources)
        {
            IPDFPageOperation operation = new ColorSpaceForNonStroking(_name);
            operation.WriteBytes(stream);
        }

        internal override bool WriteChangesForStroking(Colorspace newCS, MemoryStream stream, Resources resources)
        {
            if (!(newCS is DeviceCMYKColorspace))
            {
                WriteColorSpaceForStroking(stream, resources);
                return true;
            }
            return false;
        }

        internal override bool WriteChangesForNotStroking(Colorspace newCS, MemoryStream stream, Resources resources)
        {
            if (!(newCS is DeviceCMYKColorspace))
            {
                WriteColorSpaceForNotStroking(stream, resources);
                return true;
            }
            return false;
        }
    }
}
