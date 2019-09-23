using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class PolygonAnnotation : PolygonPolylineAnnotation
#else
	/// <summary>
    /// Represents a polygon annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class PolygonAnnotation : PolygonPolylineAnnotation
#endif
	{
	    private AnnotationBorderEffect _borderEffect;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.Polygon; } }

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
        /// Initializes a new instance of the Bytescout.PDF.PolygonAnnotation class.
        /// </summary>
        /// <param name="points">The Bytescout.PDF.PointsArray that represents the vertices of the polygon.</param>
        public PolygonAnnotation(PointsArray points)
            : base(points, null)
        {
            Dictionary.AddItem("Subtype", new PDFName("Polygon"));
        }

        internal PolygonAnnotation(PDFDictionary dict, IDocumentEssential owner)
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

            PolygonAnnotation annot = new PolygonAnnotation(res, owner);
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

        private void loadBorderEffect()
        {
            PDFDictionary dict = Dictionary["BE"] as PDFDictionary;
            if (dict == null)
            {
                _borderEffect = new AnnotationBorderEffect();
                Dictionary.AddItem("BE", _borderEffect.GetDictionary());
            }
            else
            {
                _borderEffect = new AnnotationBorderEffect(dict);
            }
            _borderEffect.ChangedBorderEffect += new ChangedBorderEffectEventHandler(changedBorderEffect);
        }

        private void changedBorderEffect(object sender)
        {
            CreateApperance();
        }
    }
}
