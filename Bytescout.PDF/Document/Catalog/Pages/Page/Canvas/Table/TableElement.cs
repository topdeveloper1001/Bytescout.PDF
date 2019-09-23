using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class TableElement
#else
	/// <summary>
    /// Represents the a cell of the table.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class TableElement
#endif
	{
	    private string _text = "";
		private DeviceColor _colorBackground = null;
		private StringFormat _formatText = new StringFormat();
	    private Font _fontText = new Font(StandardFonts.Helvetica, 8);
	    private DeviceColor _colorText = new ColorRGB(0, 0, 0);

	    /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value cref="DeviceColor"></value>
        public DeviceColor BackgroundColor
        {
            get
            {
                return _colorBackground;
            }

            set
            {
                _colorBackground = value;
            }
        }

        /// <summary>
        /// Gets or sets the text string format.
        /// </summary>
        /// <value cref="StringFormat"></value>
        public StringFormat TextFormat
        {
            get
            {
                return _formatText;
            }

            set
            {
                _formatText = value;
            }
        }

        /// <summary>
        /// Gets or sets the font of a cell.
        /// </summary>
        /// <value cref="PDF.Font"></value>
        public Font Font
        {
            get
            {
                return _fontText;
            }
            set
            {
                _fontText = value;
            }
        }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <value cref="DeviceColor"></value>
        public DeviceColor TextColor
        {
            get
            {
                return _colorText;
            }
            set
            {
                if (value != null)
                    _colorText = value;
            }
        }

        /// <summary>
        /// Gets or sets the text of a cell.
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
                _text = value;
            }
        }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.TableElement class.
	    /// </summary>
	    public TableElement()
	    {
		    _formatText.HorizontalAlign = HorizontalAlign.Center;
		    _formatText.VerticalAlign = VerticalAlign.Center;
	    }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.TableElement class.
	    /// </summary>
	    /// <param name="text" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The text of a cell.</param>
	    public TableElement(string text)
	    {
		    _text = text;
		    _formatText.HorizontalAlign = HorizontalAlign.Center;
		    _formatText.VerticalAlign = VerticalAlign.Center;
	    }
    }
}
