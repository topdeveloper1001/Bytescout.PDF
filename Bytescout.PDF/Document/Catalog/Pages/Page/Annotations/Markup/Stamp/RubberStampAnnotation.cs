using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class RubberStampAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents a rubber stamp annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class RubberStampAnnotation: MarkupAnnotation
#endif
	{
	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.Stamp; } }

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
		    set { base.Width = value; }
	    }

	    /// <summary>
	    /// Gets or sets the height of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Height
	    {
		    get { return base.Height; }
		    set { base.Height = value; }
	    }

	    /// <summary>
	    /// Gets or sets the icon to be used in displaying the annotation.
	    /// </summary>
	    /// <value cref="RubberStampAnnotationIcon"></value>
	    public RubberStampAnnotationIcon Icon
	    {
		    get { return TypeConverter.PDFNameToPDFRubberStampAnnotationIcon(Dictionary["Name"] as PDFName); }
		    set
		    {
			    Dictionary.AddItem("Name", TypeConverter.PDFRubberStampAnnotationIconToPDFName(value));
			    CreateApperance();
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.RubberStampAnnotation class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        public RubberStampAnnotation(float left, float top, float width, float height)
            : base(left, top, width, height, null)
        {
            Dictionary.AddItem("Subtype", new PDFName("Stamp"));
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.RubberStampAnnotation class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        public RubberStampAnnotation(RectangleF boundingBox)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height) { }

        internal RubberStampAnnotation(PDFDictionary dict, IDocumentEssential owner)
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

            RubberStampAnnotation annot = new RubberStampAnnotation(res, owner);
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
