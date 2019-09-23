using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class SolidBrush : Brush
#else
	/// <summary>
    /// Represents a Brush that fills any object with a solid color.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class SolidBrush : Brush
#endif
	{
	    private Color _color;
	    private float _opacity;

	    /// <summary>
	    /// Gets or sets the opacity value in percent.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public float Opacity
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
	    /// Gets or sets the color of the Brush.
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
        /// Initializes a new instance of the Bytescout.PDF.SolidBrush.
        /// </summary>
        public SolidBrush()
        {
            _color = new ColorRGB(0, 0, 0);
            _opacity = 100;
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.SolidBrush class with the specified color.
        /// </summary>
        /// <param name="color">The color of this Brush.</param>
        public SolidBrush(Color color)
        {
            if (color == null)
                throw new ArgumentNullException();

            _color = color;
            _opacity = 100;
        }

	    /// <summary>
        /// Allows the creation of a shallow copy of this Bytescout.PDF.SolidBrush.
        /// </summary>
        /// <returns cref="object" href="http://msdn.microsoft.com/en-us/library/system.object.aspx">Returns a shallow copy of this Bytescout.PDF.SolidBrush.</returns>
        public override object Clone()
        {
            SolidBrush p = this.MemberwiseClone() as SolidBrush;
            p._color = _color.Clone() as Color;
            return p;
        }

        internal override void WriteParameters(MemoryStream stream, Resources resources)
        {
            if (_color is ColorICC)
                _color.Colorspace.WriteColorSpaceForNotStroking(stream, resources);
            IPDFPageOperation operation = new DefaultColorSpaceForNotStroking(_color);
            operation.WriteBytes(stream);
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("ca", new PDFNumber(_opacity / 100));
            string name = resources.AddResources(ResourceType.ExtGState, dict);
            operation = new GraphicsState(name);
            operation.WriteBytes(stream);
        }

        internal override void WriteChanges(Brush newBrush, MemoryStream stream, Resources resources)
        {
            if (newBrush is SolidBrush)
            {
                if (!_color.Equals((newBrush as SolidBrush)._color))
                {
                    if ((newBrush as SolidBrush)._color is ColorICC)
                        (newBrush as SolidBrush)._color.Colorspace.WriteColorSpaceForNotStroking(stream, resources);
                    IPDFPageOperation operation = new DefaultColorSpaceForNotStroking((newBrush as SolidBrush).Color);
                    operation.WriteBytes(stream);
                }
                if (!_opacity.Equals((newBrush as SolidBrush)._opacity))
                {
                    PDFDictionary dict = new PDFDictionary();
                    dict.AddItem("ca", new PDFNumber((newBrush as SolidBrush)._opacity));
                    string name = resources.AddResources(ResourceType.ExtGState, dict);
                    IPDFPageOperation operation = new GraphicsState(name);
                    operation.WriteBytes(stream);
                }
            }
            else
            {
                newBrush.WriteParameters(stream, resources);
            }
        }
    }
}
