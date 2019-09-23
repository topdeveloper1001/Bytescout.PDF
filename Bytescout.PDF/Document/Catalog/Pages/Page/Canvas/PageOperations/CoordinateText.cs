using System.Text;
using System.Drawing;

namespace Bytescout.PDF
{
    internal class CoordinateText
    {
	    private float _textHeight;
	    private float _textWidth;
	    private readonly FontBase _fontBase;
	    private readonly float _fontSize;
	    private StringBuilder _text;
	    private PointF _coordinate;
	    private readonly int _weight;
	    private readonly float _scale;

	    public float TextHeight
        {
            get
            {
                return _textHeight;
            }

            set
            {
                _textHeight = value;
            }
        }

        public StringBuilder Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
            }
        }

        public PointF Coordinate
        {
            get
            {
                return _coordinate;
            }
            set
            {
                _coordinate = value;
            }
        }

        public int Weight
        {
            get
            {
                return _weight;
            }
        }

        public float TextWidth
        {
            get
            {
                return _textWidth;
            }

            set
            {
                _textWidth = value;
            }
        }

        public FontBase FontBase
        {
            get
            {
                return _fontBase;
            }
        }

        public float FontSize
        {
            get
            {
                return _fontSize;
            }
        }

        public float Scale
        {
            get
            {
                return _scale;
            }
        }

	    public CoordinateText(PointF coordinate, StringBuilder text, FontBase fontBase, float fontSize, float textWidth, float textHeight, int weight, float scale)
	    {
		    _text = text;
		    _coordinate = coordinate;
		    _weight = weight;
		    _textWidth = textWidth;
		    _fontBase = fontBase;
		    _textHeight = textHeight;
		    _fontSize = fontSize;
		    _scale = scale;
	    }

	    public override string ToString()
        {
            return "\"" + _text + "\" " + _coordinate.ToString() + " " + _weight.ToString();
        }
    }
}
