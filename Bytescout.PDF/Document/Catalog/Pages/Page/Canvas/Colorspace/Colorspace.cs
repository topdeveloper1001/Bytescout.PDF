using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class Colorspace : ICloneable
#else
	/// <summary>
    /// Represents an abstract class for color spaces.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class Colorspace : ICloneable
#endif
	{
        /// <summary>
        /// Gets the name of this color space.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Gets the number components of this color space.
        /// </summary>
        /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
        public abstract int N
        {
            get;
        }

        /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.Colorspace.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.Colorspace.</returns>
        public abstract object Clone();

        internal abstract void WriteColorSpaceForStroking(MemoryStream stream, Resources resources);
        internal abstract void WriteColorSpaceForNotStroking(MemoryStream stream, Resources resources);
        internal abstract bool WriteChangesForStroking(Colorspace newCS, MemoryStream stream, Resources resources);
        internal abstract bool WriteChangesForNotStroking(Colorspace newCS, MemoryStream stream, Resources resources);
    }
}
