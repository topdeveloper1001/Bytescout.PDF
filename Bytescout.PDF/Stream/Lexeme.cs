namespace Bytescout.PDF
{
    internal class Lexeme
    {
	    private readonly byte[] _buffer;
	    private int _offset;
	    private int _count;

	    internal int Offset { get { return _offset; } }

        internal int Length { get { return _count; } }

	    internal Lexeme(byte[] buf)
	    {
		    _buffer = buf;
	    }

	    internal void SetSize(int offset, int count)
        {
            _offset = offset;
            _count = count;
        }

        internal double ToDouble(out bool succes)
        {
            if (_count == 0)
            {
                succes = false;
                return 0;
            }

            int fractional = 0, integer = 0;
            int tmp1 = 1, tmp2 = 1;
            bool f = false;

            for (int i = _offset + _count - 1; i > _offset; --i)
            {
                if (_buffer[i] >= '0' && _buffer[i] <= '9')
                {
                    fractional += (_buffer[i] - '0') * tmp1;
                    tmp1 *= 10;
                }
                else if (_buffer[i] == '.')
                {
                    for (int j = i - 1; j > _offset; --j)
                    {
                        if (_buffer[j] >= '0' && _buffer[j] <= '9')
                        {
                            integer += (_buffer[j] - '0') * tmp2;
                            tmp2 *= 10;
                        }
                        else
                        {
                            succes = false;
                            return 0;
                        }
                    }
                    f = true;
                    break;
                }
                else
                {
                    succes = false;
                    return 0;
                }
            }

            sbyte sign = 1;
            if (_buffer[_offset] >= '0' && _buffer[_offset] <= '9')
            {
                if (f)
                    integer += (_buffer[_offset] - '0') * tmp2;
                else
                    fractional += (_buffer[_offset] - '0') * tmp1;

            }
            else if (_buffer[_offset] == '-')
            {
                sign = -1;
            }
            else if (_buffer[_offset] == '.')
            {
                f = true;
            }
            else
            {
                if (_buffer[_offset] != '+')
                {
                    succes = false;
                    return 0;
                }
            }

            succes = true;
            if (!f)
                return fractional * sign;

            return (integer + fractional * 1.0 / tmp1) * sign;
        }

        internal int ToInt(out bool succes)
        {
            if (_count == 0)
            {
                succes = false;
                return 0;
            }

            int result = 0;
            int tmp = 1;
            for (int i = _offset + _count - 1; i > _offset; --i)
            {
                if (_buffer[i] >= '0' && _buffer[i] <= '9')
                {
                    result += (_buffer[i] - '0') * tmp;
                    tmp *= 10;
                }
                else
                {
                    succes = false;
                    return 0;
                }
            }

            if (_buffer[_offset] >= '0' && _buffer[_offset] <= '9')
            {
                result += (_buffer[_offset] - '0') * tmp;
            }
            else if (_buffer[_offset] == '-')
                result = -result;
            else
            {
                if (_buffer[_offset] != '+')
                {
                    succes = false;
                    return 0;
                }
            }

            succes = true;
            return result;
        }

        internal bool AreEqual(string str)
        {
            if (str.Length != _count)
                return false;

            for (int i = 0; i < _count; ++i)
            {
                if (_buffer[i + _offset] != str[i])
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            char[] result = new char[_count];
            for (int i = 0; i < _count; ++i)
                result[i] = (char)_buffer[_offset + i];
            return new string(result);
        }
    }
}
