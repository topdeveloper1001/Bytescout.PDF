using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class Brush
#else
	/// <summary>
    /// Represents the abstract Brush, which contains the basic functionality of a Brush.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public abstract class Brush
#endif
	{
        /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.Brush
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.Brush</returns>
        public abstract object Clone();

        internal abstract void WriteParameters(MemoryStream stream, Resources resources);

        internal abstract void WriteChanges(Brush newBrush, MemoryStream stream, Resources resources);
    }
}
