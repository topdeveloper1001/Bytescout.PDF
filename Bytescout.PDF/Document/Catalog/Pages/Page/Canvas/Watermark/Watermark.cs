using System;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class Watermark : ICloneable
#else
    /// <summary>
    /// Represents the abstract watermark, which contains the basic functionality of a watermark.
    /// </summary>
    public abstract class Watermark : ICloneable
#endif
    {
        /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.Watermark.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.Watermark.</returns>
        public abstract object Clone();
    }
}
