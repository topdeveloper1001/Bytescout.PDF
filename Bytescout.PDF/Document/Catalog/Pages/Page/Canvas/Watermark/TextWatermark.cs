using System;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class TextWatermark : Watermark
#else
    /// <summary>
    /// Represents a text watermark.
    /// </summary>
    public class TextWatermark : Watermark
#endif
    {
		private String _text;
	    private Font _font;

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.TextWatermark class.
        /// </summary>
		/// <param name="text">Watermark text.</param>
        public TextWatermark(String text)
        {
	        _font = new Font(StandardFonts.Helvetica, 16);
			_text = text;

	        Left = 0f;
	        Top = 0f;
	        Angle = 0f;
	        Brush = new SolidBrush(new ColorRGB(0, 0, 0)) { Opacity = 50 };
	        Pen = null;
	        WatermarkLocation = TextWatermarkLocation.Custom;
		}

        public override object Clone()
        {
	        return new TextWatermark(_text)
	        {
		        Left = Left,
		        Top = Top,
		        Angle = Angle,
		        Font = Font,
		        Brush = Brush,
		        Pen = Pen,
		        WatermarkLocation = WatermarkLocation
			};
        }

        /// <summary>
        /// Gets or sets the text of the watermark.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value ?? "";
            }
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the left edge of watermark text.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Left { get; set; }

	    /// <summary>
        /// Gets or sets the y-coordinate of the top edge of watermark text.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Top { get; set; }

	    /// <summary>
        /// Gets or sets the angle of rotation of watermark text.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Angle { get; set; }

	    /// <summary>
        /// Gets or sets the font used to display text watermark.
        /// </summary>
        /// <value cref="Font"></value>
        public Font Font
        {
            get 
			{
                return _font; 
            }
            set
            {
	            if (value == null)
		            throw new ArgumentNullException();

				_font = value;
            }
        }


		/// <summary>
		/// Watermark brush. Default is transparent gray brush.
		/// </summary>
		public Brush Brush { get; set; }

		/// <summary>
		/// Watermark pen. Default is <c>null</c> (no text outline).
		/// </summary>
		public Pen Pen { get; set; }

		/// <summary>
		/// Watermark location and orientation.
		/// </summary>
		public TextWatermarkLocation WatermarkLocation { get; set; }
	}

#if PDFSDK_EMBEDDED_SOURCES
	internal enum TextWatermarkLocation
#else
	/// <summary>
	/// Represents the text watermark location and orientation.
	/// </summary>
	public enum TextWatermarkLocation
#endif
	{
		Custom,
		Top,
		Bottom,
		Center,
		VerticalFromTopToBottom,
		VerticalFromBottomToTop,
		DiagonalFromTopLeftToBottomRight,
		DiagonalFromBottomLeftToTopRight,
        Tiled
	}
}
