using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ColoredTilingPen : Pen
#else
	/// <summary>
    /// Represents a colored tiling pen.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class ColoredTilingPen : Pen
#endif
	{
	    private ColoredTilingPatternColorspace _pattern;
	    private float _width;
	    private float _miterLimit;
	    private float _widthPen;
	    private LineCapStyle _lineCap;
	    private LineJoinStyle _lineJoin;
	    private DashPattern _dashPattern;
	    private float _opacity;

	    /// <summary>
        /// Gets the canvas of the pattern cell.
        /// </summary>
        /// <value cref="PDF.Canvas"></value>
        public Canvas Canvas
        {
            get
            {
                return _pattern.Canvas;
            }
        }
        
        /// <summary>
        /// Gets or sets the Bytescout.PDF.TilingType value specifying the adjustments to the
        /// spacing of tiles relative to the device pixel grid.
        /// </summary>
        /// <value cref="PDF.TilingType"></value>
        public TilingType TilingType
        {
            get
            {
                return _pattern.TilingType;
            }
            set
            {
                _pattern.TilingType = value;
            }
        }
        
        /// <summary>
        /// Gets the height of pattern cells.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float PatternHeight
        {
            get
            {
                return _pattern.Height;
            }
        }
        
        /// <summary>
        /// Gets the width of pattern cells.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float PatternWidth
        {
            get
            {
                return _pattern.Width;
            }
        }
        
        /// <summary>
        /// Gets or sets the desired horizontal spacing between pattern cells, measured in the pattern coordinate system.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float XStep
        {
            get
            {
                return _pattern.XStep;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException();
                _pattern.XStep = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the desired vertical spacing between pattern cells, measured in the pattern coordinate system.
        /// </summary>
        /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
        public float YStep
        {
            get
            {
                return _pattern.YStep;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException();
                _pattern.YStep = value;
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
                return _widthPen;
            }
            set
            {
                if(value < 0)
                    throw new ArgumentException();
                _widthPen = value;
            }
        }
        
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
                DashPattern m_dashPattern = value;
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

        internal float[] Matrix
        {
            get
            {
                return _pattern.Matrix;
            }
            set
            {
                if (value.Length != 6)
                    throw new PDFException();
                _pattern.Matrix = value;
            }
        }

	    /// <summary>
	    /// Initializes a new instance of the Bytescout.PDF.ColoredTilingPen class.
	    /// </summary>
	    /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of pattern cells in pixels.</param>
	    /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of pattern cells in pixels.</param>
	    public ColoredTilingPen(float width, float height)
	    {
		    if (width < 0)
			    throw new ArgumentOutOfRangeException("width");
		    if (height < 0)
			    throw new ArgumentOutOfRangeException("height");

		    _pattern = new ColoredTilingPatternColorspace(width, height);
		    _width = 1;
		    _lineCap = LineCapStyle.Butt;
		    _lineJoin = LineJoinStyle.Miter;
		    _dashPattern = new DashPattern();
		    _opacity = 1.0f;
		    _miterLimit = 10.0f;
	    }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.ColoredTilingPen.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.ColoredTilingPen.</returns>
        public override object Clone()
        {
            ColoredTilingPen p = this.MemberwiseClone() as ColoredTilingPen;
            p._pattern = _pattern.Clone() as ColoredTilingPatternColorspace;
            return p;
        }

        internal override void WriteParameters(MemoryStream stream, Resources resources)
        {
            _pattern.WriteColorSpaceForStroking(stream, resources);
        }

        internal override void WriteForNotStroking(MemoryStream stream, Resources resources)
        {
            _pattern.WriteColorSpaceForNotStroking(stream, resources);
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
            if (newPen is ColoredTilingPen)
            {
                _pattern.WriteChangesForStroking((newPen as ColoredTilingPen)._pattern, stream, resources);
            }
            else
                newPen.WriteParameters(stream, resources);
        }
    }
}
