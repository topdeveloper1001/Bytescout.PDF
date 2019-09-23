using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class TableColumn
#else
	/// <summary>
    /// Represents the table column.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class TableColumn
#endif
	{
	    private float _width = 50.0f;
	    private DeviceColor _colorBackground = null;
	    private string _columnName = "";
	    private string _caption = "";
	    private StringFormat _formatText = new StringFormat();
	    private Font _fontText = new Font(StandardFonts.Helvetica, 8);
	    private DeviceColor _colorText = new ColorRGB(0, 0, 0);

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
        /// Gets or sets the width of the column.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Width
        {
            get
            {
                return _width;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
                _width = value;
            }
        }

        /// <summary>
        /// Gets or sets the column caption.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Text
        {
            get
            {
                return _caption;
            }

            set
            {
                _caption = value;
            }
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string ColumnName
        {
            get
            {
                return _columnName;
            }
        }

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
        /// Gets or sets the font of the column.
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
	    /// Initializes a new instance of the Bytescout.PDF.TableColumn class.
	    /// </summary>
	    public TableColumn()
	    {
		    _formatText.HorizontalAlign = HorizontalAlign.Center;
		    _formatText.VerticalAlign = VerticalAlign.Center;
		    _columnName = Guid.NewGuid().ToString();
	    }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.TableColumn class.
	    /// </summary>
	    /// <param name="columnName">The name of the column.</param>
	    public TableColumn(string columnName)
	    {
		    _formatText.HorizontalAlign = HorizontalAlign.Center;
		    _formatText.VerticalAlign = VerticalAlign.Center;
		    if (columnName != "")
			    _columnName = columnName;
		    else
			    _columnName = Guid.NewGuid().ToString();
	    }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.TableColumn class.
	    /// </summary>
	    /// <param name="columnName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the column.</param>
	    /// <param name="caption" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The caption.</param>
	    public TableColumn(string columnName, string caption)
	    {
		    _formatText.HorizontalAlign = HorizontalAlign.Center;
		    _formatText.VerticalAlign = VerticalAlign.Center;
		    _columnName = columnName;
		    _caption = caption;
	    }
    }
}
