using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
    internal enum TextRenderingModes
    {
        Fill = 0,
        Stroke = 1,
        FillAndStroke = 2,
        NeitherFill = 3,
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal class StringFormat : ICloneable
#else
	/// <summary>
    /// Represents the text layout information.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class StringFormat : ICloneable
#endif
	{
	    private bool _directionRightToLeft;
	    private float _charSpace;
	    private float _wordSpace;
	    private float _scale;
	    private float _rise;
	    private HorizontalAlign _halign;
	    private VerticalAlign _valign;
	    private float _leading;
	    private static StringFormat _default = null;

	    /// <summary>
        /// Gets or sets the text direction.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool DirectionRightToLeft
        {
            get { return _directionRightToLeft; }
            set { _directionRightToLeft = value; }
        }

        /// <summary>
        /// Gets or sets the text leading.
        /// It specifies the vertical distance between the baselines of adjacent lines of text.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Leading
        {
            get { return _leading; }
            set { _leading = value; }
        }

        /// <summary>
        /// Gets or sets value that indicates a distance between the text characters.
        /// When the glyph for each character in the string is rendered, this value is added to the the glyph’s displacement.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float CharacterSpacing
        {
            get { return _charSpace; }
            set { _charSpace = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates spacing between the words in the text.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float WordSpacing
        {
            get { return _wordSpace; }
            set { _wordSpace = value; }
        }

        /// <summary>
        /// Gets or sets the horizontal scaling of text.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Scaling
        {
            get { return _scale; }
            set { _scale = value; }
        }

        /// <summary>
        /// Gets or sets the text rise.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Rise
        {
            get { return _rise; }
            set { _rise = value; }
        }

        /// <summary>
        /// Gets or sets the text horizontal alignment.
        /// </summary>
        /// <value cref="PDF.HorizontalAlign"></value>
        public HorizontalAlign HorizontalAlign
        {
            get { return _halign; }
            set { _halign = value; }
        }

        /// <summary>
        /// Gets or sets the text vertical alignment.
        /// </summary>
        /// <value cref="PDF.VerticalAlign"></value>
        public VerticalAlign VerticalAlign
        {
            get { return _valign; }
            set { _valign = value; }
        }

        internal static StringFormat DefaultFormat
        {
            get
            {
                if (_default == null)
                    _default = new StringFormat();
                return _default;
            }
        }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.StringFormat class.
	    /// </summary>
	    public StringFormat()
	    {
		    _charSpace = 0.0f;
		    _wordSpace = 0.0f;
		    _scale = 100.0f;
		    _rise = 0.0f;
		    _leading = 1.2f;
		    _halign = PDF.HorizontalAlign.Left;
		    _valign = VerticalAlign.Top;
		    _directionRightToLeft = false;
	    }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.StringFormat.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.StringFormat.</returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        internal void WriteChanges(StringFormat newSF, MemoryStream stream, Resources resources)
        {
            if (!this._charSpace.Equals(newSF._charSpace))
                addOperation(new CharacterSpacing(newSF._charSpace), stream);
            if (!this._rise.Equals(newSF._rise))
                addOperation(new TextRise(newSF._rise), stream);
            if (!this._scale.Equals(newSF._scale))
                addOperation(new HorizontalScaling(newSF._scale), stream);
            if (!this._wordSpace.Equals(newSF._wordSpace))
                addOperation(new WordSpacing(newSF._wordSpace), stream);
        }

        private void addOperation(IPDFPageOperation operation, MemoryStream stream)
        {
            operation.WriteBytes(stream);
        }
    }
}
