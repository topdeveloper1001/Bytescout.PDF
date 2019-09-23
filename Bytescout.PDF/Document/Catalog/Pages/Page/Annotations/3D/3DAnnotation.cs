using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ThreeDAnnotation : Annotation
#else
	/// <summary>
    /// Represents a 3D annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class ThreeDAnnotation : Annotation
#endif
	{
	    private readonly ThreeDData _data;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.U3D; } }

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
	    /// Gets the data that specifies the 3D artwork to be shown.
	    /// </summary>
	    /// <value cref="ThreeDData"></value>
	    public ThreeDData Data
	    {
		    get
		    {
			    return _data;
		    }
	    }

		//TODO
		/*public PDF3DViews Views
		{
			get
			{
				if (m_views == null)
				{
					PDFDictionary dict = Dictionary["3DV"] as PDFDictionary;
					if (dict == null)
						m_views = new PDF3DViews();
					else
						m_views = new PDF3DViews(dict);
				}
				return m_views;
			}
		}*/

		//TODO
		/*public ThreeDActivation Activation
		{
			get
			{
				if (m_activation == null)
				{
					PDFDictionary dict = Dictionary["3DA"] as PDFDictionary;
					if (dict == null)
						m_activation = new ThreeDActivation();
					else
						m_activation = new ThreeDActivation(dict);
				}
				return m_activation;
			}
		}*/

	    internal bool Indicating
	    {
		    get
		    {
			    PDFBoolean b = Dictionary["3DI"] as PDFBoolean;
			    if (b == null)
				    return true;
			    return b.GetValue();
		    }
		    set
		    {
			    Dictionary.AddItem("3DI", new PDFBoolean(value));
		    }
	    }
        
	    internal RectangleF ViewBox
	    {
		    get
		    {
			    PDFArray array = Dictionary["3DB"] as PDFArray;
			    if (array == null)
				    return new RectangleF(-Width / 2, -Height / 2, Width / 2, Height / 2);
			    float[] coord = { 0, 0, 0, 0 };
			    for (int i = 0; i < 4; ++i)
			    {
				    PDFNumber number = array[i] as PDFNumber;
				    if (number != null)
					    coord[i] = (float)number.GetValue();
			    }
			    return new RectangleF(coord[0], coord[1], coord[2], coord[3]);
		    }
		    set
		    {
			    PDFArray array = new PDFArray();
			    array.AddItem(new PDFNumber(value.Left));
			    array.AddItem(new PDFNumber(value.Top));
			    array.AddItem(new PDFNumber(value.Right));
			    array.AddItem(new PDFNumber(value.Bottom));
			    Dictionary.AddItem("3DB", array);
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.ThreeDAnnotation class.
        /// </summary>
        /// <param name="data">The 3D data.</param>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        public ThreeDAnnotation(ThreeDData data, float left, float top, float width, float height)
            : base(left, top, width, height, null)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            Dictionary.AddItem("Subtype", new PDFName("3D"));
            _data = data;
            Dictionary.AddItem("3DD", _data.GetDictionary());
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.ThreeDAnnotation class.
        /// </summary>
        /// <param name="data">The 3D data.</param>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        public ThreeDAnnotation(ThreeDData data, RectangleF boundingBox)
            : this(data, boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height) { }

        internal ThreeDAnnotation(PDFDictionary dict, IDocumentEssential owner)
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

            IPDFObject threeDD = Dictionary["3DD"];
            if (threeDD != null)
                res.AddItem("3DD", ThreeDData.Copy(threeDD));

            string[] keys = { "3DV", "3DA", "3DI", "3DB" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = Dictionary[keys[i]];
                if (obj != null)
                    res.AddItem(keys[i], obj.Clone());
            }

            ThreeDAnnotation annot = new ThreeDAnnotation(res, owner);
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
        }

	    //TODO
        //private ThreeDActivation m_activation;
        //private PDF3DViews m_views;
    }
}
