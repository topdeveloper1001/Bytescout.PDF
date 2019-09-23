using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class TextAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents a text annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class TextAnnotation: MarkupAnnotation
#endif
	{
	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.Text; } }

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
	    /// Gets or sets the value indicating whether the annotation should initially be displayed open.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Open
	    {
		    get
		    {
			    PDFBoolean val = Dictionary["Open"] as PDFBoolean;
			    if (val == null)
				    return false;
			    return val.GetValue();
		    }
		    set
		    {
			    Dictionary.AddItem("Open", new PDFBoolean(value));
		    }
	    }

	    /// <summary>
	    /// Gets or sets the icon to be used in displaying the annotation.
	    /// </summary>
	    /// <value cref="TextAnnotationIcon"></value>
	    public TextAnnotationIcon Icon
	    {
		    get { return TypeConverter.PDFNameToPDFTextAnnotationIcon(Dictionary["Name"] as PDFName); }
		    set
		    {
			    Dictionary.AddItem("Name", TypeConverter.PDFTextAnnotationIconToPDFName(value));
			    CreateApperance();
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.TextAnnotation class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        public TextAnnotation(float left, float top)
            :base(left, top, 1, 1, null)
        {
            Dictionary.AddItem("Subtype", new PDFName("Text"));
            Color = new ColorRGB(255, 255, 0);
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.TextAnnotation class.
        /// </summary>
        /// <param name="location" href="http://msdn.microsoft.com/en-us/library/system.drawing.pointf.aspx">The location of the annotation.</param>
        public TextAnnotation(PointF location)
            : this(location.X, location.Y) { }

        internal TextAnnotation(PDFDictionary dict, IDocumentEssential owner)
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

            IPDFObject name = Dictionary["Name"];
            if (name != null)
                res.AddItem("Name", name.Clone());

            TextAnnotation annot = new TextAnnotation(res, owner);
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
