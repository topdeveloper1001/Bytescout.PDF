using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class SquareCircleAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents an abstract class for square and circle annotations.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class SquareCircleAnnotation: MarkupAnnotation
#endif
	{
	    private AnnotationBorderStyle _borderStyle;
	    private AnnotationBorderEffect _borderEffect;

	    /// <summary>
	    /// Gets or sets the x-coordinate of the left edge of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Left
	    {
		    get { return base.Left; }
		    set { base.Left = value; }
	    }

	    /// <summary>
	    /// Gets or sets the y-coordinate of the top edge of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Top
	    {
		    get { return base.Top; }
		    set { base.Top = value; }
	    }

	    /// <summary>
	    /// Gets or sets the width of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Width
	    {
		    get { return base.Width; }
		    set
		    {
			    base.Width = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the height of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Height
	    {
		    get { return base.Height; }
		    set
		    {
			    base.Height = value;
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
	    public AnnotationBorderStyle BorderStyle
	    {
		    get
		    {
			    if (_borderStyle == null)
				    loadBorderStyle();
			    return _borderStyle;
		    }
	    }

	    /// <summary>
	    /// Gets the border effect options for the annotation.
	    /// </summary>
	    /// <value cref="AnnotationBorderEffect"></value>
	    public AnnotationBorderEffect BorderEffect
	    {
		    get
		    {
			    if (_borderEffect == null)
				    loadBorderEffect();
			    return _borderEffect;
		    }
	    }

	    /// <summary>
	    /// Gets or sets the annotation's inner rectangle.
	    /// This property is used when the border effect (described by Bytescout.PDF.AnnotationBorderEffect) is used to increase the size of the annotation for anything in a square or a circle.
	    /// </summary>
	    /// <value cref="RectangleF" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx"></value>
		[ComVisible(false)]
	    public RectangleF InnerRectangle
	    { 
		    get
		    {
			    PDFArray rect = Dictionary["RD"] as PDFArray;
			    if (rect != null)
			    {
				    try
				    {
						float x1 = (float) ((PDFNumber) rect[0]).GetValue();
						float y1 = (float) ((PDFNumber) rect[1]).GetValue();
						float x2 = (float) ((PDFNumber) rect[2]).GetValue();
						float y2 = (float) ((PDFNumber) rect[3]).GetValue();

					    return new RectangleF(x1, Height - y1, Width - x2 - x1, y2 - (Height - y1));
				    }
				    catch
				    {
					    // ignored
				    }
			    }
			    return new RectangleF(0, 0, Width, Height);
		    }
		    set
		    {
			    PDFArray rect = new PDFArray();
			    rect.AddItem(new PDFNumber(value.Left));
			    rect.AddItem(new PDFNumber(Height - value.Top));
			    rect.AddItem(new PDFNumber(Width - (value.Left + value.Width)));
			    rect.AddItem(new PDFNumber(value.Bottom));
			    Dictionary.AddItem("RD", rect);
			    CreateApperance();
		    }
	    }

	    internal SquareCircleAnnotation(float left, float top, float width, float height, IDocumentEssential owner)
            : base(left, top, width, height, owner)
        {
            Color = new ColorRGB(0, 0, 255);
        }

        internal SquareCircleAnnotation(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

	    internal static void CopyTo(PDFDictionary sourceDict, PDFDictionary destinationDict)
        {
            string[] keys = { "IC", "BE", "RD" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = sourceDict[keys[i]];
                if (obj != null)
                    destinationDict.AddItem(keys[i], obj.Clone());
            }

            PDFDictionary bs = sourceDict["BS"] as PDFDictionary;
            if (bs != null)
                destinationDict.AddItem("BS", AnnotationBorderStyle.Copy(bs));
        }

        private void loadBorderStyle()
        {
            PDFDictionary bs = Dictionary["BS"] as PDFDictionary;
            if (bs != null)
            {
                _borderStyle = new AnnotationBorderStyle(bs);
            }
            else
            {
                _borderStyle = new AnnotationBorderStyle();
                Dictionary.AddItem("BS", _borderStyle.GetDictionary());
            }
            _borderStyle.ChangedBorderStyle += changedBorderStyle;
        }

        private void loadBorderEffect()
        {
            PDFDictionary be = Dictionary["BE"] as PDFDictionary;
            if (be != null)
            {
                _borderEffect = new AnnotationBorderEffect(be);
            }
            else
            {
                _borderEffect = new AnnotationBorderEffect();
                Dictionary.AddItem("BE", _borderEffect.GetDictionary());
            }
            _borderEffect.ChangedBorderEffect += changedBorderEffect;
        }

        private void changedBorderEffect(object sender)
        {
            CreateApperance();
        }

        private void changedBorderStyle(object sender)
        {
            CreateApperance();
        }
    }
}
