﻿using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class DeviceGrayColorspace : Colorspace
#else
	/// <summary>
    /// Represents Device Gray color space.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class DeviceGrayColorspace : Colorspace
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
        /// Gets the number components of this color space.
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
	    /// Initializes a new instance of the Bytescout.PDF.DeviceGrayColorspace class.
	    /// </summary>
	    public DeviceGrayColorspace()
	    {
		    _n = 1;
		    _name = "DeviceGray";
	    }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.DeviceGrayColorspace.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.DeviceGrayColorspace.</returns>
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
            if (!(newCS is DeviceGrayColorspace))
            {
                WriteColorSpaceForStroking(stream, resources);
                return true;
            }
            return false;
        }

        internal override bool WriteChangesForNotStroking(Colorspace newCS, MemoryStream stream, Resources resources)
        {
            if (!(newCS is DeviceGrayColorspace))
            {
                WriteColorSpaceForNotStroking(stream, resources);
                return true;
            }
            return false;
        }
    }
}
