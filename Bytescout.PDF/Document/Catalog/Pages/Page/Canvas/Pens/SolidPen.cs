using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class SolidPen : Pen
#else
	/// <summary>
    /// Represents a solid pen.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class SolidPen : Pen
#endif
	{
	    private float _miterLimit;
	    private float _width;
	    private Color _color;
	    private LineCapStyle _lineCap;
	    private LineJoinStyle _lineJoin;
	    private DashPattern _dashPattern;
	    private float _opacity;

	    /// <summary>
	    /// Gets or sets the miter limit.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public override float MiterLimit
	    {
		    get
		    {
			    return _miterLimit;
		    }
		    set
		    {
			    if (_miterLimit != value)
			    {
				    if (value < 0.0f || value > 11.5f)
					    throw new PDFMiterLimitException();
				    _miterLimit = value;
			    }
		    }
	    }
        
	    /// <summary>
	    /// Gets or sets the opacity value in percent.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public override float Opacity
	    {
		    get
		    {
			    return _opacity * 100;
		    }
		    set
		    {
			    if (value < 0 || value > 100)
				    throw new PDFOpacityException();
			    _opacity = value / 100;
		    }
	    }
        
	    /// <summary>
	    /// Gets or sets the line dash pattern.
	    /// </summary>
	    /// <value cref="PDF.DashPattern"></value>
	    public override DashPattern DashPattern
	    {
		    get
		    {
			    return _dashPattern;
		    }
		    set
		    {
			    if (value == null)
				    throw new NullReferenceException();
			    _dashPattern = value;
		    }
	    }
        
	    /// <summary>
	    /// Gets or sets the width of this pen.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public override float Width
	    {
		    get
		    {
			    return _width;
		    }
		    set
		    {
			    if (value <= 0)
				    throw new ArgumentOutOfRangeException();
			    _width = value;
		    }
	    }
        
	    /// <summary>
	    /// Gets or sets the line cap of this pen.
	    /// </summary>
	    /// <value cref="LineCapStyle"></value>
	    public override LineCapStyle LineCap
	    {
		    get
		    {
			    return _lineCap;
		    }
		    set
		    {
			    _lineCap = value;
		    }
	    }
        
	    /// <summary>
	    /// Gets or sets the line join style of this pen.
	    /// </summary>
	    /// <value cref="LineJoinStyle"></value>
	    public override LineJoinStyle LineJoin
	    {
		    get
		    {
			    return _lineJoin;
		    }
		    set
		    {
			    _lineJoin = value;
		    }
	    }
        
	    /// <summary>
	    /// Gets or sets the color of this pen.
	    /// </summary>
	    /// <value cref="PDF.Color"></value>
	    public Color Color
	    {
		    get
		    {
			    return _color;
		    }
		    set
		    {
			    if (value == null)
				    throw new NullReferenceException();
			    _color = value;
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.SolidPen class.
        /// </summary>
        public SolidPen()
        {
            _width = 1;
            _color = new ColorRGB(0, 0, 0);
            _lineCap = LineCapStyle.Butt;
            _lineJoin = LineJoinStyle.Miter;
            _dashPattern = new DashPattern();
            _opacity = 1.0f;
            _miterLimit = 10.0f;
        }
        
        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.SolidPen class.
        /// </summary>
        /// <param name="color">A Bytescout.PDF.Color that indicates the color of the pen.</param>
        public SolidPen(Color color)
        {
            if (color == null)
                throw new ArgumentNullException();

            _width = 1;
            _color = color;
            _lineCap = LineCapStyle.Butt;
            _lineJoin = LineJoinStyle.Miter;
            _dashPattern = new DashPattern();
            _opacity = 1.0f;
            _miterLimit = 10.0f;
        }
        
        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.SolidPen class.
        /// </summary>
        /// <param name="color">A Bytescout.PDF.Color that indicates the color of the pen.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the pen.</param>
        public SolidPen(Color color, float width)
        {
            if (color == null)
                throw new ArgumentNullException("color");
            if (width < 0)
                throw new ArgumentOutOfRangeException("width");

            _width = width;
            _color = color;
            _lineCap = LineCapStyle.Butt;
            _lineJoin = LineJoinStyle.Miter;
            _dashPattern = new DashPattern();
            _opacity = 1.0f;
            _miterLimit = 10.0f;
        }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.SolidPen.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.SolidPen.</returns>
        public override object Clone()
        {
            SolidPen p = this.MemberwiseClone() as SolidPen;
            p._color = _color.Clone() as Color;
            return p;
        }

        internal override void WriteParameters(MemoryStream stream, Resources resources)
        {
            if (_color is ColorICC)
                _color.Colorspace.WriteColorSpaceForStroking(stream, resources);
            IPDFPageOperation operation = new DefaultColorSpaceForStroking(_color);
            operation.WriteBytes(stream);
        }

        internal override void WriteForNotStroking(MemoryStream stream, Resources resources)
        {
            if (_color is ColorICC)
                _color.Colorspace.WriteColorSpaceForNotStroking(stream, resources);
            IPDFPageOperation operation = new DefaultColorSpaceForNotStroking(_color);
            operation.WriteBytes(stream);
        }

        internal override void WriteChanges(Pen newPen, MemoryStream stream, Resources resources)
        {
            if (!_dashPattern.Equals(newPen.DashPattern))
            {
                IPDFPageOperation operation = new LineDash(newPen.DashPattern);
                operation.WriteBytes(stream);
            }
            if (!_lineCap.Equals(newPen.LineCap))
            {
                IPDFPageOperation operation = new LineCap(newPen.LineCap);
                operation.WriteBytes(stream);
            }
            if (!_lineJoin.Equals(newPen.LineJoin))
            {
                IPDFPageOperation operation = new LineJoin(newPen.LineJoin);
                operation.WriteBytes(stream);
            }
            if (!_miterLimit.Equals(newPen.MiterLimit))
            {
                IPDFPageOperation operation = new MiterLimit(newPen.MiterLimit);
                operation.WriteBytes(stream);
            }
            if (!_opacity.Equals(newPen.Opacity / 100))
            {
                PDFDictionary dict = new PDFDictionary();
                dict.AddItem("CA", new PDFNumber(newPen.Opacity / 100));
                string name = resources.AddResources(ResourceType.ExtGState, dict);
                IPDFPageOperation operation = new GraphicsState(name);
                operation.WriteBytes(stream);
            }
            if (!_width.Equals(newPen.Width))
            {
                IPDFPageOperation operation = new Linewidth(newPen.Width);
                operation.WriteBytes(stream);
            }
            if (newPen is SolidPen)
            {
                if (!_color.Equals((newPen as SolidPen).Color))
                {
                    if ((newPen as SolidPen)._color is ColorICC)
                        (newPen as SolidPen)._color.Colorspace.WriteColorSpaceForStroking(stream, resources);
                    newPen.WriteParameters(stream, resources);
                }
            }
            else
            {
                newPen.WriteParameters(stream, resources);
            }
        }
    }
}
