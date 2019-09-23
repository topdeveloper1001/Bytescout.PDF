using System;

namespace Bytescout.PDF
{
    internal class StateCanvas : ICloneable
    {
	    private Pen _pen;
	    private Brush _brush;
	    private RenderingIntentType _renderingIntent;
	    private BlendMode _blendMode;
	    private StringFormat _stringFormat;

	    public Brush Brush
        {
            get
            {
                return _brush;
            }

            set
            {
                _brush = value;
            }
        }

        public Pen Pen
        {
            get
            {
                return _pen;
            }

            set
            {
                _pen = value;
            }
        }

        public StringFormat StringFormat
        {
            get
            {
                return _stringFormat;
            }
            set
            {
                _stringFormat = value;
            }
        }

        public RenderingIntentType RenderingIntent
        {
            get
            {
                return _renderingIntent;
            }
            set
            {
                _renderingIntent = value;
            }
        }

        public BlendMode BlendMode
        {
            get
            {
                return _blendMode;
            }

            set
            {
                _blendMode = value;
            }
        }

	    public StateCanvas()
	    {
		    _pen = new SolidPen();
		    _brush = new SolidBrush();
		    _renderingIntent = RenderingIntentType.RelativeColorimetric;
		    _blendMode = PDF.BlendMode.Normal;
		    _stringFormat = new StringFormat();
	    }

	    public object Clone()
	    {
		    StateCanvas p = this.MemberwiseClone() as StateCanvas;
		    p._pen = _pen.Clone() as Pen;
		    p._brush = _brush.Clone() as Brush;
		    p._stringFormat = _stringFormat.Clone() as StringFormat;
		    return p;
	    }
    }
}
