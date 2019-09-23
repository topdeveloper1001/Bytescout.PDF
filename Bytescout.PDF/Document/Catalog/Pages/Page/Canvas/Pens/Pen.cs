using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class Pen
#else
	/// <summary>
    /// Represents the abstract pen, which contains the basic functionality of a pen.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public abstract class Pen
#endif
	{
        /// <summary>
        /// Gets or sets the miter limit.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public abstract float MiterLimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the line dash pattern.
        /// </summary>
        /// <value cref="PDF.DashPattern"></value>
        public abstract DashPattern DashPattern
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the width of this pen.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public abstract float Width
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the line cap of this pen.
        /// </summary>
        /// <value cref="LineCapStyle"></value>
        public abstract LineCapStyle LineCap
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the line join style of this pen.
        /// </summary>
        /// <value cref="LineJoinStyle"></value>
        public abstract LineJoinStyle LineJoin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the opacity value in percent.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public abstract float Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.Pen.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.Pen.</returns>
        public abstract object Clone();

        internal abstract void WriteParameters(MemoryStream stream, Resources resources);
        internal abstract void WriteChanges(Pen newPen, MemoryStream stream, Resources resources);
        internal abstract void WriteForNotStroking(MemoryStream stream, Resources resources);
    }
}
