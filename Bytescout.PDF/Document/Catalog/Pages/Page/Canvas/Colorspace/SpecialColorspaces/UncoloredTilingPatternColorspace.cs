using System.IO;

namespace Bytescout.PDF
{
    internal class UncoloredTilingPatternColorspace : TilingPatternColorspace
    {
	    private DeviceColor _color;
	    private readonly Canvas _canvas;
	    private readonly Resources _resources;
	    private readonly MemoryStream _stream;
	    private readonly PDFDictionaryStream _streamDict;
	    private readonly PDFTilingPaintType _paintType;
	    private TilingType _tilingType;
	    private float _height;
	    private float _width;
	    private float _xstep;
	    private float _ystep;
	    private float[] _matrix;
	    private string _name;
		public override int N
        {
            get
            {
                return _color.Colorspace.N;
            }
        }
        
        public DeviceColor Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
            }
        }
        
        public Canvas Canvas
        {
            get
            {
                return _canvas;
            }
        }
        
        public override PDFTilingPaintType PaintType
        {
            get
            {
                return _paintType;
            }
        }
        
        public override TilingType TilingType
        {
            get
            {
                return _tilingType;
            }
            set
            {
                _tilingType = value;
                _streamDict.Dictionary.AddItem("TilingType", new PDFNumber((float)_tilingType));
            }
        }
        
        public override float Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                PDFArray rect = new PDFArray();
                rect.AddItem(new PDFNumber(0));
                rect.AddItem(new PDFNumber(0));
                rect.AddItem(new PDFNumber(_width));
                rect.AddItem(new PDFNumber(_height));
                _streamDict.Dictionary.AddItem("BBox", rect);
            }
        }
        
        public override float Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                PDFArray rect = new PDFArray();
                rect.AddItem(new PDFNumber(0));
                rect.AddItem(new PDFNumber(0));
                rect.AddItem(new PDFNumber(_width));
                rect.AddItem(new PDFNumber(_height));
                _streamDict.Dictionary.AddItem("BBox", rect);
            }
        }
        
        public override float XStep
        {
            get
            {
                return _xstep;
            }
            set
            {
                _xstep = value;
                _streamDict.Dictionary.AddItem("XStep", new PDFNumber(_xstep));
            }
        }
        
        public override float YStep
        {
            get
            {
                return _ystep;
            }
            set
            {
                _ystep = value;
                _streamDict.Dictionary.AddItem("YStep", new PDFNumber(_ystep));
            }
        }
        
        public override float[] Matrix
        {
            get
            {
                return _matrix;
            }
            set
            {
                if (value.Length != 6)
                    throw new PDFException();
                _matrix = value;
                PDFArray array = new PDFArray();
                for (int i = 0; i < 6; ++i)
                    array.AddItem(new PDFNumber((_matrix[i])));
                _streamDict.Dictionary.AddItem("Matrix", array);
            }
        }

	    public UncoloredTilingPatternColorspace(float width, float height)
	    {
		    _stream = new MemoryStream();
		    _resources = new Resources();
		    _canvas = new Canvas(_stream, _resources, width, height);
		    _streamDict = new PDFDictionaryStream(new PDFDictionary(), _stream);
		    _paintType = PDFTilingPaintType.UncoloredTilingPattern;
		    _tilingType = PDF.TilingType.ConstantSpacing;
		    _height = height;
		    _width = width;
		    _matrix = new float[6];
		    _matrix[0] = 1;
		    _matrix[1] = 0;
		    _matrix[2] = 0;
		    _matrix[3] = 1;
		    _matrix[4] = 0;
		    _matrix[5] = 0;
		    _xstep = width;
		    _ystep = height;
		    _color = new ColorRGB(0, 0, 0);

		    PDFDictionary dict = _streamDict.Dictionary;
		    dict.AddItem("Type", new PDFName(Name));
		    dict.AddItem("PatternType", new PDFNumber((float)PDFPatternType.TilingPattern));
		    dict.AddItem("PaintType", new PDFNumber((float)_paintType));
		    dict.AddItem("TilingType", new PDFNumber((float)_tilingType));
		    PDFArray rect = new PDFArray();
		    rect.AddItem(new PDFNumber(0));
		    rect.AddItem(new PDFNumber(0));
		    rect.AddItem(new PDFNumber(_width));
		    rect.AddItem(new PDFNumber(_height));
		    dict.AddItem("BBox", rect);
		    dict.AddItem("XStep", new PDFNumber(_xstep));
		    dict.AddItem("YStep", new PDFNumber(_ystep));
		    dict.AddItem("Resources", _resources.Dictionary);
		    PDFArray array = new PDFArray();
		    for (int i = 0; i < 6; ++i)
			    array.AddItem(new PDFNumber((_matrix[i])));
		    dict.AddItem("Matrix", array);
		    _name = "";
	    }

	    public override object Clone()
        {
            UncoloredTilingPatternColorspace p = this.MemberwiseClone() as UncoloredTilingPatternColorspace;
            p._color = _color.Clone() as DeviceColor;
            p._canvas.Resources.Dictionary.AddRange(_canvas.Resources.Dictionary);
            byte[] buf = new byte[_canvas.Stream.Length];
            _canvas.Stream.Position = 0;
            _canvas.Stream.Read(buf, 0, buf.Length);
            p._canvas.Stream.Position = 0;
            p._canvas.Stream.Write(buf, 0, buf.Length);
            return p;
        }

        internal override void WriteColorSpaceForNotStroking(MemoryStream stream, Resources resources)
        {
            _name = resources.AddResources(ResourceType.Pattern, _streamDict);
            PDFArray colorspace = new PDFArray();
            colorspace.AddItem(new PDFName(Name));
            colorspace.AddItem(new PDFName(_color.Colorspace.Name));
            string name = resources.AddResources(ResourceType.ColorSpace, colorspace);
            IPDFPageOperation operation = new ColorSpaceForNonStroking(name);
            operation.WriteBytes(stream);
            operation = new ColorForNonStrokingEx(_color.ToArray(), _name);
            operation.WriteBytes(stream);
        }

        internal override void WriteColorSpaceForStroking(MemoryStream stream, Resources resources)
        {
            _name = resources.AddResources(ResourceType.Pattern, _streamDict);
            PDFArray colorspace = new PDFArray();
            colorspace.AddItem(new PDFName(Name));
            colorspace.AddItem(new PDFName(_color.Colorspace.Name));
            string name = resources.AddResources(ResourceType.ColorSpace, colorspace);
            IPDFPageOperation operation = new ColorSpaceForStroking(name);
            operation.WriteBytes(stream);
            operation = new ColorForStrokingEx(_color.ToArray(), _name);
            operation.WriteBytes(stream);
        }

        internal override bool WriteChangesForNotStroking(Colorspace newCS, MemoryStream stream, Resources resources)
        {
            _name = resources.AddResources(ResourceType.Pattern, _streamDict);
            if (newCS is UncoloredTilingPatternColorspace)
            {
                if (!_name.Equals((newCS as UncoloredTilingPatternColorspace)._name))
                {
                    newCS.WriteColorSpaceForNotStroking(stream, resources);
                }
                else if (!_color.Equals((newCS as UncoloredTilingPatternColorspace)._color))
                {
                    IPDFPageOperation operation = new ColorForNonStrokingEx((newCS as UncoloredTilingPatternColorspace)._color.ToArray(), (newCS as UncoloredTilingPatternColorspace)._name);
                    operation.WriteBytes(stream);
                    return true;
                }
            }
            else
            {
                newCS.WriteColorSpaceForNotStroking(stream, resources);
                return true;
            }

            return false;
        }

        internal override bool WriteChangesForStroking(Colorspace newCS, MemoryStream stream, Resources resources)
        {
            _name = resources.AddResources(ResourceType.Pattern, _streamDict);
            if (newCS is UncoloredTilingPatternColorspace)
            {
                if (!_name.Equals((newCS as UncoloredTilingPatternColorspace)._name))
                {
                    newCS.WriteColorSpaceForStroking(stream, resources);
                }
                else if (!_color.Equals((newCS as UncoloredTilingPatternColorspace)._color))
                {
                    IPDFPageOperation operation = new ColorForStrokingEx((newCS as UncoloredTilingPatternColorspace)._color.ToArray(), (newCS as UncoloredTilingPatternColorspace)._name);
                    operation.WriteBytes(stream);
                    return true;
                }
            }
            else
            {
                newCS.WriteColorSpaceForStroking(stream, resources);
                return true;
            }

            return false;
        }
    }
}
