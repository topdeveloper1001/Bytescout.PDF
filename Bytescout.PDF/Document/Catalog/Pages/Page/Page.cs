using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Page
#else
	/// <summary>
    /// Represents a PDF document page.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public sealed class Page
#endif
    {
	    private Image _thumbnail;
	    private Canvas _canvas;
	    private Resources _resources;
	    private AnnotationCollections _annotations;
	    private PDFGroup _group;

	    private readonly PDFDictionary _dictionary;
	    private IDocumentEssential _owner;

	    private Action _onOpened;
	    private Action _onClosed;
	    private string _id = Guid.NewGuid().ToString();

	    /// <summary>
        /// Gets the collection of images added to this page.
        /// </summary>
        /// <value cref="!:ReadOnlyCollection"></value>
        [ComVisible(false)]
        public ReadOnlyCollection<Image> Images
        {
            get
            {
                return Resources.Images;
            }
        }

        /// <summary>
        /// Gets or sets the width of the page in pixels.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Width
        {
            get { return PageRect.Width; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException();
                setPageWidth(value);
            }
        }

        /// <summary>
        /// Gets or sets the height of the page in pixels.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Height
        {
            get { return PageRect.Height; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException();
                setPageHeight(value);
            }
        }

        /// <summary>
        /// Gets or sets the page rotation angle.
        /// </summary>
        /// <value cref="PDF.RotationAngle"></value>
        public RotationAngle RotationAngle
        {
            get { return TypeConverter.PDFNumberToRotationAngle(_dictionary["Rotate"] as PDFNumber); }
            set { _dictionary.AddItem("Rotate", TypeConverter.RotationAngleToPDFNumber(value)); }
        }

        /// <summary>
        /// Gets the page canvas.
        /// </summary>
        /// <value cref="PDF.Canvas"></value>
        public Canvas Canvas
        {
            get
            {
                if (_canvas == null)
                    loadCanvas();
                return _canvas; 
            }
        }

        /// <summary>
        /// Gets the collection of annotations added to this page.
        /// </summary>
        /// <value cref="AnnotationCollections"></value>
        public AnnotationCollections Annotations
        {
            get
            {
                if (_annotations == null)
                    loadAnnotations();
                return _annotations;
            }
            private set
            {
                _annotations = value;
                _dictionary.AddItem("Annots", value.GetArray());
            }
        }

        /// <summary>
        /// Gets or sets the thumbnail image to be used for this page by a viewer application instead of an auto-generated image of the contents of this page in miniature form.
        /// </summary>
        /// <value cref="Image"></value>
        public Image Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                    loadThumbnail();
                return _thumbnail;
            }
            set
            {
                _thumbnail = value;
                if (_thumbnail == null)
                    _dictionary.RemoveItem("Thumb");
                else
                    _dictionary.AddItem("Thumb", _thumbnail.GetDictionary());
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the page is opened (for example, when the user navigates
        /// to it from the next or previous page or by means of a link annotation or outline item).
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnOpened
        {
            get
            {
                if (_onOpened == null)
                    loadAdditionalAction("O");
                return _onOpened;
            }
            set
            {
                setAdditionalAction(value, "O");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the page is closed (for example, when the user navigates
        /// to the next or previous page or follows a link annotation or an outline item).
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnClosed
        {
            get
            {
                if (_onClosed == null)
                    loadAdditionalAction("C");
                return _onClosed;
            }
            set
            {
                setAdditionalAction(value, "C");
            }
        }

		/// <summary>
		/// Gets the collection of watermarks added to this page.
		/// </summary>
		public WatermarkCollection Watermarks { get; private set; }

	    internal PageCollection Parent
        {
            set { _dictionary.AddItem("Parent", value.GetDictionary()); }
        }

        internal Resources Resources
        {
            get
            {
                if (_resources == null)
                    loadResources();
                return _resources;
            }
        }

        internal RectangleF PageRect
        {
            get
            {
                RectangleF rect = new RectangleF();
                RectangleF media = MediaBox;
                RectangleF crop = CropBox;

                // workaround for malformed pages without MediaBox 
                if (MediaBox.IsEmpty)
                    media = crop;

                rect.Width = Math.Min(media.Width, crop.Width);
                rect.Height = Math.Min(media.Height, crop.Height);
                rect.X = getMin(media.X, crop.X);
                rect.Y = getMin(media.Y, crop.Y);

                return rect;
            }
        }

        internal RectangleF MediaBox
        {
            get { return getBox(_dictionary["MediaBox"] as PDFArray); }
        }

        internal RectangleF CropBox
        {
            get
            {
                RectangleF rect = getBox(_dictionary["CropBox"] as PDFArray);
                if (rect == RectangleF.Empty)
                    return MediaBox;
                return rect;
            }
        }

        internal RectangleF BleedBox
        {
            get
            {
                RectangleF rect = getBox(_dictionary["BleedBox"] as PDFArray);
                if (rect == RectangleF.Empty)
                    return CropBox;
                return rect;
            }
        }

        internal RectangleF TrimBox
        {
            get
            {
                RectangleF rect = getBox(_dictionary["TrimBox"] as PDFArray);
                if (rect == RectangleF.Empty)
                    return CropBox;
                return rect;
            }
        }

        internal RectangleF ArtBox
        {
            get
            {
                RectangleF rect = getBox(_dictionary["ArtBox"] as PDFArray);
                if (rect == RectangleF.Empty)
                    return CropBox;
                return rect;
            }
        }

        internal PDFGroup Group
        {
            get
            {
                if (_group == null)
                    loadGroup();
                
                return _group;
            }
        }

        internal string PageID { get { return _id; } }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.Page class.
	    /// </summary>
	    /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The page width in PDF Points (1/72").</param>
		/// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The page height in PDF Points (1/72").</param>
	    public Page(float width, float height)
	    {
		    if (width <= 0)
			    throw new ArgumentOutOfRangeException("width");
		    if (height <= 0)
			    throw new ArgumentOutOfRangeException("height");

		    _dictionary = new PDFDictionary();
		    _dictionary.AddItem("Type", new PDFName("Page"));
		    _dictionary.AddItem("MediaBox", createBox(0, 0, width, height));

			Watermarks = new WatermarkCollection();
		}

		/// <summary>
		/// Initializes a new instance of the Bytescout.PDF.Page class.
		/// </summary>
		/// <param name="width">The page width in specified units.</param>
		/// <param name="height">The page height in specified units.</param>
		/// <param name="units">Units of measure.</param>
		public Page(float width, float height, UnitOfMeasure units)
	    {
		    if (width <= 0)
			    throw new ArgumentOutOfRangeException("width");
		    if (height <= 0)
			    throw new ArgumentOutOfRangeException("height");

			float newWidth, newHeight;

			switch (units)
			{
				case UnitOfMeasure.Inch:
					newWidth = width * 72;
					newHeight = height * 72;
					break;
				case UnitOfMeasure.Millimeter:
					newWidth = width / 25.4f * 72;
					newHeight = height / 25.4f * 72;
					break;
				case UnitOfMeasure.Centimeter:
					newWidth = width / 2.54f * 72;
					newHeight = height / 2.54f * 72;
					break;
				case UnitOfMeasure.Pixel96DPI:
					newWidth = width / 96 * 72;
					newHeight = height / 96 * 72;
					break;
				case UnitOfMeasure.Pixel120DPI:
					newWidth = width / 120 * 72;
					newHeight = height / 120 * 72;
					break;
				case UnitOfMeasure.Twip:
					newWidth = width / 20 * 72;
					newHeight = height / 20 * 72;
					break;
				case UnitOfMeasure.Document:
					newWidth = width / 300 * 72;
					newHeight = height / 300 * 72;
					break;
				default:
					newWidth = width;
					newHeight = height;
					break;
			}

			_dictionary = new PDFDictionary();
		    _dictionary.AddItem("Type", new PDFName("Page"));
			_dictionary.AddItem("MediaBox", createBox(0, 0, newWidth, newHeight));

		    Watermarks = new WatermarkCollection();
		}

		/// <summary>
		/// Initializes a new instance of the Bytescout.PDF.Page class.
		/// </summary>
		/// <param name="format">The page size.</param>
		/// <param name="orientation">The paper orientation.</param>
		public Page(PaperFormat format, PaperOrientation orientation)
		{
			_dictionary = new PDFDictionary();
			_dictionary.AddItem("Type", new PDFName("Page"));
			_dictionary.AddItem("MediaBox", createBox(PaperSizes.GetRect(format, orientation)));

			Watermarks = new WatermarkCollection();
		}

		/// <summary>
		/// Initializes a new instance of the Bytescout.PDF.Page class.
		/// </summary>
		/// <param name="format">The page size.</param>
		public Page(PaperFormat format) : this(format, (PaperOrientation) PaperOrientation.Portrait)
		{
		}

		internal Page(PDFDictionary dict, IDocumentEssential owner)
		{
			_dictionary = dict;
			_owner = owner;

			Watermarks = new WatermarkCollection();
		}

		/// <summary>
		/// Retrieves all text drawn on the page in plain text format.
		/// </summary>
		/// <returns cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">All text drawn on the page in plain text format.</returns>
		public string GetText()
		{
			try
			{
				return TextExtractor.ExtractText(_dictionary["Contents"], Resources);
			}
			catch
			{
				return "";
			}
		}

		/// <summary>
		/// Saves the page as a graphic template.
		/// </summary>
		/// <returns cref="GraphicsTemplate">The saved graphics template.</returns>
		public GraphicsTemplate SaveAsTemplate()
		{
			MemoryStream stream = new MemoryStream();
			stream.WriteByte((byte) 'q');
			stream.WriteByte((byte) '\n');

			byte[] buf = new byte[4096];
			IPDFObject contents = _dictionary["Contents"];
			if (contents is PDFDictionaryStream)
			{
				writeContents(contents as PDFDictionaryStream, buf, stream);
			}
			else if (contents is PDFArray)
			{
				PDFArray arr = contents as PDFArray;
				for (int i = 0; i < arr.Count; ++i)
				{
					writeContents(arr[i] as PDFDictionaryStream, buf, stream);
					stream.WriteByte((byte) '\n');
				}
			}

			return new GraphicsTemplate(stream, Resources.Clone() as Resources, Width, Height);
		}

		internal PDFDictionary GetDictionary()
		{
			return _dictionary;
		}

		internal Page Clone(IDocumentEssential owner)
		{
			if (_owner == null)
			{
				_owner = owner;
				if (_onClosed != null)
					_onClosed.ApplyOwner(owner);
				if (_onOpened != null)
					_onOpened.ApplyOwner(owner);

				if (_dictionary["Annots"] as PDFArray != null)
				{
					for (int i = 0; i < Annotations.Count; ++i)
					{
						Annotations[i].ApplyOwner(owner);
						if (Annotations[i] is Field)
							_owner.AddField(Annotations[i] as Field);
					}
				}

				return this;
			}

			PDFDictionary pageDictionary = PageBase.Copy(GetDictionary());
			Page page = new Page(pageDictionary, owner);
			if (_dictionary["Annots"] as PDFArray != null)
				page.Annotations = Annotations.Clone(owner, this);

			if (OnClosed != null)
				page.OnClosed = OnClosed;
			if (OnOpened != null)
				page.OnOpened = OnOpened;

			page._id = _id;
			return page;
		}

		private void addFirstq(PDFArray contents)
		{
			MemoryStream ms = new MemoryStream(2);
			ms.WriteByte((byte) 'q');
			ms.WriteByte((byte) '\r');
			PDFDictionaryStream q = new PDFDictionaryStream(new PDFDictionary(), ms);
			contents.Insert(0, q);
		}

		private void addLast(PDFArray contents)
		{
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			ms.WriteByte((byte) 'Q');
			ms.WriteByte((byte) '\r');
			ms.WriteByte((byte) 'q');
			ms.WriteByte((byte) '\r');
			ms.WriteByte((byte) 'Q');

			PDFDictionary dict = new PDFDictionary();
			PDFDictionaryStream ds = new PDFDictionaryStream(dict, ms);

			contents.AddItem(ds);
			_canvas = new Canvas(ms, Resources, PageRect);
			_canvas.ChangeGroup += new ChangeGroupEventHandler(m_canvas_ChangeGroup);
		}

		private void loadCanvas()
		{
			IPDFObject contents = _dictionary["Contents"];
			if (contents is PDFArray)
			{
				addFirstq(contents as PDFArray);
				addLast(contents as PDFArray);
			}
			else if (contents is PDFDictionaryStream)
			{
				PDFArray arr = new PDFArray();
				addFirstq(arr);
				arr.AddItem(contents);
				addLast(arr);
				_dictionary.AddItem("Contents", arr);
			}
			else
			{
				PDFArray arr = new PDFArray();
				PDFDictionaryStream ds = new PDFDictionaryStream();
				MemoryStream s = ds.GetStream();
				s.WriteByte((byte) 'q');
				s.WriteByte((byte) '\r');
				s.WriteByte((byte) 'Q');
				arr.AddItem(ds);
				_dictionary.AddItem("Contents", arr);
				_canvas = new Canvas(s, Resources, PageRect);
				_canvas.ChangeGroup += new ChangeGroupEventHandler(m_canvas_ChangeGroup);
			}
		}

		private void m_canvas_ChangeGroup(object sender, ChangeGroupEventArgs e)
		{
			Group.ColorSpace = e.Colorspace;
			Group.Isolated = true;
		}

		private void loadThumbnail()
		{
			PDFDictionaryStream ds = _dictionary["Thumb"] as PDFDictionaryStream;
			if (ds != null)
				_thumbnail = new Image(ds);
		}

		private void loadResources()
		{
			PDFDictionary dict = _dictionary["Resources"] as PDFDictionary;
			if (dict == null)
			{
				_resources = new Resources();
				_dictionary.AddItem("Resources", _resources.Dictionary);
			}
			else
			{
				_resources = new Resources(dict);
			}
		}

		private void loadAnnotations()
		{
			PDFArray annots = _dictionary["Annots"] as PDFArray;

			if (annots != null)
			{
				_annotations = new AnnotationCollections(annots, _owner, this);
			}
			else
			{
				_annotations = new AnnotationCollections(_owner, this);
				_dictionary.AddItem("Annots", _annotations.GetArray());
			}
		}

		private void loadGroup()
		{
			PDFDictionary group = _dictionary["Group"] as PDFDictionary;

			if (group != null)
			{
				_group = new PDFGroup(group);
			}
			else
			{
				_group = new PDFGroup();
				_dictionary.AddItem("Group", _group.GetDictionary());
			}
		}

		private void setPageWidth(float width)
		{
			RectangleF rect = PageRect;
			if (_dictionary["CropBox"] as PDFArray != null)
				_dictionary.AddItem("CropBox", createBox(rect.Left, rect.Top, width, rect.Height));
			_dictionary.AddItem("MediaBox", createBox(rect.Left, rect.Top, width, rect.Height));

			if (_canvas != null)
				_canvas.SetSize(PageRect);
		}

		private void setPageHeight(float height)
		{
			RectangleF rect = PageRect;
			if (_dictionary["CropBox"] as PDFArray != null)
				_dictionary.AddItem("CropBox", createBox(rect.Left, rect.Bottom - height, rect.Width, height));
			_dictionary.AddItem("MediaBox", createBox(rect.Left, rect.Bottom - height, rect.Width, height));

			if (_canvas != null)
				_canvas.SetSize(PageRect);
		}

		private static RectangleF getBox(PDFArray box)
		{
			if (box == null)
				return RectangleF.Empty;

			try
			{
				float x = (float) ((PDFNumber) box[0]).GetValue();
				float y = (float) ((PDFNumber) box[1]).GetValue();
				float right = (float) ((PDFNumber) box[2]).GetValue();
				float bottom = (float) ((PDFNumber) box[3]).GetValue();
				return new RectangleF(x, y, right - x, bottom - y);
			}
			catch
			{
				return RectangleF.Empty;
			}
		}

		private static PDFArray createBox(float left, float top, float width, float height)
		{
			PDFArray box = new PDFArray();
			box.AddItem(new PDFNumber(left));
			box.AddItem(new PDFNumber(top));
			box.AddItem(new PDFNumber(left + width));
			box.AddItem(new PDFNumber(top + height));
			return box;
		}

		private static PDFArray createBox(System.Drawing.RectangleF rect)
		{
			PDFArray box = new PDFArray();
			box.AddItem(new PDFNumber(rect.X));
			box.AddItem(new PDFNumber(rect.Y));
			box.AddItem(new PDFNumber(rect.Right));
			box.AddItem(new PDFNumber(rect.Bottom));
			return box;
		}

		private void loadAdditionalAction(string name)
		{
			PDFDictionary aa = _dictionary["AA"] as PDFDictionary;
			if (aa != null)
			{
				PDFDictionary dict = aa[name] as PDFDictionary;
				if (dict != null)
					setActionValue(Action.Create(dict, _owner), name);
			}
		}

		private void setAdditionalAction(Action action, string key)
		{
			if (action == null)
			{
				setActionValue(null, key);
				if (_dictionary["AA"] as PDFDictionary != null)
				{
					(_dictionary["AA"] as PDFDictionary).RemoveItem(key);
					if ((_dictionary["AA"] as PDFDictionary).Count == 0)
						_dictionary.RemoveItem("AA");
				}
			}
			else
			{
				Action a = action.Clone(_owner);
				setActionValue(a, key);
				if ((_dictionary["AA"] as PDFDictionary) == null)
					_dictionary.AddItem("AA", new PDFDictionary());
				(_dictionary["AA"] as PDFDictionary).AddItem(key, a.GetDictionary());
			}
		}

		private void setActionValue(Action action, string key)
		{
			switch (key)
			{
				case "O":
					_onOpened = action;
					break;
				case "C":
					_onClosed = action;
					break;
			}
		}

		private static float getMin(float a, float b)
		{
			if ((a >= 0 && b >= 0) || (a < 0 && b < 0))
				return Math.Max(a, b);
			if (a < 0)
				return b;
			return a;
		}

		private void writeContents(PDFDictionaryStream dictStream, byte[] buf, Stream output)
		{
			if (dictStream != null)
			{
				dictStream.Decode();
				Stream str = dictStream.GetStream();
				str.Position = 0;
				int numread;
				while ((numread = str.Read(buf, 0, buf.Length)) > 0)
					output.Write(buf, 0, numread);
			}
		}
    }

#if PDFSDK_EMBEDDED_SOURCES
	internal enum UnitOfMeasure
#else
	/// <summary>
	/// Specifies the unit of measure for the given data.
	/// </summary>
	public enum UnitOfMeasure
#endif
	{
		/// <summary>
		/// (0) Specifies a printer's point (1/72 inch) as the unit of measure. These are default units used in PDF documents.
		/// </summary>
		Point = 0,

		/// <summary>
		/// (1) Specifies the inch as the unit of measure.
		/// </summary>
		Inch = 1,

		/// <summary>
		/// (2) Specifies the millimeter as the unit of measure.
		/// </summary>
		Millimeter = 2,

		/// <summary>
		/// (3) Specifies the centimeter as the unit of measure.
		/// </summary>
		Centimeter = 3,

		/// <summary>
		/// (4) Specifies a device pixel at 96 DPI resolution as the unit of measure.
		/// </summary>
		Pixel96DPI = 4,

		/// <summary>
		/// (5) Specifies a device pixel at 120 DPI resolution as the unit of measure.
		/// </summary>
		Pixel120DPI = 5,

		/// <summary>
		/// (6) Specifies the Twip unit (1/20 inch) as the unit of measure.
		/// </summary>
		Twip = 6,

		/// <summary>
		/// (7) Specifies the document unit (1/300 inch) as the unit of measure.
		/// </summary>
		Document = 7,
	}
}
