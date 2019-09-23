using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class HighlightAnnotation: TextMarkupAnnotation
#else
	/// <summary>
    /// Represents a highlight annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class HighlightAnnotation: TextMarkupAnnotation
#endif
	{
	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.Highlight; } }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.HighlightAnnotation class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        /// <param name="angle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The angle, in degrees, to rotate the annotation by.</param>
        public HighlightAnnotation(float left, float top, float width, float height, float angle)
            : base(new RotatingRectangle(left, top, width, height, angle), null)
        {
            Dictionary.AddItem("Subtype", new PDFName("Highlight"));
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.HighlightAnnotation class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        public HighlightAnnotation(float left, float top, float width, float height)
            : this(left, top, width, height, 0) { }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.HighlightAnnotation class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        /// <param name="angle" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The angle, in degrees, to rotate the annotation by.</param>
        public HighlightAnnotation(RectangleF boundingBox, float angle)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height, angle) { }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.HighlightAnnotation class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        public HighlightAnnotation(RectangleF boundingBox)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height, 0) { }

        internal HighlightAnnotation(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

	    internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                ApplyOwner(owner);
                SetPage(page, true);

                float l = Left;
                float t = Top;
                float w = Width;
                float h = Height;
                float a = RotationAngle;

                Rectangle = null;
                Rectangle.Left = l;
                Rectangle.Top = t;
                Rectangle.Width = w;
                Rectangle.Height = h;
                Rectangle.Angle = a;

                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);
            MarkupAnnotationBase.CopyTo(Dictionary, res);
            TextMarkupAnnotation.CopyTo(Dictionary, res, Page, page);

            HighlightAnnotation annot = new HighlightAnnotation(res, owner);
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
