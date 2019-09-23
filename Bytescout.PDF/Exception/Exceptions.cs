using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class PDFException : Exception
#else
	/// <summary>
	/// Represents general PDF exception.
	/// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class PDFException : Exception
#endif
	{
		/// <summary>
		/// 
		/// </summary>
		public PDFException() { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public PDFException(string message) : base(message) { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public PDFException(string message, Exception innerException) : base(message, innerException) { }
	}

    internal class BorderEffectIntensityException : PDFException
    {
        public BorderEffectIntensityException()
            : base("Intensity of the effect is out of range. Suggested values range from 0 to 2.")
        { }
    }

    internal class PDFVolumeException : PDFException
    {
        public PDFVolumeException()
            : base("Volume value is out of range. Suggested values range from -1.0 to 1.0")
        { }
    }

    internal class PDFMiterLimitException : PDFException
    {
        public PDFMiterLimitException()
            : base("Miter limit is out of range. Suggested values range from 0 to 11.5")
        { }
    }

    internal class PDFOpacityException : PDFException
    {
        public PDFOpacityException()
            : base("Opacity value is out of range. Suggested values range from 0 to 100.")
        { }
    }

    internal class InvalidDocumentException : PDFException
    {
        public InvalidDocumentException()
            : base("Document is invalid.")
        { }
    }

    internal class PDFUnsupportEncryptorException : PDFException
    {
        public PDFUnsupportEncryptorException()
            : base("Unsupported encryption algorithm.")
        { }
    }

    internal class PDFUnsupportFontFormatException : PDFException
    {
        public PDFUnsupportFontFormatException()
            : base("Unsupported font format.")
        { }
    }

    internal class PDFWrongFontFileException : PDFException
    {
        public PDFWrongFontFileException()
            : base("The font file is damaged or unsupported format.")
        { }
    }

    internal class PDFUnableLoadFontException : PDFException
    {
        public PDFUnableLoadFontException()
            : base("Unable to load font.")
        { }
    }

    internal class PDFInvalidICCException : PDFException
    {
        public PDFInvalidICCException()
            : base("ICC color profile is invalid.")
        { }
    }

    internal class PDFNotEqualColorspacesException : PDFException
    {
        public PDFNotEqualColorspacesException()
            : base("Color space of color and color should be the same.")
        { }
    }

    internal class PDFUnsupportImageFormat : PDFException
    {
        public PDFUnsupportImageFormat()
            : base("Unsupport image format.")
        { }
    }

    internal class PDFUnsupportedSoundFormatException : PDFException
    {
        public PDFUnsupportedSoundFormatException()
            : base("Unsupport sound format.")
        { }
    }

    internal class PDFFieldNameOccupied : PDFException
    {
        public PDFFieldNameOccupied()
            : base("Field with such name already exists.")
        { }
    }
}
