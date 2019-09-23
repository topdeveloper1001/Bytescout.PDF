using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class SquareAnnotation: SquareCircleAnnotation
#else
	/// <summary>
    /// Represents a square annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class SquareAnnotation: SquareCircleAnnotation
#endif
	{
	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.Square; } }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.SquareAnnotation class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        public SquareAnnotation(float left, float top, float width, float height)
            : base(left, top, width, height, null)
        {
            Dictionary.AddItem("Subtype", new PDFName("Square"));
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.SquareAnnotation class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        public SquareAnnotation(RectangleF boundingBox)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height) { }

        internal SquareAnnotation(PDFDictionary dict, IDocumentEssential owner)
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
            SquareCircleAnnotation.CopyTo(Dictionary, res);

            SquareAnnotation annot = new SquareAnnotation(res, owner);
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
