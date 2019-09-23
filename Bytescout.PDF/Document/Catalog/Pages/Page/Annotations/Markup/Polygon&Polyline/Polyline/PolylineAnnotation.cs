using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class PolylineAnnotation: PolygonPolylineAnnotation
#else
	/// <summary>
    /// Represents a polyline annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class PolylineAnnotation: PolygonPolylineAnnotation
#endif
	{
	    private EndingStyles _endingStyles;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.PolyLine; } }

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
        /// Initializes a new instance of the Bytescout.PDF.PolylineAnnotation class.
        /// </summary>
        /// <param name="points">The Bytescout.PDF.PointsArray that represents the vertices of the polyline.</param>
        public PolylineAnnotation(PointsArray points)
            : base (points, null)
        {
            Dictionary.AddItem("Subtype", new PDFName("PolyLine"));
        }

        internal PolylineAnnotation(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

	    //TODO : IT

        internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                PointF[] points = Vertices.ToArray();
                ApplyOwner(owner);
                SetPage(page, true);

                Vertices.Clear();
                Vertices.Page = page;
                Vertices.AddRange(points);

                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);
            MarkupAnnotationBase.CopyTo(Dictionary, res);
            PolygonPolylineAnnotation.CopyTo(Dictionary, res, Page, page);

            PolylineAnnotation annot = new PolylineAnnotation(res, owner);
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
