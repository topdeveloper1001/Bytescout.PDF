using System;

namespace Bytescout.PDF
{
    internal class RotatingRectangle
    {
	    private float _left;
	    private float _top;
	    private float _width;
	    private float _height;
	    private float _angle;
	    private readonly PDFArray _array;
	    readonly Page _page;

	    public float Left
	    {
		    get
		    {
			    return _left;
		    }
		    set
		    {
			    _left = value;
			    changeArray();
		    }
	    }
        
	    public float Top
	    {
		    get
		    {
			    return _top;
		    }
		    set
		    {
			    _top = value;
			    changeArray();
		    }
	    }
        
	    public float Width
	    {
		    get
		    {
			    return _width;
		    }
		    set
		    {
			    _width = value;
			    changeArray();
		    }
	    }
        
	    public float Height
	    {
		    get
		    {
			    return _height;
		    }

		    set
		    {
			    _height = value;
			    changeArray();
		    }
	    }
        
	    public float Angle
	    {
		    get
		    {
			    return _angle;
		    }
		    set
		    {
			    _angle = value;
			    changeArray();
		    }
	    }

	    internal PDFArray Array
	    {
		    get
		    {
			    return _array;
		    }
	    }

	    public RotatingRectangle(float left, float top, float width, float height, float angle)
        {
            _angle = angle;
            _left = left;
            _top = top;
            _width = width;
            _height = height;
            _array = new PDFArray();
            changeArray();
        }
        
        public RotatingRectangle(float left, float top, float width, float height)
        {
            _angle = 0;
            _left = left;
            _top = top;
            _width = width;
            _height = height;
            _array = new PDFArray();
            changeArray();
        }

        internal RotatingRectangle(PDFArray array, Page page)
        {
            _array = array;
            _page = page;
            if (array.Count != 8)
            {
                setDefault();
                changeArray();
            }
            else
            {
                float[] arrayf = new float[8];
                for (int i = 0; i < 8; ++i)
                {
                    PDFNumber number = array[i] as PDFNumber;
                    if (number == null)
                    {
                        setDefault();
                        changeArray();
                        return;
                    }
                    arrayf[i] = (float)number.GetValue();
                }
                setFromArray(arrayf);
            }
        }

	    private void changeArray()
        {
            _array.Clear();
            double angle = _angle * Math.PI / 180;
            double[] arrayf = new double[8];

            arrayf[6] = _left + _width * Math.Cos(angle) - _height * Math.Sin(angle);
            arrayf[7] = _top + _width * Math.Sin(angle) + _height * Math.Cos(angle);
            arrayf[4] = _left - _height * Math.Sin(angle);
            arrayf[5] = _top + _height * Math.Cos(angle);
            arrayf[2] = _left + _width * Math.Cos(angle);
            arrayf[3] = _top + _width * Math.Sin(angle);
            arrayf[0] = _left;
            arrayf[1] = _top;

            if (_page != null)
            {
                for (int i = 0; i < 4; ++i)
                {
                    arrayf[i * 2] = arrayf[i * 2] + _page.PageRect.Left;
                    arrayf[i * 2 + 1] = _page.PageRect.Bottom - arrayf[i * 2 + 1];
                }
            }

            for (int i = 0; i < 8; ++i)
            {
                _array.AddItem(new PDFNumber((float)arrayf[i]));
            }
        }

        private void setDefault()
        {
            _angle = 0;
            _left = 0;
            _top = 0;
            _width = 0;
            _height = 0;
        }

        private void setFromArray(float[] array)
        {
            if (_page != null)
            {
                for (int i = 0; i < 4; ++i)
                {
                    array[i * 2] = array[i * 2] - _page.PageRect.Left;
                    array[i * 2 + 1] = _page.PageRect.Bottom - array[i * 2 + 1];
                }
            }
            _left = array[6];
            _top = array[7];
            _width = (float)Math.Sqrt((array[0] - array[2]) * (array[0] - array[2]) + (array[1] - array[3]) * (array[1] - array[3]));
            _height = (float)Math.Sqrt((array[2] - array[4]) * (array[2] - array[4]) + (array[3] - array[5]) * (array[3] - array[5]));
            _angle = (float)(Math.Acos((array[4] - array[6]) / _width) * 180 / Math.PI);
        }
    }
}
