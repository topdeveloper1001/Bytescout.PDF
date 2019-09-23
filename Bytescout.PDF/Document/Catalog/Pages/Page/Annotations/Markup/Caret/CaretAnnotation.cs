using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class CaretAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents a caret annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class CaretAnnotation: MarkupAnnotation
#endif
	{
	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.Caret; } }

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
	    /// Gets or sets annotation's inner rectangle.
	    /// This property is used when a paragraph symbol specified by CaretAnnotation.Symbol
	    /// property is displayed along with the caret.
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
					    float x1 = (float)((PDFNumber) rect[0]).GetValue();
					    float y1 = (float)((PDFNumber) rect[1]).GetValue();
					    float x2 = (float)((PDFNumber) rect[2]).GetValue();
					    float y2 = (float)((PDFNumber) rect[3]).GetValue();

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

	    /// <summary>
	    /// Gets or sets a value specifying a symbol to be associated with the caret.
	    /// </summary>
	    /// <value cref="CaretSymbol"></value>
	    public CaretSymbol Symbol
	    {
		    get { return TypeConverter.PDFNameToPDFCaretSymbol(Dictionary["Sy"] as PDFName); }
		    set
		    {
			    Dictionary.AddItem("Sy", TypeConverter.PDFCaretSymbolToPDFName(value));
			    CreateApperance();
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.CaretAnnotation class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        public CaretAnnotation(float left, float top, float width, float height)
            : base(left, top, width, height, null)
        {
            Dictionary.AddItem("Subtype", new PDFName("Caret"));
            Color = new ColorRGB(0, 0, 255);
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.CaretAnnotation class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        public CaretAnnotation(RectangleF boundingBox)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height) { }

        internal CaretAnnotation(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

	    internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                ApplyOwner(owner);
                SetPage(page, true);
                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);
            MarkupAnnotationBase.CopyTo(Dictionary, res);

            string[] keys = { "RD", "Sy" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = Dictionary[keys[i]];
                if (obj != null)
                    res.AddItem(keys[i], obj.Clone());
            }

            CaretAnnotation annot = new CaretAnnotation(res, owner);
            annot.SetPage(Page, false);
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
    }
}
