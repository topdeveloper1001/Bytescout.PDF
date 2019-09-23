using System;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Table
#else
	/// <summary>
    /// Represents the table.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class Table
#endif
	{
	    private string _tableName = "";
	    private DeviceColor _colorBackground = null;
	    private DeviceColor _colorBorder = new ColorRGB(0, 0, 0);
	    private TableBorderStyle _borderStyle = TableBorderStyle.Solid;
	    private float _widthBorder = 1.0f;
	    private float _heightHeadRow = 20.0f;
	    private StringFormat _formatText = new StringFormat();
	    private Font _fontText = new Font(StandardFonts.Helvetica, 8);
	    private DeviceColor _colorText = new ColorRGB(0, 0, 0);
	    private TableColumnCollection _columns;
	    private readonly TableRowCollection _rows = new TableRowCollection();

	    /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Name
        {
            get
            {
                return _tableName;
            }

            set
            {
                _tableName = value;
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
        /// Gets or sets the color of the border.
        /// </summary>
        /// <value cref="DeviceColor"></value>
        public DeviceColor BorderColor
        {
            get
            {
                return _colorBorder;
            }

            set
            {
                _colorBorder = value;
            }
        }

        /// <summary>
        /// Gets or sets the border style.
        /// </summary>
        /// <value cref="TableBorderStyle"></value>
        public TableBorderStyle BorderStyle
        {
            get
            {
                return _borderStyle;
            }

            set
            {
                _borderStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the border width.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float BorderWidth
        {
            get
            {
                return _widthBorder;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
                _widthBorder = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the table header.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float HeaderHeight
        {
            get
            {
                return _heightHeadRow;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
                _heightHeadRow = value;
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
        /// Gets or sets the font of the table.
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
        /// Gets the table column collection.
        /// </summary>
        /// <value cref="TableColumnCollection"></value>
        public TableColumnCollection Columns
        {
            get
            {
                if (_columns == null)
                    _columns = new TableColumnCollection(this);
                return _columns;
            }
        }

        /// <summary>
        /// Gets the table row collection.
        /// </summary>
        /// <value cref="TableRowCollection"></value>
        public TableRowCollection Rows
        {
            get
            {
                return _rows;
            }
        }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.Table class.
	    /// </summary>
	    public Table()
	    {
		    _formatText.HorizontalAlign = HorizontalAlign.Center;
		    _formatText.VerticalAlign = VerticalAlign.Center;
	    }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.Table class.
	    /// </summary>
	    /// <param name="tableName" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The name of the table.</param>
	    public Table(string tableName)
	    {
		    _tableName = tableName;
		    _formatText.HorizontalAlign = HorizontalAlign.Center;
		    _formatText.VerticalAlign = VerticalAlign.Center;
	    }

	    /// <summary>
        /// Creates a new Bytescout.PDF.TableRow with the same schema as the table.
        /// </summary>
        /// <returns cref="TableRow">A Bytescout.PDF.TableRow with the same schema as the Bytescout.PDF.Table.</returns>
        public TableRow NewRow()
        {
            return new TableRow(_columns.HashTable, true);
        }
    }
}
