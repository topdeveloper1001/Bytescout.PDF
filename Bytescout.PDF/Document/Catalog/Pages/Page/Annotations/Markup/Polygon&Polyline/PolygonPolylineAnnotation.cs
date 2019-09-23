using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class PolygonPolylineAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents an abstract class for polygon and polyline annotations.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class PolygonPolylineAnnotation: MarkupAnnotation
#endif
	{
	    private PointsArray _pointsArray;
	    private AnnotationBorderStyle _borderStyle;

	    /// <summary>
	    /// Gets the value indicating the border style of this annotation.
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
	    /// Gets the coordinates of the annotation’s vertices.
	    /// </summary>
	    /// <value cref="PointsArray"></value>
	    public PointsArray Vertices
	    {
		    get
		    {
			    if (_pointsArray == null)
			    {
				    PDFArray array = Dictionary["Vertices"] as PDFArray;
				    if (array == null)
				    {
					    array = new PDFArray();
					    Dictionary.AddItem("Vertices", array);
				    }
				    _pointsArray = new PointsArray(array, Page);
				    _pointsArray.ChangedPointsArray += changedPointsArray;
			    }
			    return _pointsArray;
		    }
	    }

	    /// <summary>
	    /// Gets or sets the color of the background.
	    /// </summary>
	    /// <value cref="DeviceColor"></value>
	    public DeviceColor BackgroundColor
	    {
		    get
		    {
			    if (Dictionary["IC"] == null)
				    return null;
			    return TypeConverter.PDFArrayToPDFColor(Dictionary["IC"] as PDFArray);
		    }
		    set
		    {
			    if (value == null)
				    Dictionary.RemoveItem("IC");
			    else
				    Dictionary.AddItem("IC", TypeConverter.PDFColorToPDFArray(value));
			    CreateApperance();
		    }
	    }

	    internal PolygonPolylineAnnotation(PointsArray points, IDocumentEssential owner)
            : base(null)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            if (points.Page != null)
            {
                _pointsArray = new PointsArray();
                _pointsArray.AddRange(points.ToArray());
            }
            else
            {
                _pointsArray = points;
            }
            _pointsArray.ChangedPointsArray += changedPointsArray;

            Dictionary.AddItem("Vertices", _pointsArray.Array);
            Color = new ColorRGB(0, 0, 0);
        }

        internal PolygonPolylineAnnotation(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

	    //TODO: Measure

        internal static void CopyTo(PDFDictionary sourceDict, PDFDictionary destinationDict, Page oldPage, Page newPage)
        {
            string[] keys = { "LE", "IC", "BE", "IT" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = sourceDict[keys[i]];
                if (obj != null)
                    destinationDict.AddItem(keys[i], obj.Clone());
            }

            PDFDictionary bs = sourceDict["BS"] as PDFDictionary;
            if (bs != null)
                destinationDict.AddItem("BS", AnnotationBorderStyle.Copy(bs));

            PDFArray vertices = sourceDict["Vertices"] as PDFArray;
            if (vertices != null)
            {
	            RectangleF oldRect = oldPage == null ? new RectangleF() : oldPage.PageRect;

	            destinationDict.AddItem("Vertices", CloneUtility.CopyArrayCoordinates(vertices, oldRect, newPage.PageRect, oldPage == null));
            }

	        PDFDictionary measure = sourceDict["Measure"] as PDFDictionary;
            if (measure != null)
                destinationDict.AddItem("Measure", Measure.Copy(measure));
        }

        private void loadBorderStyle()
        {
            PDFDictionary dict = Dictionary["BS"] as PDFDictionary;
            if (dict == null)
            {
                _borderStyle = new AnnotationBorderStyle();
                Dictionary.AddItem("BS", _borderStyle.GetDictionary());
            }
            else
            {
                _borderStyle = new AnnotationBorderStyle(dict);
            }
            _borderStyle.ChangedBorderStyle += changedBorderStyle;
        }

        private void changedBorderStyle(object sender)
        {
            CreateApperance();
        }

        private void changedPointsArray(object sender)
        {
            CreateApperance();
        }
    }
}
