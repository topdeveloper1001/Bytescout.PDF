using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class UncoloredTilingBrush : Brush
#else
	/// <summary>
    /// Represents an uncolored tiling Brush.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class UncoloredTilingBrush : Brush
#endif
	{
	    private UncoloredTilingPatternColorspace _pattern;

	    /// <summary>
        /// Gets or sets the color of the Brush.
        /// </summary>
        /// <value cref="DeviceColor"></value>
        public DeviceColor Color
        {
            get
            {
                return _pattern.Color;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException();
                _pattern.Color = value;
            }
        }
        
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
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
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
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
                _pattern.YStep = value;
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
	    /// Initializes a new instance of the Bytescout.PDF.UncoloredTilingBrush class.
	    /// </summary>
	    /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of pattern cells in pixels.</param>
	    /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of pattern cells in pixels.</param>
	    public UncoloredTilingBrush(float width, float height)
	    {
		    if (width < 0)
			    throw new ArgumentOutOfRangeException("width");
		    if (height < 0)
			    throw new ArgumentOutOfRangeException("height");

		    _pattern = new UncoloredTilingPatternColorspace(width, height);
	    }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.UncoloredTilingBrush.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.UncoloredTilingBrush.</returns>
        public override object Clone()
        {
            UncoloredTilingBrush p = this.MemberwiseClone() as UncoloredTilingBrush;
            p._pattern = _pattern.Clone() as UncoloredTilingPatternColorspace;
            return p;
        }

        internal override void WriteParameters(MemoryStream stream, Resources resources)
        {
            _pattern.WriteColorSpaceForNotStroking(stream, resources);
        }

        internal override void WriteChanges(Brush newBrush, MemoryStream stream, Resources resources)
        {
            if (newBrush is UncoloredTilingBrush)
            {
                _pattern.WriteChangesForNotStroking((newBrush as UncoloredTilingBrush)._pattern, stream, resources);
            }
            else
                newBrush.WriteParameters(stream, resources);
        }
    }
}
