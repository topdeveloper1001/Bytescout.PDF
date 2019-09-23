using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Font
#else
	/// <summary>
    ///  Represents a font in the PDF document.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class Font
#endif
	{
	    private FontBase _baseFont;
	    private float _size;
	    private bool _bold;
	    private bool _italic;
	    private bool _underline;
	    private bool _strikeout;

	    internal event ChangedFontSizeEventHandler ChangedFontSize;

	    /// <summary>
	    /// Gets the name of the font.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string Name { get { return _baseFont.Name; } }

	    /// <summary>
	    /// Gets a value indicating whether this Bytescout.PDF.Font is bold.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Bold
	    {
		    get { return _bold; }
		    internal set { _bold = value; }
	    }

	    /// <summary>
	    /// Gets a value indicating whether this Bytescout.PDF.Font is italic.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Italic
	    {
		    get { return _italic; }
		    internal set { _italic = value; }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether this Bytescout.PDF.Font is underlined.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Underline
	    {
		    get { return _underline; }
		    set { _underline = value; }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether this Bytescout.PDF.Font specifies a horizontal line through the font.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Strikeout
	    {
		    get { return _strikeout; }
		    set { _strikeout = value; }
	    }

	    /// <summary>
	    /// Gets or sets the em-size of this Bytescout.PDF.Font.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float Size
	    {
		    get { return _size; }
		    set
		    {
			    if (value < 0)
				    throw new ArgumentOutOfRangeException();

			    _size = value;
			    if (ChangedFontSize != null)
				    ChangedFontSize(this);
		    }
	    }

	    internal FontBase BaseFont { get { return _baseFont; } }

	    /// <summary>
        /// Initializes a new Bytescout.PDF.Font using a specified properties.
        /// </summary>
        /// <param name="fontName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the font.</param>
        /// <param name="emSize" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The em-size, in points, of the font.</param>
        /// <param name="bold" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">If set to true, the initialized font will be bold.</param>
        /// <param name="italic" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">If set to true, the initialized font will be italic.</param>
        /// <param name="underline" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">If set to true, the initialized font will be underlined.</param>
        /// <param name="strikeout" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">If set to true, the initialized font will be stroked out.</param>
        public Font(string fontName, float emSize, bool bold, bool italic, bool underline, bool strikeout)
        {
            if (emSize < 0)
                throw new ArgumentOutOfRangeException("emSize");

            _baseFont = FontsManager.AddFont(fontName, bold, italic);
            _strikeout = strikeout;
            _underline = underline;
            _size = emSize;
            _bold = bold;
            _italic = italic;
        }

        /// <summary>
        /// Initializes a new Bytescout.PDF.Font using a specified properties.
        /// </summary>
        /// <param name="fontName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the font.</param>
        /// <param name="emSize" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The em-size, in points, of the font.</param>
        public Font(string fontName, float emSize)
            : this(fontName, emSize, false, false, false, false) { }

        /// <summary>
        /// Initializes a new instance of the standard font with specified properties.
        /// </summary>
        /// <param name="standardFont">The Bytescout.PDF.StandardFonts value that specifies the standard font.</param>
        /// <param name="emSize" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The em-size, in points, of the font.</param>
        /// <param name="underline" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">If set to true, the initialized font will be underlined.</param>
        /// <param name="strikeout" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">If set to true, the initialized font will be stroked out.</param>
        public Font(StandardFonts standardFont, float emSize, bool underline, bool strikeout)
        {
            if (emSize < 0)
                throw new ArgumentOutOfRangeException("emSize");

            _baseFont = FontsManager.AddStandardFont(standardFont, underline, strikeout);
            _size = emSize;
            _bold = _baseFont.RealBold;
            _italic = _baseFont.RealItalic;
            _underline = underline;
            _strikeout = strikeout;
        }

        /// <summary>
        /// Initializes a new instance of the standard font with specified properties.
        /// </summary>
        /// <param name="standardFont">The Bytescout.PDF.StandardFonts value that specifies the standard font.</param>
        /// <param name="emSize" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The em-size, in points, of the font.</param>
        public Font(StandardFonts standardFont, float emSize)
            : this(standardFont, emSize, false, false) { }

        internal Font(FontBase baseFont)
        {
            _baseFont = baseFont;
        }

	    /// <summary>
        /// Get the height of a string in pixels.
        /// </summary>
		/// <returns cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the string in document points (1/72").</returns>
        public float GetTextHeight()
        {
            return _baseFont.GetTextHeight(_size);
        }

        /// <summary>
        /// Get the width in pixels of the specified string.
        /// </summary>
        /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The measured string.</param>
        /// <returns cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the string in document points (1/72").</returns>
        public float GetTextWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            return _baseFont.GetTextWidth(text, _size);
        }

        /// <summary>
        /// Creates a Bytescout.PDF.Font from the specified file.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A string that contains the name of the file from which to create the Bytescout.PDF.Font.</param>
        /// <param name="emSize" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The em-size, in points, of the font.</param>
        /// <param name="underline" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">If set to true, the initialized font will be underlined.</param>
        /// <param name="strikeout" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">If set to true, the initialized font will be stroked out.</param>
        /// <returns cref="Font">The Bytescout.PDF.Font this method creates.</returns>
        public static Font FromFile(string fileName, float emSize, bool underline, bool strikeout)
        {
            if (emSize < 0)
                throw new ArgumentOutOfRangeException("emSize");

            FontBase fnt = FontsManager.AddFontFromFile(fileName);
            Font font = new Font(fnt);
            font._strikeout = strikeout;
            font._underline = underline;
            font.Size = emSize;
            return font;
        }

        /// <summary>
        /// Creates a Bytescout.PDF.Font from the specified file.
        /// </summary>
        /// <param name="fileName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A string that contains the name of the file from which to create the Bytescout.PDF.Font.</param>
        /// <param name="emSize" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The em-size, in points, of the font.</param>
        /// <returns cref="Font">The Bytescout.PDF.Font this method creates.</returns>
        public static Font FromFile(string fileName, float emSize)
        {
            return Font.FromFile(fileName, emSize, false, false);
        }

        /// <summary>
        /// Creates a Bytescout.PDF.Font from the specified data stream.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">A System.IO.Stream that contains the data for this Bytescout.PDF.Font.</param>
        /// <param name="emSize" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The em-size, in points, of the font.</param>
        /// <param name="underline" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">If set to true, the initialized font will be underlined.</param>
        /// <param name="strikeout" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">If set to true, the initialized font will be stroked out.</param>
        /// <returns cref="Font">The Bytescout.PDF.Font this method creates.</returns>
        public static Font FromStream(System.IO.Stream stream, float emSize, bool underline, bool strikeout)
        {
            if (emSize < 0)
                throw new ArgumentOutOfRangeException("emSize");

            FontBase fnt = FontsManager.AddFontFromStream(stream);
            Font font = new Font(fnt);
            font._strikeout = strikeout;
            font._underline = underline;
            font.Size = emSize;
            return font;
        }

        /// <summary>
        /// Creates a Bytescout.PDF.Font from the specified data stream.
        /// </summary>
        /// <param name="stream" href="http://msdn.microsoft.com/en-us/library/system.io.stream.aspx">A System.IO.Stream that contains the data for this Bytescout.PDF.Font.</param>
        /// <param name="emSize" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The em-size, in points, of the font.</param>
        /// <returns cref="Font">The Bytescout.PDF.Font this method creates.</returns>
        public static Font FromStream(System.IO.Stream stream, float emSize)
        {
            return FromStream(stream, emSize, false, false);
        }

        internal void WriteParameters(System.IO.MemoryStream stream, Resources resources)
        {
            string fontName = resources.AddResources(ResourceType.Font, _baseFont.GetDictionary());
            IPDFPageOperation operation = new TextFont(fontName, Size);
            operation.WriteBytes(stream);
        }

        internal object Clone()
        {
            return MemberwiseClone();
        }

        internal float GetCharWidth(char c)
        {
            return _baseFont.GetCharWidth(c, _size);
        }
    }
}
