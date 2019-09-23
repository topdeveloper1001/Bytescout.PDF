using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class LineAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents a line annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class LineAnnotation: MarkupAnnotation
#endif
	{
	    private EndingStyles _endingStyles;
	    private AnnotationBorderStyle _borderStyle;
	    private PointsArray _coordinates;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.Line; } }

	    /// <summary>
	    /// Gets or sets the start point of this Bytescout.PDF.LineAnnotation.
	    /// </summary>
	    /// <value cref="PointF" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx"></value>
	    public PointF StartPoint
	    {
		    get
		    {
			    if (_coordinates == null)
				    initCoordinates();
			
				return _coordinates[0];
		    }
		    set
		    {
			    if (_coordinates == null)
				    initCoordinates();

			    _coordinates.RemovePoint(0);
			    _coordinates.Insert(0, value);
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the end point of this Bytescout.PDF.LineAnnotation.
	    /// </summary>
	    /// <value cref="PointF" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx"></value>
	    public PointF EndPoint
	    {
		    get
		    {
			    if (_coordinates == null)
			    {
				    initCoordinates();
			    }
			    return _coordinates[1];
		    }
		    set
		    {
			    if (_coordinates == null)
			    {
				    initCoordinates();
			    }
			    _coordinates.RemovePoint(1);
			    _coordinates.AddPoint(value);
			    CreateApperance();
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
			    if (Dictionary["IC"] == null)
				    return null;
			    return TypeConverter.PDFArrayToPDFColor(Dictionary["IC"] as PDFArray);
		    }
		    set
		    {
			    if (value == null)
				    Dictionary.RemoveItem("IC");
			    else
				    Dictionary.AddItem("IC", TypeConverter.PDFColorToPDFArray(value));
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets the border style options for the annotation.
	    /// </summary>
	    /// <value cref="AnnotationBorderStyle"></value>
	    public AnnotationBorderStyle LineStyle
	    {
		    get
		    {
			    if (_borderStyle == null)
			    {
				    PDFDictionary dict = Dictionary["BS"] as PDFDictionary;
				    if (dict == null)
				    {
					    _borderStyle = new AnnotationBorderStyle();
					    Dictionary.AddItem("BS", _borderStyle.GetDictionary());
				    }
				    else
				    {
					    _borderStyle = new AnnotationBorderStyle(dict);
				    }
				    _borderStyle.ChangedBorderStyle += new ChangedBorderStyleEventHandler(changedBorderStyle);
			    }
			    return _borderStyle;
		    }
	    }

	    /// <summary>
	    /// Gets or sets the style used for the beginning of the line.
	    /// </summary>
	    /// <value cref="LineEndingStyle"></value>
	    public LineEndingStyle StartLineStyle
	    {
		    get { return EndingStyles.Start; }
		    set
		    {
			    EndingStyles.Start = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the style used for the end of the line.
	    /// </summary>
	    /// <value cref="LineEndingStyle"></value>
	    public LineEndingStyle EndLineStyle
	    {
		    get { return EndingStyles.End; }
		    set
		    {
			    EndingStyles.End = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets whether the line annotation caption should be displayed.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Caption
	    {
		    get
		    {
			    PDFBoolean cap = Dictionary["Cap"] as PDFBoolean;
			    if (cap == null)
				    return false;
			    return cap.GetValue();
		    }
		    set
		    {
			    Dictionary.AddItem("Cap", new PDFBoolean(value));
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the line caption text type.
	    /// <remarks>Meaningful only if Caption is true.</remarks>
	    /// </summary>
	    /// <value cref="LineCaptionType"></value>
	    public LineCaptionType CaptionType
	    {
		    get
		    {
			    PDFName name = Dictionary["CP"] as PDFName;
			    if (name == null)
				    return LineCaptionType.Inline;
			    return getOffsetFromString(name.GetValue());
		    }
		    set
		    {
			    Dictionary.AddItem("CP", new PDFName(value.ToString()));
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the offset of the caption text from its normal position.
	    /// The horizontal offset along the annotation line from its midpoint, with a positive value indicating
	    /// offset to the right and a negative value indicating offset to the left.
	    /// The vertical offset perpendicular to the annotation line, with a positive value indicating a
	    /// shift up and a negative value indicating a shift down.
	    /// <remarks>Meaningful only if Caption is true.</remarks>
	    /// </summary>
	    /// <value cref="PointF" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx"></value>
	    public PointF CaptionOffset
	    {
		    get
		    {
			    PDFArray array = Dictionary["CO"] as PDFArray;
			    if (array == null)
				    return new PointF(0, 0);
			    if (array.Count != 2)
			    {
				    array.Clear();
				    array.AddItem(new PDFNumber(0));
				    array.AddItem(new PDFNumber(0));
				    return new PointF(0, 0);
			    }

			    PDFNumber x = array[0] as PDFNumber;
			    PDFNumber y = array[1] as PDFNumber;
			    if (x == null || y == null)
			    {
				    array.Clear();
				    array.AddItem(new PDFNumber(0));
				    array.AddItem(new PDFNumber(0));
				    return new PointF(0, 0);
			    }
			    return new PointF((float)x.GetValue(), (float)y.GetValue());
		    }
		    set
		    {
			    PDFArray array = new PDFArray();
			    array.AddItem(new PDFNumber(value.X));
			    array.AddItem(new PDFNumber(value.Y));
			    Dictionary.AddItem("CO", array);
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the length of leader lines that extend from each endpoint of the line
	    /// perpendicular to the line itself. A positive value means that the leader lines appear
	    /// in the direction that is clockwise when traversing the line from its starting point
	    /// to its ending point; a negative value indicates the opposite direction.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float LeaderLineLength
	    {
		    get
		    {
			    PDFNumber number = Dictionary["LL"] as PDFNumber;
			    if (number == null)
				    return 0;
			    return (float)number.GetValue();
		    }
		    set
		    {
			    Dictionary.AddItem("LL", new PDFNumber(value));
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or set a non-negative number representing the length of leader line extensions that
	    /// extend from the line proper 180 degrees from the leader lines.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float LeaderLineExtension
	    {
		    get
		    {
			    PDFNumber number = Dictionary["LLE"] as PDFNumber;
			    if (number == null)
				    return 0;
			    return (float)number.GetValue();
		    }
		    set
		    {
			    if (value < 0)
				    throw new ArgumentException();
			    Dictionary.AddItem("LLE", new PDFNumber(value));
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets a non-negative number representing the length of the leader line offset, which is
	    /// the amount of empty space between the endpoints of the annotation and the beginning of the leader lines.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float LeaderLineOffset
	    {
		    get
		    {
			    PDFNumber llo = Dictionary["LLO"] as PDFNumber;
			    if (llo == null)
				    return 0;
			    return (float)llo.GetValue();
		    }
		    set
		    {
			    if (value < 0)
				    throw new ArgumentException();
			    Dictionary.AddItem("LLO", new PDFNumber(value));
			    CreateApperance();
		    }
	    }

	    internal EndingStyles EndingStyles
	    {
		    get
		    {
			    if (_endingStyles == null)
			    {
				    PDFArray array = Dictionary["LE"] as PDFArray;
				    if (array == null)
				    {
					    _endingStyles = new EndingStyles();
					    Dictionary.AddItem("LE", _endingStyles.Array);
				    }
				    else
				    {
					    _endingStyles = new EndingStyles(array);
				    }
			    }

			    return _endingStyles;
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.LineAnnotation class.
        /// </summary>
        /// <param name="x1" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the first point.</param>
        /// <param name="y1" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the first point.</param>
        /// <param name="x2" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the second point.</param>
        /// <param name="y2" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the second point.</param>
        public LineAnnotation(float x1, float y1, float x2, float y2)
            : base (null)
        {
            Dictionary.AddItem("Subtype", new PDFName("Line"));
            _coordinates = new PointsArray();
            _coordinates.AddPoint(new PointF(x1, y1));
            _coordinates.AddPoint(new PointF(x2, y2));
            Dictionary.AddItem("L", _coordinates.Array);
            Color = new ColorRGB(0, 0, 0);
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.LineAnnotation class.
        /// </summary>
        /// <param name="pt1" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The first point.</param>
        /// <param name="pt2" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The second point.</param>
        public LineAnnotation(PointF pt1, PointF pt2)
            : this(pt1.X, pt1.Y, pt2.X, pt2.Y) { }

        internal LineAnnotation(PDFDictionary dictionary, IDocumentEssential owner)
            : base(dictionary, owner) { }

	    //TODO: Measure
        //TODO: IT

        internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                if (_coordinates == null)
                    initCoordinates();

                PointF[] points = _coordinates.ToArray();
                ApplyOwner(owner);
                SetPage(page, true);

                _coordinates.Clear();
                _coordinates.Page = page;
                _coordinates.AddRange(points);

                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);
            MarkupAnnotationBase.CopyTo(Dictionary, res);

            string[] keys = { "LE", "IC", "LL", "LLE", "Cap", "IT", "LLO", "CP", "CO" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = Dictionary[keys[i]];
                if (obj != null)
                    res.AddItem(keys[i], obj.Clone());
            }

            PDFArray l = Dictionary["L"] as PDFArray;
            if (l != null)
            {
                RectangleF oldRect;
                if (Page == null)
                    oldRect = new RectangleF();
                else
                    oldRect = Page.PageRect;

                res.AddItem("L", CloneUtility.CopyArrayCoordinates(l, oldRect, page.PageRect, Page == null));
            }

            PDFDictionary bs = Dictionary["BS"] as PDFDictionary;
            if (bs != null)
                res.AddItem("BS", AnnotationBorderStyle.Copy(bs));
            
            PDFDictionary measure = Dictionary["Measure"] as PDFDictionary;
            if (measure != null)
                res.AddItem("Measure", Measure.Copy(measure));

            LineAnnotation annot = new LineAnnotation(res, owner);
            annot.SetPage(page, true);

            return annot;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            Owner = owner;
        }

        internal override void CreateApperance()
        {
            Dictionary.RemoveItem("AP");
        }

        private LineCaptionType getOffsetFromString(string str)
        {
            switch (str)
            { 
                case "Inline":
                    return LineCaptionType.Inline;
                case "Top":
                    return LineCaptionType.Top;
            }
            return LineCaptionType.Inline;
        }

        private void initCoordinates()
        {
            _coordinates = new PointsArray();
            PDFArray array = Dictionary["L"] as PDFArray;
            if (array == null)
            {
                _coordinates.AddPoint(new PointF(0, 0));
                _coordinates.AddPoint(new PointF(0, 0));
            }
            else
            {
                if (array.Count != 2)
                {
                    _coordinates.AddPoint(new PointF(0, 0));
                    _coordinates.AddPoint(new PointF(0, 0));
                    Dictionary.AddItem("L", _coordinates.Array);
                }
                else
                {
                    _coordinates = new PointsArray(array, Page);
                }
            }
        }

        private void changedBorderStyle(object sender)
        {
            CreateApperance();
        }
    }
}
