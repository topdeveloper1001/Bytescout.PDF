using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class InkAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents an ink annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class InkAnnotation: MarkupAnnotation
#endif
	{
	    private AnnotationBorderStyle _borderStyle;
	    private InkList _inkList;

	    /// <summary>
        /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
        /// </summary>
        /// <value cref="AnnotationType"></value>
        public override AnnotationType Type { get { return AnnotationType.Ink; } }

        /// <summary>
        /// Gets Bytescout.PDF.InkList value that representing a stroked path of this annotation.
        /// </summary>
        /// <value cref="PDF.InkList"></value>
        public InkList InkList
        {
            get
            {
                if (_inkList == null)
                {
                    PDFArray array = Dictionary["InkList"] as PDFArray;
                    if (array == null)
                    {
                        array = new PDFArray();
                        Dictionary.AddItem("InkList", array);
                    }
                    _inkList = new InkList(array, Page);
                    _inkList.ChangedInkList += changedInkList;
                }
                return _inkList;
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
	    /// Initializes a new instance of the Bytescout.PDF.InkAnnotation class.
	    /// </summary>
	    /// <param name="inkList">The Bytescout.PDF.InkList representing a stroked path.</param>
	    public InkAnnotation(InkList inkList)
		    : base(null)
	    {
		    if (inkList == null)
			    throw new ArgumentNullException();

		    if (inkList.Page != null)
		    {
			    _inkList = new InkList();
			    for (int i = 0; i < inkList.Count; ++i)
				    _inkList.AddArray(new PointsArray(inkList[i].ToArray()));
		    }
		    else
			    _inkList = inkList;
		    _inkList.ChangedInkList += changedInkList;

		    Dictionary.AddItem("Subtype", new PDFName("Ink"));
		    Dictionary.AddItem("InkList", _inkList.Array);
		    _borderStyle = new AnnotationBorderStyle();
		    Dictionary.AddItem("BS", _borderStyle.GetDictionary());
		    Color = new ColorRGB(0, 0, 0);
	    }

	    internal InkAnnotation(PDFDictionary dict, IDocumentEssential owner)
		    : base(dict, owner) { }

	    internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                InkList lst = InkList;

                ApplyOwner(owner);
                SetPage(page, true);

                for (int i = 0; i < lst.Count; ++i)
                {
                    PointF[] points = lst[i].ToArray();
                    lst[i].Clear();
                    lst[i].Page = page;
                    lst[i].AddRange(points);
                }
                lst.Page = page;

                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);
            MarkupAnnotationBase.CopyTo(Dictionary, res);

            PDFDictionary bs = Dictionary["BS"] as PDFDictionary;
            if (bs != null)
                res.AddItem("BS", AnnotationBorderStyle.Copy(bs));

            PDFArray inkList = Dictionary["InkList"] as PDFArray;
            if (inkList != null)
            {
                PDFArray newInkList = new PDFArray();
                for (int i = 0; i < inkList.Count; ++i)
                {
                    PDFArray points = inkList[i] as PDFArray;
                    if (points != null)
                    {
                        RectangleF oldRect;
                        if (Page == null)
                            oldRect = new RectangleF();
                        else
                            oldRect = Page.PageRect;

                        newInkList.AddItem(CloneUtility.CopyArrayCoordinates(points, oldRect, page.PageRect, Page == null));
                    }
                }
                res.AddItem("InkList", newInkList);
            }

            InkAnnotation annot = new InkAnnotation(res, owner);
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
            _borderStyle.ChangedBorderStyle += new ChangedBorderStyleEventHandler(changedBorderStyle);
        }

        private void changedBorderStyle(object sender)
        {
            CreateApperance();
        }

        private void changedInkList(object sender)
        {
            CreateApperance();
        }
    }
}
