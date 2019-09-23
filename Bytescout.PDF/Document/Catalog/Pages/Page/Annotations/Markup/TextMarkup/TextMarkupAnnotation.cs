using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class TextMarkupAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents an abstract class for text markup annotations.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class TextMarkupAnnotation: MarkupAnnotation
#endif
	{
	    private RotatingRectangle _quadPoints;

	    /// <summary>
	    /// Gets or sets the x-coordinate of the left edge of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Left
	    {
		    get { return Rectangle.Left; }
		    set
		    {
			    Rectangle.Left = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the y-coordinate of the top edge of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Top
	    {
		    get { return Rectangle.Top; }
		    set
		    {
			    Rectangle.Top = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the width of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Width
	    {
		    get { return Rectangle.Width; }
		    set
		    {
			    Rectangle.Width = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the height of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Height
	    {
		    get { return Rectangle.Height; }
		    set
		    {
			    Rectangle.Height = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the rotation angle of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float RotationAngle
	    {
		    get { return Rectangle.Angle; }
		    set
		    {
			    Rectangle.Angle = value;
			    CreateApperance();
		    }
	    }

	    internal RotatingRectangle Rectangle
	    {
		    get
		    {
			    if (_quadPoints == null)
			    {
				    PDFArray array = Dictionary["QuadPoints"] as PDFArray;
				    if (array == null)
				    {
					    _quadPoints = new RotatingRectangle(0, 0, 1, 1, 0);
					    Dictionary.AddItem("QuadPoints", _quadPoints.Array);
				    }
				    else 
				    {
					    _quadPoints = new RotatingRectangle(array, Page);
				    }
			    }
			    return _quadPoints;
		    }
		    set
		    {
			    _quadPoints = value;
		    }
	    }

	    internal static void CopyTo(PDFDictionary sourceDict, PDFDictionary destinationDict, Page oldPage, Page newPage)
	    {
		    PDFArray quadPoints = sourceDict["QuadPoints"] as PDFArray;
		    if (quadPoints != null)
		    {
			    RectangleF oldRect;
			    if (oldPage == null)
				    oldRect = new RectangleF();
			    else
				    oldRect = oldPage.PageRect;

			    destinationDict.AddItem("QuadPoints", CloneUtility.CopyArrayCoordinates(quadPoints, oldRect, newPage.PageRect, oldPage == null));
		    }
	    }

	    internal TextMarkupAnnotation(RotatingRectangle quadPoints, IDocumentEssential owner)
            : base(null)
        {
            if (quadPoints == null)
                _quadPoints = new RotatingRectangle(0, 0, 1, 1, 0);
            else
                _quadPoints = quadPoints;
            Dictionary.AddItem("QuadPoints", _quadPoints.Array);
            Color = new ColorRGB(255, 0, 0);
        }

        internal TextMarkupAnnotation(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner)
        { }
    }
}
