using System.IO;

namespace Bytescout.PDF
{
    internal class CCITTFaxStream : FilterStream
    {
	    private readonly Stream _inputStream;
	    private bool _eof;

	    private readonly int _encoding;
	    private readonly bool _endOfLine;
	    private readonly bool _byteAlign;
	    private readonly int _columns;
	    private readonly int _rows;
	    private readonly bool _endOfBlock;
	    private readonly bool _black;

	    private bool _nextLine2D;
	    private int _row;
	    private int _inputBuf;
	    private int _inputBits;
	    private readonly int[] _codingLine;
	    private readonly int[] _refLine;
	    private int _a0i;
	    private bool _error;
	    private int _outputBits;
	    private int _buffer;

	    internal CCITTFaxStream(Stream inputStream, CCITTFaxParameters param)
        {
            _inputStream = inputStream;
            _eof = false;

            _encoding = param.K;
            _endOfLine = param.EndOfLine;
            _byteAlign = param.EncodedByteAlign;
            _columns = param.Columns;
            if (_columns < 1)
                _columns = 1;
            _rows = param.Rows;
            _endOfBlock = param.EndOfBlock;
            _black = param.BlackIs1;

            _codingLine = new int[_columns + 1];
            _refLine = new int[_columns + 2];
            _row = 0;
            _nextLine2D = _encoding < 0;
            _inputBits = 0;
            _codingLine[0] = _columns;
            _a0i = 0;
            _outputBits = 0;
            _buffer = -1;

            int code1;
            while ((code1 = lookBits(12)) == 0)
                eatBits(1);

            if (code1 == 0x001)
            {
                eatBits(12);
                _endOfLine = true;
            }
            if (_encoding > 0)
            {
                _nextLine2D = lookBits(1) == 0;
                eatBits(1);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int b;
            for (int i = 0; i < count; ++i)
            {
                b = ReadByte();
                if (b == -1)
                    return i;
                else
                    buffer[offset + i] = (byte)b;
            }
            return count;
        }

        public override int ReadByte()
        {
            int c = lookByte();
            _buffer = -1;
            return c;
        }

        private void addPixels(int a1, int blackPixels)
        {
            if (a1 > _codingLine[_a0i])
            {
                if (a1 > _columns)
                {
                    _error = true;
                    a1 = _columns;
                }

                if (((_a0i & 1) ^ blackPixels) != 0)
                    ++_a0i;
                _codingLine[_a0i] = a1;
            }
        }

        private void addPixelsNeg(int a1, int blackPixels)
        {
            if (a1 > _codingLine[_a0i])
            {
                if (a1 > _columns)
                {
                    _error = true;
                    a1 = _columns;
                }

                if (((_a0i & 1) ^ blackPixels) != 0)
                    ++_a0i;
                _codingLine[_a0i] = a1;
            }
            else if (a1 < _codingLine[_a0i])
            {
                if (a1 < 0)
                {
                    _error = true;
                    a1 = 0;
                }

                while (_a0i > 0 && a1 <= _codingLine[_a0i - 1])
                    --_a0i;
                _codingLine[_a0i] = a1;
            }
        }

        private short lookBits(int n)
        {
            int c;
            while (_inputBits < n)
            {
                if ((c = _inputStream.ReadByte()) == -1)
                {
                    if (_inputBits == 0)
                        return -1;
                    return (short)((_inputBuf << (n - _inputBits)) & (0xffffffff >> (32 - n)));
                }

                _inputBuf = (_inputBuf << 8) + c;
                _inputBits += 8;
            }
            return (short)((_inputBuf >> (_inputBits - n)) & (0xffffffff >> (32 - n)));
        }

        private void eatBits(int n)
        {
            if ((_inputBits -= n) < 0) _inputBits = 0;
        }

        private short getTwoDimCode()
        {
            int code = 0, n;
            uint p;

            if (_endOfBlock)
            {
                if ((code = lookBits(7)) != -1)
                {
                    p = CCITTCodeTables.TwoDimTab1[code];
                    if ((short)p > 0)
                    {
                        eatBits((short)p);
                        return (short)(p >> 16);
                    }
                }
            }
            else
            {
                for (n = 1; n <= 7; ++n)
                {
                    if ((code = lookBits(n)) == -1)
                        break;
                    if (n < 7)
                        code <<= 7 - n;

                    p = CCITTCodeTables.TwoDimTab1[code];
                    if ((short)p == n)
                    {
                        eatBits(n);
                        return (short)(p >> 16);
                    }
                }
            }

            return -1;
        }

        private short getWhiteCode()
        {
            short code = 0, n;
            uint p;

            if (_endOfBlock)
            {
                code = lookBits(12);
                if (code == -1)
                    return 1;

                if ((code >> 5) == 0)
                    p = CCITTCodeTables.WhiteTab1[code];
                else
                    p = CCITTCodeTables.WhiteTab2[code >> 3];

                if ((short)p > 0)
                {
                    eatBits((short)p);
                    return (short)(p >> 16);
                }
            }
            else
            {
                for (n = 1; n <= 9; ++n)
                {
                    code = lookBits(n);
                    if (code == -1)
                        return 1;
                    if (n < 9)
                        code <<= 9 - n;

                    p = CCITTCodeTables.WhiteTab2[code];
                    if ((short)p == n)
                    {
                        eatBits(n);
                        return (short)(p >> 16);
                    }
                }
                for (n = 11; n <= 12; ++n)
                {
                    code = lookBits(n);
                    if (code == -1)
                        return 1;
                    if (n < 12)
                        code <<= 12 - n;

                    p = CCITTCodeTables.WhiteTab1[code];
                    if ((short)p == n)
                    {
                        eatBits(n);
                        return (short)(p >> 16);
                    }
                }
            }

            eatBits(1);
            return 1;
        }

        private short getBlackCode()
        {
            short code = 0, n;
            uint p;

            if (_endOfBlock)
            {
                code = lookBits(13);
                if (code == -1)
                    return 1;

                if ((code >> 7) == 0)
                    p = CCITTCodeTables.BlackTab1[code];
                else if ((code >> 9) == 0 && (code >> 7) != 0)
                    p = CCITTCodeTables.BlackTab2[(code >> 1) - 64];
                else
                    p = CCITTCodeTables.BlackTab3[code >> 7];

                if ((short)p > 0)
                {
                    eatBits((short)p);
                    return (short)(p >> 16);
                }
            }
            else
            {
                for (n = 2; n <= 6; ++n)
                {
                    code = lookBits(n);
                    if (code == -1)
                        return 1;
                    if (n < 6)
                        code <<= 6 - n;

                    p = CCITTCodeTables.BlackTab3[code];
                    if ((short)p == n)
                    {
                        eatBits(n);
                        return (short)(p >> 16);
                    }
                }
                for (n = 7; n <= 12; ++n)
                {
                    code = lookBits(n);
                    if (code == -1)
                        return 1;
                    if (n < 12)
                        code <<= 12 - n;

                    if (code >= 64)
                    {
                        p = CCITTCodeTables.BlackTab2[code - 64];
                        if ((short)p == n)
                        {
                            eatBits(n);
                            return (short)(p >> 16);
                        }
                    }
                }
                for (n = 10; n <= 13; ++n)
                {
                    code = lookBits(n);
                    if (code == -1)
                        return 1;
                    if (n < 13)
                        code <<= 13 - n;

                    p = CCITTCodeTables.BlackTab1[code];
                    if ((short)p == n)
                    {
                        eatBits(n);
                        return (short)(p >> 16);
                    }
                }
            }

            eatBits(1);
            return 1;
        }

        private int lookByte()
        {
            int code1 = 0, code2 = 0, code3 = 0, b1i = 0, blackPixels = 0, i = 0, bits = 0;
            bool gotEOL;

            if (_buffer != -1)
                return _buffer;

            // read the next row
            if (_outputBits == 0)
            {
                if (_eof)
                    return -1;
                _error = false;

                if (_nextLine2D)
                    process2DEncoding(ref i, ref b1i, ref blackPixels, ref code1, ref code2, ref code3);
                else
                    process1DEncoding(ref code1, ref code3, ref blackPixels);

                gotEOL = false;

                if (!_endOfBlock && _row == _rows - 1)
                    _eof = true;
                else if (_endOfLine || !_byteAlign)
                {
                    code1 = lookBits(12);
                    if (_endOfLine)
                    {
                        while (code1 != -1 && code1 != 0x001)
                        {
                            eatBits(1);
                            code1 = lookBits(12);
                        }
                    }
                    else
                    {
                        while (code1 == 0)
                        {
                            eatBits(1);
                            code1 = lookBits(12);
                        }
                    }
                    if (code1 == 0x001)
                    {
                        eatBits(12);
                        gotEOL = true;
                    }
                }

                if (_byteAlign && !gotEOL)
                    _inputBits &= ~7;

                if (lookBits(1) == -1)
                    _eof = true;

                // get 2D encoding tag
                if (!_eof && _encoding > 0)
                {
                    _nextLine2D = lookBits(1) == 0;
                    eatBits(1);
                }

                // check for end-of-block marker
                if (_endOfBlock && !_endOfLine && _byteAlign)
                {
                    code1 = lookBits(24);
                    if (code1 == 0x001001)
                    {
                        eatBits(12);
                        gotEOL = true;
                    }
                }

                if (_endOfBlock && gotEOL)
                {
                    code1 = lookBits(12);
                    if (code1 == 0x001)
                    {
                        eatBits(12);
                        if (_encoding > 0)
                        {
                            lookBits(1);
                            eatBits(1);
                        }

                        if (_encoding >= 0)
                        {
                            for (i = 0; i < 4; ++i)
                            {
                                code1 = lookBits(12);
                                if (code1 != 0x001)
                                    _error = true;

                                eatBits(12);
                                if (_encoding > 0)
                                {
                                    lookBits(1);
                                    eatBits(1);
                                }
                            }
                        }

                        _eof = true;
                    }
                }
                else if (_error && _endOfLine)
                {
                    while (true)
                    {
                        code1 = lookBits(13);
                        if (code1 == -1)
                        {
                            _eof = true;
                            return -1;
                        }

                        if ((code1 >> 1) == 0x001)
                            break;
                        eatBits(1);
                    }
                    eatBits(12);
                    if (_encoding > 0)
                    {
                        eatBits(1);
                        _nextLine2D = (code1 & 1) == 0;
                    }
                }

                // set up for output
                if (_codingLine[0] > 0)
                    _outputBits = _codingLine[_a0i = 0];
                else
                    _outputBits = _codingLine[_a0i = 1];
                ++_row;
            }
            
            return getByte(bits);
        }

        private int getByte(int bits)
        {
            if (_outputBits >= 8)
            {
                _buffer = ((_a0i & 1) != 0) ? 0x00 : 0xff;
                _outputBits -= 8;
                if (_outputBits == 0 && _codingLine[_a0i] < _columns)
                {
                    ++_a0i;
                    _outputBits = _codingLine[_a0i] - _codingLine[_a0i - 1];
                }
            }
            else
            {
                bits = 8;
                _buffer = 0;
                do
                {
                    if (_outputBits > bits)
                    {
                        _buffer <<= bits;
                        if ((_a0i & 1) == 0)
                            _buffer |= 0xff >> (8 - bits);

                        _outputBits -= bits;
                        bits = 0;
                    }
                    else
                    {
                        _buffer <<= _outputBits;
                        if ((_a0i & 1) == 0)
                            _buffer |= 0xff >> (8 - _outputBits);

                        bits -= _outputBits;
                        _outputBits = 0;

                        if (_codingLine[_a0i] < _columns)
                        {
                            ++_a0i;
                            _outputBits = _codingLine[_a0i] - _codingLine[_a0i - 1];
                        }
                        else if (bits > 0)
                        {
                            _buffer <<= bits;
                            bits = 0;
                        }
                    }
                } while (bits != 0);
            }

            if (_black)
                _buffer ^= 0xff;

            return _buffer;
        }

        private void process2DEncoding(ref int i, ref int b1i, ref int blackPixels, ref int code1, ref int code2, ref int code3)
        {
            for (i = 0; _codingLine[i] < _columns; ++i)
                _refLine[i] = _codingLine[i];

            _refLine[i++] = _columns;
            _refLine[i] = _columns;
            _codingLine[0] = 0;
            _a0i = 0;
            b1i = 0;
            blackPixels = 0;

            while (_codingLine[_a0i] < _columns)
            {
                code1 = getTwoDimCode();
                switch (code1)
                {
                    case CCITTCodeTables.TwoDimPass:
                        addPixels(_refLine[b1i + 1], blackPixels);
                        if (_refLine[b1i + 1] < _columns)
                            b1i += 2;
                        break;
                    case CCITTCodeTables.TwoDimHoriz:
                        code1 = code2 = 0;
                        if (blackPixels != 0)
                        {
                            do
                                code1 += code3 = getBlackCode();
                            while (code3 >= 64);
                            do
                                code2 += code3 = getWhiteCode();
                            while (code3 >= 64);
                        }
                        else
                        {
                            do
                                code1 += code3 = getWhiteCode();
                            while (code3 >= 64);
                            do
                                code2 += code3 = getBlackCode();
                            while (code3 >= 64);
                        }

                        addPixels(_codingLine[_a0i] + code1, blackPixels);
                        if (_codingLine[_a0i] < _columns)
                            addPixels(_codingLine[_a0i] + code2, blackPixels ^ 1);

                        while (_refLine[b1i] <= _codingLine[_a0i] && _refLine[b1i] < _columns)
                            b1i += 2;
                        break;
                    case CCITTCodeTables.TwoDimVertR3:
                        addPixels(_refLine[b1i] + 3, blackPixels);
                        blackPixels ^= 1;
                        if (_codingLine[_a0i] < _columns)
                        {
                            ++b1i;
                            while (_refLine[b1i] <= _codingLine[_a0i] && _refLine[b1i] < _columns)
                                b1i += 2;
                        }
                        break;
                    case CCITTCodeTables.TwoDimVertR2:
                        addPixels(_refLine[b1i] + 2, blackPixels);
                        blackPixels ^= 1;
                        if (_codingLine[_a0i] < _columns)
                        {
                            ++b1i;
                            while (_refLine[b1i] <= _codingLine[_a0i] && _refLine[b1i] < _columns)
                                b1i += 2;
                        }
                        break;
                    case CCITTCodeTables.TwoDimVertR1:
                        addPixels(_refLine[b1i] + 1, blackPixels);
                        blackPixels ^= 1;
                        if (_codingLine[_a0i] < _columns)
                        {
                            ++b1i;
                            while (_refLine[b1i] <= _codingLine[_a0i] && _refLine[b1i] < _columns)
                            {
                                b1i += 2;
                            }
                        }
                        break;
                    case CCITTCodeTables.TwoDimVert0:
                        addPixels(_refLine[b1i], blackPixels);
                        blackPixels ^= 1;
                        if (_codingLine[_a0i] < _columns)
                        {
                            ++b1i;
                            while (_refLine[b1i] <= _codingLine[_a0i] && _refLine[b1i] < _columns)
                                b1i += 2;
                        }
                        break;
                    case CCITTCodeTables.TwoDimVertL3:
                        addPixelsNeg(_refLine[b1i] - 3, blackPixels);
                        blackPixels ^= 1;
                        if (_codingLine[_a0i] < _columns)
                        {
                            if (b1i > 0)
                                --b1i;
                            else
                                ++b1i;

                            while (_refLine[b1i] <= _codingLine[_a0i] && _refLine[b1i] < _columns)
                                b1i += 2;
                        }
                        break;
                    case CCITTCodeTables.TwoDimVertL2:
                        addPixelsNeg(_refLine[b1i] - 2, blackPixels);
                        blackPixels ^= 1;
                        if (_codingLine[_a0i] < _columns)
                        {
                            if (b1i > 0)
                                --b1i;
                            else
                                ++b1i;

                            while (_refLine[b1i] <= _codingLine[_a0i] && _refLine[b1i] < _columns)
                                b1i += 2;
                        }
                        break;
                    case CCITTCodeTables.TwoDimVertL1:
                        addPixelsNeg(_refLine[b1i] - 1, blackPixels);
                        blackPixels ^= 1;
                        if (_codingLine[_a0i] < _columns)
                        {
                            if (b1i > 0)
                                --b1i;
                            else
                                ++b1i;

                            while (_refLine[b1i] <= _codingLine[_a0i] && _refLine[b1i] < _columns)
                                b1i += 2;
                        }
                        break;
                    case -1:
                        addPixels(_columns, 0);
                        _eof = true;
                        break;
                    default:
                        addPixels(_columns, 0);
                        _error = true;
                        break;
                }
            }
        }

        private void process1DEncoding(ref int code1, ref int code3, ref int blackPixels)
        {
            _codingLine[0] = 0;
            _a0i = 0;
            blackPixels = 0;

            while (_codingLine[_a0i] < _columns)
            {
                code1 = 0;
                if (blackPixels != 0)
                {
                    do
                        code1 += code3 = getBlackCode();
                    while (code3 >= 64);
                }
                else
                {
                    do
                        code1 += code3 = getWhiteCode();
                    while (code3 >= 64);
                }

                addPixels(_codingLine[_a0i] + code1, blackPixels);
                blackPixels ^= 1;
            }
        }
    }
}
