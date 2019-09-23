using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class LinkAnnotation: Annotation
#else
	/// <summary>
    /// Represents a link annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class LinkAnnotation: Annotation
#endif
	{
	    private Destination _destination;
	    private Action _action;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.Link; } }

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
	    /// Gets or sets the color of this annotation.
	    /// </summary>
	    /// <value cref="DeviceColor"></value>
	    public DeviceColor Color
	    {
		    get
		    {
			    if (Dictionary["C"] == null)
				    return null;
			    return TypeConverter.PDFArrayToPDFColor(Dictionary["C"] as PDFArray);
		    }
		    set
		    {
			    if (value == null)
				    Dictionary.RemoveItem("C");
			    else
				    Dictionary.AddItem("C", TypeConverter.PDFColorToPDFArray(value));
		    }
	    }

	    /// <summary>
	    /// Gets or sets the annotation destination to jump to.
	    /// <remarks>If this value is set, the Action property will be ignored.</remarks>
	    /// </summary>
	    /// <value cref="PDF.Destination"></value>
	    public Destination Destination
	    {
		    get { return _destination; }
		    set { setDestination(value); }
	    }

	    /// <summary>
	    /// Gets or sets the action for this outline.
	    /// <remarks>If this value is set, the Destination property will be ignored.</remarks>
	    /// </summary>
	    /// <value cref="PDF.Action"></value>
	    public Action Action
	    {
		    get { return _action; }
		    set { setAction(value); }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating which visual effect is to be used when the mouse button is pressed or held down inside its active area.
	    /// </summary>
	    /// <value cref="LinkAnnotationHighlightingMode"></value>
	    public LinkAnnotationHighlightingMode HighlightingMode
	    {
		    get { return TypeConverter.PDFNameToPDFLinkAnnotationHighlightingMode(Dictionary["H"] as PDFName); }
		    set { Dictionary.AddItem("H", TypeConverter.PDFLinkAnnotationHighlightingModeToPDFName(value)); }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.LinkAnnotation class.
        /// </summary>
        /// <param name="destination">The destination to jump to.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the link area.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the link area.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the link area.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the link area.</param>
        public LinkAnnotation(Destination destination, float left, float top, float width, float height)
            : base(left, top, width, height, null)
        {
            if (destination == null)
                throw new ArgumentNullException("destination");

            Dictionary.AddItem("Subtype", new PDFName("Link"));
            Destination = destination;
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.LinkAnnotation class.
        /// </summary>
        /// <param name="action">The action to be performed when this item is activated.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the link area.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the link area.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the link area.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the link area.</param>
        public LinkAnnotation(Action action, float left, float top, float width, float height)
            : base(left, top, width, height, null)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            Dictionary.AddItem("Subtype", new PDFName("Link"));
            Action = action;
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.LinkAnnotation class.
        /// </summary>
        /// <param name="destination">The destination to jump to.</param>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the link area.</param>
        public LinkAnnotation(Destination destination, RectangleF boundingBox)
            : this(destination, boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height) { }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.LinkAnnotation class.
        /// </summary>
        /// <param name="action">The action to be performed when this item is activated.</param>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the link area.</param>
        public LinkAnnotation(Action action, RectangleF boundingBox)
            : this(action, boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height) { }

        internal LinkAnnotation(PDFDictionary dict, IDocumentEssential owner)
            :base(dict, owner)
        {
            loadDestination();
            loadAction();
        }

	    internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                ApplyOwner(owner);
                SetPage(page, true);
                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);
            IPDFObject h = Dictionary["H"];
            if (h != null)
                res.AddItem("H", h.Clone());

            PDFArray quadPoints = Dictionary["QuadPoints"] as PDFArray;
            if (quadPoints != null)
            {
                RectangleF oldRect;
                if (Page == null)
                    oldRect = new RectangleF();
                else
                    oldRect = Page.PageRect;

                res.AddItem("QuadPoints", CloneUtility.CopyArrayCoordinates(quadPoints, oldRect, page.PageRect, Page == null));
            }

            IPDFObject pa = Dictionary["PA"];
            if (pa != null)
                res.AddItem("PA", pa.Clone());

            LinkAnnotation annot = new LinkAnnotation(res, owner);
            if (_destination != null)
                annot.Destination = Destination;
            if (_action != null)
                annot.Action = _action;

            annot.SetPage(Page, false);
            annot.SetPage(page, true);
            return annot;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            Owner = owner;
            if (_action != null)
                _action.ApplyOwner(owner);
        }

        internal override void CreateApperance()
        {
        }

        private void setDestination(Destination dest)
        {
            if (dest == null)
            {
                Dictionary.RemoveItem("Dest");
                return;
            }
            
            _destination = dest;
            Dictionary.AddItem("Dest", _destination.GetArray());
            Action = null;
        }

        private void setAction(Action action)
        {
            if (action == null)
            {
                _action = null;
                Dictionary.RemoveItem("A");
                return;
            }

            _action = action.Clone(Owner);
            Dictionary.AddItem("A", _action.GetDictionary());
            Destination = null;
        }

        private void loadDestination()
        {
            IPDFObject dest = Dictionary["Dest"];
            if (dest == null)
                return;
            _destination = new Destination(dest, Owner);
        }

        private void loadAction()
        {
            PDFDictionary dict = Dictionary["A"] as PDFDictionary;
            if (dict == null)
                return;
            try
            {
                _action = Action.Create(dict, Owner);
            }
            catch (PDFException)
            {
                Dictionary.RemoveItem("A");
            }
        }
    }
}
