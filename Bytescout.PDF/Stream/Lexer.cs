using System;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class Lexer
    {
	    private Stream _stream;
	    private MemoryStream _bufferedStream;
	    private long _length;
	    private int _lastParsedByte = -1;

	    private bool _usedLastParsedNumber = false;
	    private bool _lastParsedNumberIsInteger = false;
	    private double _lastParsedNumber;

	    private XRef _xref;
	    private Encryptor _encryptor;

	    private static System.Text.Encoding _utf8 = new UTF8Encoding(false, true);

	    internal long Position { set { _stream.Position = value; } }

        internal long Length { get { return _length; } }

        internal int LastParsedByte
        {
            get { return _lastParsedByte; }
            set { _lastParsedByte = value; }
        }

	    internal Lexer(Stream stream, XRef xref, Encryptor encryptor, int bufferSize)
	    {
		    _stream = stream;
		    _bufferedStream = new MemoryStream(bufferSize);
		    _length = _stream.Length;
		    _xref = xref;
		    _encryptor = encryptor;
            _lastParsedByte = 0;
        }

	    internal void SetStream(Stream stream)
        {
            _stream = stream;
            _lastParsedByte = 0;
        }

        internal void SetEncryptor(Encryptor encryptor)
        {
            _encryptor = encryptor;
        }

        internal int ReadInteger(out bool succes)
        {
            ReadLexeme();
            return toInt(_bufferedStream.GetBuffer(), (int)_bufferedStream.Length, out succes);
        }

        internal void ReadLexeme()
        {
            _bufferedStream.SetLength(0);
            SkipEOL();

            int b = _lastParsedByte;
            for (; ; )
            {
                _bufferedStream.WriteByte((byte)b);
                b = _stream.ReadByte();
                if (IsEOL(b) || IsSpecialCharacter(b) || b == -1)
                    break;
            }

            _lastParsedByte = b;
        }

        internal void ReadLexemeWithLastParsedByte()
        {
            _bufferedStream.SetLength(0);
            int b = _lastParsedByte;
            for (; ; )
            {
                _bufferedStream.WriteByte((byte)b);
                b = _stream.ReadByte();
                if (IsEOL(b) || IsSpecialCharacter(b) || b == -1)
                    break;
            }
            _lastParsedByte = b;
        }

        internal int ReadByte()
        {
            return _stream.ReadByte();
        }

        internal int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        internal bool CurrentLexemeEquals(string s)
        {
            if (s.Length != _bufferedStream.Length)
                return false;

            byte[] buf = _bufferedStream.GetBuffer();
            int len = (int)_bufferedStream.Length;
            for (int i = 0; i < len; ++i)
            {
                if (buf[i] != s[i])
                    return false;
            }
            return true;
        }

        internal int CurrentLexemeToInteger(out bool succes)
        {
            return toInt(_bufferedStream.GetBuffer(), (int)_bufferedStream.Length, out succes);
        }

        internal double CurrentLexemeToDouble(out bool succes)
        {
            return toDouble(_bufferedStream.GetBuffer(), (int)_bufferedStream.Length, out succes);
        }

        internal string CurrentLexemeToString()
        {
            return System.Text.Encoding.ASCII.GetString(_bufferedStream.GetBuffer(), 0, (int)_bufferedStream.Length);
        }

        internal long FindLastSubstring(string s)
        {
            byte[] buf = _bufferedStream.GetBuffer();
            int bufLen = buf.Length;

            long curPos;
            if (_length < bufLen)
                curPos = 0;
            else
                curPos = _length - bufLen;
            
            int finder, numRead;
            for (; ;)
            {
                _stream.Position = curPos;
                numRead = _stream.Read(buf, 0, bufLen);
                finder = findLast(buf, numRead, s);
                if (finder < 0)
                {
                    if (curPos == 0)
                        return -1;

                    curPos -= bufLen - s.Length + 1;
                    if (curPos < 0)
                    {
                        bufLen += (int)curPos;
                        curPos = 0;
                    }
                }
                else
                {
                    return curPos + finder;
                }
            }
        }

        internal long FindSubstring(string s, long start, long end)
        {
            if (start >= _length)
                return -1;

            byte[] buf = _bufferedStream.GetBuffer();
            int bufLen = buf.Length;

            long curPos = start;
            int finder, numRead;
            for (; ; )
            {
                _stream.Position = curPos;
                numRead = _stream.Read(buf, 0, bufLen);
                finder = findFirst(buf, numRead, s);
                if (finder < 0)
                {
                    curPos += numRead - s.Length + 1;
                    if (curPos + bufLen > end)
                        bufLen = (int)(end - curPos);
                    if (curPos + s.Length - 1 >= end)
                        return -1;
                }
                else
                {
                    return curPos + finder;
                }
            }
        }

        internal int ReadHexValue()
        {
            _bufferedStream.SetLength(0);
            int hi = _stream.ReadByte(), lo;
            for (; ; )
            {
                if (IsEOL(hi))
                {
                    SkipEOL();
                    hi = _lastParsedByte;
                }
                if (hi == '>')
                    break;
                if (!isHex(hi))
                    return 0;

                lo = _stream.ReadByte();
                if (IsEOL(lo))
                {
                    SkipEOL();
                    lo = _lastParsedByte;
                }
                if (lo == '>')
                {
                    hi = hexToInt(hi);
                    _bufferedStream.WriteByte((byte)(hi << 4));
                    break;
                }
                if (!isHex(lo))
                    return 0;

                hi = hexToInt(hi);
                lo = hexToInt(lo);
                _bufferedStream.WriteByte((byte)((hi << 4) + lo));

                hi = _stream.ReadByte();
            }

            _lastParsedByte = _stream.ReadByte();
            return (int)_bufferedStream.Length;
        }

        internal byte GetLexemeHexByte()
        {
            return _bufferedStream.GetBuffer()[0];
        }

        internal int GetLexemeHex2Bytes()
        {
            byte[] buf = _bufferedStream.GetBuffer();
            return buf[0] * 256 + buf[1];
        }

        internal string GetLexemeHexLigature()
        {
            byte[] buf = _bufferedStream.GetBuffer();
            int count = (int)(_bufferedStream.Length / 2);
            char[] result = new char[count];
            for (int i = 0; i < count; ++i)
                result[i] = (char)(buf[2 * i] * 256 + buf[2 * i + 1]);
            return new string(result);
        }

        internal IPDFObject ParseEntry(int index, int offset, int genNo)
        {
            bool succes = false;
            if (offset >= 0)
            {
                _stream.Position = offset;
                int objNo = ReadInteger(out succes);
                if (succes)
                {
                    succes = (index == objNo);
                    if (succes)
                    {
                        ReadInteger(out succes);
                        if (succes)
                        {
                            ReadLexeme();
                            if (!CurrentLexemeEquals("obj"))
                                succes = false;
                        }
                    }
                }
            }

            if (!succes)
            {
                string s = (char)256 + index.ToString() + " 0 obj";
                long pos = FindSubstring(s, 0, _length);
                if (pos < 0)
                    return new PDFNull();
                _stream.Position = pos + s.Length;
                _lastParsedByte = _stream.ReadByte();
            }

            IPDFObject obj = readObject(_lastParsedByte, index, genNo);
            if (obj is PDFDictionary)
            {
                if (IsEOL(_lastParsedByte))
                    SkipEOL();
                if (_lastParsedByte == 's')
                    return parseStream(obj as PDFDictionary, index, genNo);
            }

            return obj;
        }

        internal PDFDictionaryStream ParseCrossRefObject()
        {
            bool succes;
            ReadInteger(out succes);
            if (!succes)
                return null;
            ReadLexeme();
            if (!CurrentLexemeEquals("obj"))
                return null;

            PDFDictionary obj = readObject(_lastParsedByte, 0, 0) as PDFDictionary;
            if (obj == null)
                return null;

            PDFName type = obj["Type"] as PDFName;
            if (type == null || type.GetValue() != "XRef")
                return null;

            if (IsEOL(_lastParsedByte))
                SkipEOL();
            if (_lastParsedByte != 's')
                return null;

            return parseStream(obj, 0, 0) as PDFDictionaryStream;
        }

        public void SkipEOL()
        {
            _lastParsedByte = _stream.ReadByte();
            while (IsEOL(_lastParsedByte))
                _lastParsedByte = _stream.ReadByte();
        }

        private IPDFObject parseStream(PDFDictionary dict, int objNo, int genNo)
        {
            if (_stream.ReadByte() != 't')
                return dict;
            if (_stream.ReadByte() != 'r')
                return dict;
            if (_stream.ReadByte() != 'e')
                return dict;
            if (_stream.ReadByte() != 'a')
                return dict;
            if (_stream.ReadByte() != 'm')
                return dict;

            int b = _stream.ReadByte();
            long startPosition;
            for (; ; )
            {
                if (b == -1)
                    return null;
                if (b == '\n')
                {
                    startPosition = _stream.Position;
                    break;
                }
                else if (b == '\r')
                {
                    b = _stream.ReadByte();
                    if (b == '\n')
                    {
                        startPosition = _stream.Position;
                        break;
                    }
                    startPosition = _stream.Position - 1;
                    break;

                }
                b = _stream.ReadByte();
            }

            PDFNumber lengthObj = dict["Length"] as PDFNumber;
            bool wrongLength = false;

            if (lengthObj == null)
                wrongLength = true;

            int length = 0;
            if (lengthObj != null)
            {
                length = (int)lengthObj.GetValue();
                if (length < 0)
                    wrongLength = true;
                if (startPosition + length >= _length)
                    wrongLength = true;
            }

            if (!wrongLength)
            {
                _stream.Position = startPosition + length;

                ReadLexeme();
                if (!CurrentLexemeEquals("endstream"))
                    wrongLength = true;
            }

            if (wrongLength)
            {
                long endPos = findEndStream(startPosition, _length);
                if (endPos < 0)
                    return null;
                length = (int)(endPos - startPosition);
            }

            MemoryStream ms = new MemoryStream(length);
            if (_encryptor == null)
            {
                ms.SetLength(length);
                byte[] buf = ms.GetBuffer();
                _stream.Position = startPosition;
                _stream.Read(buf, 0, length);
            }
            else
            {
                _encryptor.ResetObjectReference(objNo, genNo, DataType.String);
                _stream.Position = startPosition;
                _encryptor.Decrypt(_stream, length, ms, DataType.Stream);
            }

            return new PDFDictionaryStream(dict, ms);
        }

        internal IPDFObject ReadObject()
        {
            _usedLastParsedNumber = false;
            return readObject(0, 0, 0);
        }

        internal IPDFObject ReadObjectWithLastParsedByte()
        {
            _usedLastParsedNumber = false;
            return readObject(_lastParsedByte, 0, 0);
        }

        internal void ReadComment()
        {
            int b = _stream.ReadByte();
            for (; ; )
            {
                if (b == '\r' || b == '\n')
                    break;
                b = _stream.ReadByte();
            }

            _lastParsedByte = b;
        }

        internal PDFNumber ParseNumber(int firstByte)
        {
            readLexeme(firstByte);
            bool succes;
            double val = CurrentLexemeToDouble(out succes);
            if (!succes)
                return null;
            return new PDFNumber(val);
        }

        internal PDFArray ParseArray(int objNo, int genNo)
        {
            PDFArray arr = new PDFArray();
            int b = _stream.ReadByte();

            for (; ; )
            {
                if (IsEOL(b))
                {
                    SkipEOL();
                    b = _lastParsedByte;
                }

                if (!_usedLastParsedNumber)
                    if (b == ']')
                        break;

                IPDFObject obj = readObject(b, objNo, genNo);
                if (obj == null)
                    return null;
                arr.AddItem(obj);
                b = _lastParsedByte;
            }

            _lastParsedByte = _stream.ReadByte();
            return arr;
        }

        internal PDFName ParseName()
        {
            return new PDFName(readNameBytes());
        }

        public PDFString ParseString(int objNo, int genNo)
        {
            _bufferedStream.SetLength(0);
            if (readStringBytes(true))
            {
                if (_encryptor != null)
                {
                    _encryptor.ResetObjectReference(objNo, genNo, DataType.String);
                    byte[] buf = _bufferedStream.GetBuffer();
                    int l = (int)_bufferedStream.Length;
                    _bufferedStream.SetLength(0);
                    _encryptor.Decrypt(buf, 0, l, _bufferedStream, DataType.String);
                }

                byte[] result = new byte[_bufferedStream.Length];
                Array.Copy(_bufferedStream.GetBuffer(), result, result.Length);
                _lastParsedByte = _stream.ReadByte();

                return new PDFString(result, false);
            }

            return null;
        }

        internal IPDFObject ParseHexStringOrDictionary(int objNo, int genNo)
        {
            int b = _stream.ReadByte();
            if (b == '<')
                return parseDictionary(objNo, genNo);
            else
                return ParseHexString(b, objNo, genNo);
        }

        private IPDFObject readObject(int lastParsedByte, int objNo, int genNo)
        {
            if (_usedLastParsedNumber)
            {
                return parseNumberOrLink(_lastParsedByte);
            }

            if (IsEOL(lastParsedByte))
            {
                SkipEOL();
                lastParsedByte = _lastParsedByte;
            }

            switch (lastParsedByte)
            {
                case '<':
                    return ParseHexStringOrDictionary(objNo, genNo);
                case '[':
                    return ParseArray(objNo, genNo);
                case '(':
                    return ParseString(objNo, genNo);
                case '/':
                    return ParseName();
                case '-':
                case '.':
                case '+':
                    return ParseNumber(lastParsedByte);
                case 'n':
                    return ParseNull();
                case 't':
                case 'f':
                    return parseBoolean(lastParsedByte);
                case '%':
                    ReadComment();
                    return readObject(_lastParsedByte, objNo, genNo);
            }

            if (isNumberPart(lastParsedByte))
                return parseNumberOrLink(lastParsedByte);

            return null;
        }

        private long findEndStream(long start, long end)
        {
            long endPos = FindSubstring("endstream", start, end);
            if (endPos < 0)
                return -1;
            _stream.Position = endPos + 9;
            ReadLexeme();
            if (CurrentLexemeEquals("endobj"))
            {
                _stream.Position = endPos - 2;
                int eod1 = _stream.ReadByte();
                int eod2 = _stream.ReadByte();
                if ((IsEOL(eod1) && eod1 != 0) && (IsEOL(eod2) && eod2 != 0))
                    return endPos - 2;
                if (IsEOL(eod2) && eod2 != 0)
                    return endPos - 1;
                return endPos;
            }
            else
                return findEndStream(endPos + 1, end);
        }

        private PDFBoolean parseBoolean(int firstByte)
        {
            if (firstByte == 't')
            {
                if (_stream.ReadByte() != 'r')
                    return null;
                if (_stream.ReadByte() != 'u')
                    return null;
                if (_stream.ReadByte() != 'e')
                    return null;

                _lastParsedByte = _stream.ReadByte();
                return new PDFBoolean(true);
            }
            else if (firstByte == 'f')
            {
                if (_stream.ReadByte() != 'a')
                    return null;
                if (_stream.ReadByte() != 'l')
                    return null;
                if (_stream.ReadByte() != 's')
                    return null;
                if (_stream.ReadByte() != 'e')
                    return null;

                _lastParsedByte = _stream.ReadByte();
                return new PDFBoolean(false);
            }

            return null;
        }

        private PDFNull ParseNull()
        {
            if (_stream.ReadByte() != 'u')
                return null;
            if (_stream.ReadByte() != 'l')
                return null;
            if (_stream.ReadByte() != 'l')
                return null;

            _lastParsedByte = _stream.ReadByte();
            return new PDFNull();
        }

        private PDFString ParseHexString(int firstByte, int objNo, int genNo)
        {
            _bufferedStream.SetLength(0);
            int hi = firstByte, lo;
            for (; ; )
            {
                if (IsEOL(hi))
                {
                    SkipEOL();
                    hi = _lastParsedByte;
                }
                if (hi == '>')
                    break;
                if (!isHex(hi))
                    return null;

                lo = _stream.ReadByte();
                if (IsEOL(lo))
                {
                    SkipEOL();
                    lo = _lastParsedByte;
                }
                if (lo == '>')
                {
                    hi = hexToInt(hi);
                    _bufferedStream.WriteByte((byte)(hi << 4));
                    break;
                }
                if (!isHex(lo))
                    return null;

                hi = hexToInt(hi);
                lo = hexToInt(lo);
                _bufferedStream.WriteByte((byte)((hi << 4) + lo));

                hi = _stream.ReadByte();
            }

            _lastParsedByte = _stream.ReadByte();

            if (_encryptor != null)
            {
                _encryptor.ResetObjectReference(objNo, genNo, DataType.String);
                byte[] buf = _bufferedStream.GetBuffer();
                int l = (int)_bufferedStream.Length;
                _bufferedStream.SetLength(0);
                _encryptor.Decrypt(buf, 0, l, _bufferedStream, DataType.String);
            }

            byte[] result = new byte[_bufferedStream.Length];
            Array.Copy(_bufferedStream.GetBuffer(), result, result.Length);

            return new PDFString(result, true);
        }

        private PDFDictionary parseDictionary(int objNo, int genNo)
        {
            PDFDictionary dict = new PDFDictionary();
            int b = _stream.ReadByte();
            for (; ; )
            {
                if (IsEOL(b))
                {
                    SkipEOL();
                    b = _lastParsedByte;
                }
                if (b == '%')
                {
                    ReadComment();
                    b = _lastParsedByte;
                    continue;
                }

                if (b == '>')
                    break;
                if (b != '/')
                    return null;

                string key = readNameBytes();
                IPDFObject obj = readObject(_lastParsedByte, objNo, genNo);
                b = _lastParsedByte;

                if (obj == null)
                    return null;
                dict.AddItem(key, obj);
            }

            _stream.ReadByte();
            _lastParsedByte = _stream.ReadByte();
            return dict;
        }

        private IPDFObject parseNumberOrLink(int firstByte)
        {
            bool succes;
            if (_usedLastParsedNumber)
            {
                _usedLastParsedNumber = false;
                if (!_lastParsedNumberIsInteger)
                    return new PDFNumber(_lastParsedNumber);

                if (isNumberPart(firstByte))
                {
                    readLexeme(firstByte);

                    if (IsEOL(_lastParsedByte))
                        SkipEOL();
                    if (_lastParsedByte == 'R')
                    {
                        int second = CurrentLexemeToInteger(out succes);
                        if (!succes)
                            return null;

                        _lastParsedByte = _stream.ReadByte();
                        return new PDFLink(_xref, (int)_lastParsedNumber);
                    }

                    PDFNumber obj = new PDFNumber(_lastParsedNumber);
                    int number = CurrentLexemeToInteger(out succes);
                    if (!succes)
                    {
                        double n = CurrentLexemeToDouble(out succes);
                        if (!succes)
                            return null;
                        _usedLastParsedNumber = true;
                        _lastParsedNumberIsInteger = false;
                        _lastParsedNumber = n;
                    }
                    else
                    {
                        _usedLastParsedNumber = true;
                        _lastParsedNumberIsInteger = true;
                        _lastParsedNumber = number;
                    }
                    return obj;
                }
                else
                {
                    return new PDFNumber(_lastParsedNumber);;
                }
            }
            else
            {
                readLexeme(firstByte);
                if (IsEOL(_lastParsedByte))
                    SkipEOL();
                
                if (!isNumberPart(_lastParsedByte))
                {
                    double val = CurrentLexemeToDouble(out succes);
                    if (!succes)
                        return null;
                    return new PDFNumber(val);
                }

                int first = CurrentLexemeToInteger(out succes);
                if (!succes)
                {
                    double val = CurrentLexemeToDouble(out succes);
                    if (!succes)
                        return null;
                    return new PDFNumber(val);
                }
                
                readLexeme(_lastParsedByte);
                if (IsEOL(_lastParsedByte))
                    SkipEOL();
                if (_lastParsedByte == 'R')
                {
                    int second = CurrentLexemeToInteger(out succes);
                    if (!succes)
                        return null;

                    _lastParsedByte = _stream.ReadByte();
                    return new PDFLink(_xref, first);
                }

                int number = CurrentLexemeToInteger(out succes);
                if (!succes)
                {
                    double n = CurrentLexemeToDouble(out succes);
                    if (!succes)
                        return null;
                    _usedLastParsedNumber = true;
                    _lastParsedNumberIsInteger = false;
                    _lastParsedNumber = n;
                }
                else
                {
                    _usedLastParsedNumber = true;
                    _lastParsedNumberIsInteger = true;
                    _lastParsedNumber = number;
                }

                return new PDFNumber(first);
            }
        }

        private bool readStringBytes(bool start)
        {
            int b = _stream.ReadByte();
            for (; ; )
            {
                if (b == '(')
                {
                    _bufferedStream.WriteByte((byte)'(');
                    readStringBytes(false);
                }
                else if (b == ')')
                {
                    if (!start)
                        _bufferedStream.WriteByte((byte)')');
                    return true;
                }
                else if (b == '\\')
                {
                    b = _stream.ReadByte();
                    if (b == -1)
                        return false;
                    if (isOctal(b))
                    {
                        int second = _stream.ReadByte();
                        if (isOctal(second))
                        {
                            int third = _stream.ReadByte();
                            if (isOctal(third))
                            {
                                int res = octToInt(b, second, third);
                                if (res > 255)
                                {
                                    _bufferedStream.WriteByte((byte)octToInt(b, second, 0));
                                    b = third;
                                    continue;
                                }
                                else
                                {
                                    _bufferedStream.WriteByte((byte)res);
                                }
                            }
                            else
                            {
                                _bufferedStream.WriteByte((byte)octToInt(b, second, 0));
                                b = third;
                                continue;
                            }
                        }
                        else
                        {
                            _bufferedStream.WriteByte((byte)octToInt(b, 0, 0));
                            b = second;
                            continue;
                        }
                    }
                    else
                    {
                        switch (b)
                        {
                            case '\r':
                            case '\n':
                                break;
                            case 'r':
                                _bufferedStream.WriteByte((byte)'\r');
                                break;
                            case 't':
                                _bufferedStream.WriteByte((byte)'\t');
                                break;
                            case 'n':
                                _bufferedStream.WriteByte((byte)'\n');
                                break;
                            case 'b':
                                _bufferedStream.WriteByte((byte)'\b');
                                break;
                            case 'f':
                                _bufferedStream.WriteByte((byte)'\f');
                                break;
                            default:
                                _bufferedStream.WriteByte((byte)b);
                                break;
                        }
                    }

                }
                else if (b == -1)
                    return false;
                else
                    _bufferedStream.WriteByte((byte)b);

                b = _stream.ReadByte();
            }
        }

        private string readNameBytes()
        {
            _bufferedStream.SetLength(0);
            int b = _stream.ReadByte();
            for (; ; )
            {
                if (IsEOL(b) || IsSpecialCharacter(b) || (b == -1))
                    break;
                if (b == '#')
                {
                    int b1 = _stream.ReadByte();
                    if (IsEOL(b1) || IsSpecialCharacter(b1) || (b1 == -1))
                    {
                        _bufferedStream.WriteByte((byte)b);
                        break;
                    }
                    if (!isHex(b1))
                    {
                        _bufferedStream.WriteByte((byte)b);
                        b = b1;
                    }
                    else
                    {
                        int b2 = _stream.ReadByte();
                        if (IsEOL(b2) || IsSpecialCharacter(b2) || (b2 == -1))
                        {
                            _bufferedStream.WriteByte((byte)b);
                            _bufferedStream.WriteByte((byte)b1);
                            break;
                        }
                        if (!isHex(b1))
                        {
                            _bufferedStream.WriteByte((byte)b);
                            _bufferedStream.WriteByte((byte)b1);
                            b = b2;
                        }
                        else
                        {
                            b1 = hexToInt(b1);
                            b2 = hexToInt(b2);
                            _bufferedStream.WriteByte((byte)((b1 << 4) + b2));
                            b = _stream.ReadByte();
                            continue;
                        }
                    }
                }

                _bufferedStream.WriteByte((byte)b);
                b = _stream.ReadByte();
            }
            _lastParsedByte = b;

            byte[] buf = _bufferedStream.GetBuffer();
            int len = (int)_bufferedStream.Length;
            try
            {
                return _utf8.GetString(buf, 0, len);
            }
            catch
            {
                return System.Text.Encoding.Default.GetString(buf, 0, len);
            }
        }

        private void readLexeme(int firstByte)
        {
            _bufferedStream.SetLength(0);
            int b = firstByte;
            for (; ; )
            {
                _bufferedStream.WriteByte((byte)b);
                b = _stream.ReadByte();
                if (IsEOL(b) || IsSpecialCharacter(b) || b == -1)
                    break;
            }
            _lastParsedByte = b;
        }

        public static bool IsEOL(int ch)
        {
            if (ch == 0 || ch == 9 || ch == 10 || ch == 12 || ch == 13 || ch == 32)
                return true;
            return false;
        }

        public static bool IsSpecialCharacter(int ch)
        {
            if (ch == '/' || ch == '[' || ch == ']' || ch == '%' || ch == '}' ||
                ch == '(' || ch == ')' || ch == '<' || ch == '>' || ch == '{')
                return true;
            return false;
        }

        private static bool isOctal(int c)
        {
            return (c >= '0' && c <= '7');
        }

        private static bool isHex(int c)
        {
            if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                return true;
            return false;
        }

        private static int hexToInt(int val)
        {
            if (val >= '0' && val <= '9')
                val -= '0';
            else if (val >= 'a' && val <= 'f')
                val = val - 'a' + 10;
            else if (val >= 'A' && val <= 'F')
                val = val - 'A' + 10;
            return val;
        }

        private static int octToInt(int o1, int o2, int o3)
        {
            if (o2 == 0)
                return o1 - '0';
            if (o3 == 0)
                return (o1 - '0') * 8 + o2 - '0';
            return (o1 - '0') * 64 + (o2 - '0') * 8 + o3 - '0';
        }

        private static bool isNumberPart(int ch)
        {
            if ('0' <= ch && ch <= '9')
                return true;
            return false;
        }

        private static int findLast(byte[] buf, int bufLen, string s)
        {
            int sLen = s.Length;
            for (int i = bufLen - 1; i >= sLen - 1; --i)
            {
                if (buf[i] == s[sLen - 1] && buf[i - sLen + 1] == s[0])
                {
                    bool succes = true;
                    for (int j = i - sLen + 2, k = 1; k < sLen - 1; ++j, ++k)
                    {
                        if (buf[j] != s[k])
                        {
                            succes = false;
                            break;
                        }
                    }

                    if (succes)
                        return i - sLen + 1;
                }
            }

            return -1;
        }

        private static int findFirst(byte[] buf, int bufLen, string s)
        {
            int sLen = s.Length;
            if (s[0] == 256)
            {
                for (int i = 0; i <= bufLen - sLen; ++i)
                {
                    if ((IsEOL(buf[i]) && buf[i] != 0) && buf[i + sLen - 1] == s[sLen - 1])
                    {
                        bool succes = true;
                        for (int j = i + 1, k = 1; k < sLen - 1; ++j, ++k)
                        {
                            if (buf[j] != s[k])
                            {
                                succes = false;
                                break;
                            }
                        }

                        if (succes)
                            return i;
                    }
                }
            }
            else
            {
                for (int i = 0; i <= bufLen - sLen; ++i)
                {
                    if (buf[i] == s[0] && buf[i + sLen - 1] == s[sLen - 1])
                    {
                        bool succes = true;
                        for (int j = i + 1, k = 1; k < sLen - 1; ++j, ++k)
                        {
                            if (buf[j] != s[k])
                            {
                                succes = false;
                                break;
                            }
                        }

                        if (succes)
                            return i;
                    }
                }
            }
            return -1;
        }

        private static int toInt(byte[] buf, int bufLen, out bool succes)
        {
            if (bufLen == 0)
            {
                succes = false;
                return 0;
            }

            int result = 0;
            int tmp = 1;
            for (int i = bufLen - 1; i > 0; --i)
            {
                if (buf[i] >= '0' && buf[i] <= '9')
                {
                    result += (buf[i] - '0') * tmp;
                    tmp *= 10;
                }
                else
                {
                    succes = false;
                    return 0;
                }
            }

            if (buf[0] >= '0' && buf[0] <= '9')
            {
                result += (buf[0] - '0') * tmp;
            }
            else if (buf[0] == '-')
                result = -result;
            else
            {
                if (buf[0] != '+')
                {
                    succes = false;
                    return 0;
                }
            }

            succes = true;
            return result;
        }

        private static double toDouble(byte[] buf, int bufLen, out bool succes)
        {
            if (bufLen == 0)
            {
                succes = false;
                return 0;
            }

            int fractional = 0, integer = 0;
            int tmp1 = 1, tmp2 = 1;
            bool f = false;

            for (int i = bufLen - 1; i > 0; --i)
            {
                if (buf[i] >= '0' && buf[i] <= '9')
                {
                    fractional += (buf[i] - '0') * tmp1;
                    tmp1 *= 10;
                }
                else if (buf[i] == '.')
                {
                    for (int j = i - 1; j > 0; --j)
                    {
                        if (buf[j] >= '0' && buf[j] <= '9')
                        {
                            integer += (buf[j] - '0') * tmp2;
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
            if (buf[0] >= '0' && buf[0] <= '9')
            {
                if (f)
                    integer += (buf[0] - '0') * tmp2;
                else
                    fractional += (buf[0] - '0') * tmp1;

            }
            else if (buf[0] == '-')
            {
                sign = -1;
            }
            else if (buf[0] == '.')
            {
                f = true;
            }
            else
            {
                if (buf[0] != '+')
                {
                    succes = false;
                    return 0;
                }
            }

            succes = true;
            if (!f)
                return fractional * sign;

            return (integer + (double)fractional / tmp1) * sign;
        }
    }
}
