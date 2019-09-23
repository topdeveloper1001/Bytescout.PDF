using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class Color : ICloneable
#else
	/// <summary>
    /// Represents an abstract class for PDF color.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public abstract class Color : ICloneable
#endif
	{
        /// <summary>
        /// Gets the colorspace associated with this Bytescout.PDF.Color.
        /// </summary>
        /// <value cref="PDF.Colorspace"></value>
        public abstract Colorspace Colorspace
        {
            get;
        }

        /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.Color.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.Color.</returns>
        public abstract object Clone();

        internal abstract float[] ToArray();
    }
}
