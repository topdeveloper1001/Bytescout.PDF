using System;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Destination
#else
	/// <summary>
    /// Represents a destination in the PDF document for links, bookmarks, and other interactive PDF features.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class Destination
#endif
	{
	    private Page _page;
	    private float _left = 0;
	    private float _top = 0;
	    private float _width = -1;
	    private float _height = -1;
	    private int _zoom = -1;
	    private ZoomMode _zoomMode;
	    private PDFArray _array;

	    /// <summary>
        /// Gets the destination page.
        /// </summary>
        /// <value cref="PDF.Page"></value>
        public Page Page { get { return _page; } }

        /// <summary>
        /// Gets the x-coordinate of the upper-left corner of the page to be positioned at the
        /// upper-left corner of the window when this view gets displayed.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Left { get { return _left; } }

        /// <summary>
        /// Gets the y-coordinate of the upper-left corner of the page to be positioned at the
        /// upper-left corner of the window when this view gets displayed.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Top { get { return _top; } }

        /// <summary>
        /// Gets the width of the page area to fit into the window width.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Width { get { return _width; } }

        /// <summary>
        /// Gets the height of the page area to fit into the window height.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float Height { get { return _height; } }

        /// <summary>
        ///  Gets the zoom percent (magnification level) of the page to be displayed in the view.
        /// </summary>
        /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
        public int Zoom { get { return _zoom; } }

        /// <summary>
        /// Gets the zoom type of the page to be displayed in the view.
        /// </summary>
        /// <value cref="PDF.ZoomMode"></value>
        public ZoomMode ZoomMode { get { return _zoomMode; } }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.Destination.
	    /// </summary>
	    /// <param name="page">The destination page.</param>
	    public Destination(Page page)
	    {
		    if (page == null)
			    throw new ArgumentNullException("page");

		    _page = page;
		    _zoomMode = ZoomMode.FitXYZ;
		    createArray();
	    }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.Destination.
	    /// </summary>
	    /// <param name="page">The destination page.</param>
	    /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The vertical coordinate of the page that should be positioned at the top edge of the window when the page is displayed.</param>
	    public Destination(Page page, float top)
	    {
		    if (page == null)
			    throw new ArgumentNullException("page");

		    _page = page;
		    _top = top;
		    _zoomMode = ZoomMode.FitXYZ;
		    createArray();
	    }

	    internal Destination(IPDFObject dest, IDocumentEssential owner)
	    {
		    PDFString s = dest as PDFString;
		    if (s != null)
		    {
			    dest = owner.GetDestinationFromNames(s.GetValue());
			    if (dest == null)
				    dest = new PDFArray();
		    }

		    PDFArray array = dest as PDFArray;
		    if (array != null)
		    {
			    parsePage(array, owner);
			    parseZoomMode(array);
			    parseProperties(array, owner);
		    }
		    else
			    dest = new PDFArray();

		    _array = dest as PDFArray;
	    }

	    /// <summary>
        /// Sets the zoom type to use when displaying the page in the view to Bytescout.PDF.ZoomMode.FitXYZ.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the page to be positioned at the upper-left corner of the window when this view gets displayed.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the page to be positioned at the upper-left corner of the window when this view gets displayed.</param>
        /// <param name="zoom" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx">The magnification level in percent.</param>
        public void SetFitXYZ(float left, float top, int zoom)
        {
            setProperties(left, top, -1, -1, zoom, ZoomMode.FitXYZ);
        }

        /// <summary>
        /// Sets the zoom type to use when displaying the page in the view to Bytescout.PDF.ZoomMode.FitPage.
        /// </summary>
        public void SetFitPage()
        {
            setProperties(0, 0, -1, -1, -1, ZoomMode.FitPage);
        }

        /// <summary>
        /// Sets the zoom type to use when displaying the page in the view to Bytescout.PDF.ZoomMode.FitHorizontal.
        /// </summary>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the page to be positioned at the
        /// upper-left corner of the window when this view gets displayed.</param>
        public void SetFitHorizontal(float top)
        {
            setProperties(0, top, -1, -1, -1, ZoomMode.FitHorizontal);
        }

        /// <summary>
        /// Sets the zoom type to use when displaying the page in the view to Bytescout.PDF.ZoomMode.FitVertical.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the page to be positioned at the
        /// upper-left corner of the window when this view gets displayed.</param>
        public void SetFitVertical(float left)
        {
            setProperties(left, 0, -1, -1, -1, ZoomMode.FitVertical);
        }

        /// <summary>
        /// Sets the zoom type to use when displaying the page in the view to Bytescout.PDF.ZoomMode.FitRectangle.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the page to be positioned at the
        /// upper-left corner of the window when this view gets displayed.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the page to be positioned at the
        /// upper-left corner of the window when this view gets displayed.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the page area to fit into window width.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the page area to fit into window height.</param>
        public void SetFitRectangle(float left, float top, float width, float height)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException("width");
            if (height < 0)
                throw new ArgumentOutOfRangeException("height");

            setProperties(left, top, width, height, -1, ZoomMode.FitRectangle);
        }

        /// <summary>
        /// Sets the zoom type to use when displaying the page in the view to Bytescout.PDF.ZoomMode.FitBounding.
        /// </summary>
        public void SetFitBounding()
        {
            setProperties(0, 0, -1, -1, -1, ZoomMode.FitBounding);
        }

        /// <summary>
        /// Sets the zoom type to use when displaying the page in the view to Bytescout.PDF.ZoomMode.FitBoundingHorizontal.
        /// </summary>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the page to be positioned at the
        /// upper-left corner of the window when this view gets displayed.</param>
        public void SetFitBoundingHorizontal(float top)
        {
            setProperties(0, top, -1, -1, -1, ZoomMode.FitBoundingHorizontal);
        }

        /// <summary>
        /// Sets the zoom type to use when displaying the page in the view to Bytescout.PDF.ZoomMode.FitBoundingVertical.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the page to be positioned at the
        /// upper-left corner of the window when this view gets displayed.</param>
        public void SetFitBoundingVertical(float left)
        {
            setProperties(left, 0, -1, -1, -1, ZoomMode.FitBoundingVertical);
        }

        /// <summary>
        /// Returns a System.String that represents the current System.Object.
        /// </summary>
        /// <returns cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">A System.String that represents the current System.Object.</returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder(128);
            str.Append(" ZoomMode = ");
            str.Append(_zoomMode);
            switch (_zoomMode)
            {
                case ZoomMode.FitBoundingHorizontal:
                case ZoomMode.FitHorizontal:
                    str.Append(" Top = ");
                    str.Append(_top);
                    break;
                case ZoomMode.FitBoundingVertical:
                case ZoomMode.FitVertical:
                    str.Append(" Left = ");
                    str.Append(_left);
                    break;
                case ZoomMode.FitRectangle:
                    str.Append(" Left = ");
                    str.Append(_left);
                    str.Append(" Top = ");
                    str.Append(_top);
                    str.Append(" Width = ");
                    str.Append(_width);
                    str.Append(" Height = ");
                    str.Append(_height);
                    break;
                case ZoomMode.FitXYZ:
                    str.Append(" Left = ");
                    str.Append(_left);
                    str.Append(" Top = ");
                    str.Append(_top);
                    str.Append(" Zoom = ");
                    str.Append(_zoom);
                    str.Append('%');
                    break;
            }

            return str.ToString();
        }

        internal PDFArray GetArray()
        {
            return _array;
        }

        private void createArray()
        {
            float pageLeft = 0, pageBottom = 0;
            if (_page != null)
            {
                RectangleF rect = _page.PageRect;
                pageLeft = rect.Left;
                pageBottom = rect.Bottom;
            }

            if (_array == null)
                _array = new PDFArray();
            else
                _array.Clear();

            if (_page != null)
                _array.AddItem(_page.GetDictionary());
            else
                _array.AddItem(new PDFNull());

            switch (_zoomMode)
            {
                case ZoomMode.FitBounding:
                    _array.AddItem(new PDFName("FitB"));
                    break;
                case ZoomMode.FitBoundingHorizontal:
                    _array.AddItem(new PDFName("FitBH"));
                    _array.AddItem(new PDFNumber(pageBottom - _top));
                    break;
                case ZoomMode.FitBoundingVertical:
                    _array.AddItem(new PDFName("FitBV"));
                    _array.AddItem(new PDFNumber(_left + pageLeft));
                    break;
                case ZoomMode.FitHorizontal:
                    _array.AddItem(new PDFName("FitH"));
                    _array.AddItem(new PDFNumber(pageBottom - _top));
                    break;
                case ZoomMode.FitPage:
                    _array.AddItem(new PDFName("Fit"));
                    break;
                case ZoomMode.FitRectangle:
                    _array.AddItem(new PDFName("FitR"));
                    _array.AddItem(new PDFNumber(_left + pageLeft));//left
                    _array.AddItem(new PDFNumber(pageBottom - _top - _height));//bottom
                    _array.AddItem(new PDFNumber(_left + pageLeft + _width));//right
                    _array.AddItem(new PDFNumber(pageBottom - _top));//top
                    break;
                case ZoomMode.FitVertical:
                    _array.AddItem(new PDFName("FitV"));
                    _array.AddItem(new PDFNumber(_left + pageLeft));
                    break;
                case ZoomMode.FitXYZ:
                    _array.AddItem(new PDFName("XYZ"));
                    _array.AddItem(new PDFNumber(_left + pageLeft));
                    _array.AddItem(new PDFNumber(pageBottom - _top));
                    if (_zoom > 0)
                        _array.AddItem(new PDFNumber(_zoom * 1.0f / 100));
                    else
                        _array.AddItem(new PDFNull());
                    break;
            }
        }

        private void setProperties(float left, float top, float width, float height, int zoom, ZoomMode zoomMode)
        {
            _left = left;
            _top = top;
            _width = width;
            _height = height;
            _zoom = zoom;
            _zoomMode = zoomMode;

            createArray();
        }

        private void parsePage(PDFArray arr, IDocumentEssential owner)
        {
            IPDFObject page = arr[0];
            if (page == null)
                return;
            if (page is PDFNumber)
                _page = owner.GetPage((int)(page as PDFNumber).GetValue());
            else if (page is PDFDictionary)
                _page = owner.GetPage(page as PDFDictionary);
            else
                _page = null;

            arr.RemoveItem(0);
            if (_page != null)
                arr.Insert(0, _page.GetDictionary());
            else
                arr.Insert(0, new PDFNull());
        }

        private void parseZoomMode(PDFArray arr)
        {
            PDFName mode = arr[1] as PDFName;
            _zoomMode = TypeConverter.PDFNameToPDFZoomMode(mode);
        }

        private void parseProperties(PDFArray arr, IDocumentEssential owner)
        {
            PDFNumber left, top;
            float pageLeft = 0, pageBottom = 0;
            if (_page != null)
            {
                RectangleF rect = _page.PageRect;
                pageLeft = rect.Left;
                pageBottom = rect.Bottom;
            }

            switch (_zoomMode)
            {
                case ZoomMode.FitBoundingVertical:
                case ZoomMode.FitVertical:
                    left = arr[2] as PDFNumber;
                    if (left != null)
                        _left = (float)left.GetValue() - pageLeft;
                    break;

                case ZoomMode.FitBoundingHorizontal:
                case ZoomMode.FitHorizontal:
                    top = arr[2] as PDFNumber;
                    if (top != null)
                        _top = pageBottom - (float)top.GetValue();
                    break;

                case ZoomMode.FitRectangle:
                    //left bottom right top
                    left = arr[2] as PDFNumber;
                    if (left != null)
                        _left = (float)left.GetValue() - pageLeft;

                    top = arr[5] as PDFNumber;
                    if (top != null)
                        _top = pageBottom - (float)top.GetValue();

                    PDFNumber bottom = arr[3] as PDFNumber;
                    PDFNumber right = arr[4] as PDFNumber;
                    if (bottom != null)
                        _height = pageBottom - _top - (float)bottom.GetValue();
                    if (right != null)
                        _width = (float)right.GetValue() - (_left + pageLeft);
                    break;

                case ZoomMode.FitXYZ:
                    left = arr[2] as PDFNumber;
                    if (left != null)
                        _left = (float)left.GetValue() - pageLeft;

                    top = arr[3] as PDFNumber;
                    if (top != null)
                        _top = pageBottom - (float)top.GetValue();

                    PDFNumber zoom = arr[4] as PDFNumber;
                    if (zoom != null)
                        _zoom = (int)(zoom.GetValue() * 100);
                    break;
            }
        }
    }
}
