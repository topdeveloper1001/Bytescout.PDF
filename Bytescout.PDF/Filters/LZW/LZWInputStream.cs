using System;
using System.IO;

namespace Bytescout.PDF
{
    internal class LZWInputStream : InputStream
    {
	    private readonly Stream _inputStream;
	    private int _earlyChange;
	    private byte[][] _table;
	    private int _tableIndex;
	    private int _bitsToGet;
	    private int _nextData;
	    private int _nextBits;
	    private int _oldCode;
	    private bool _eof;

	    internal LZWInputStream(Stream inputStream, int earlyChange)
        {
            _inputStream = inputStream;
            _earlyChange = earlyChange;

            _table = new byte[8192][];
            for (int i = 0; i < 256; i++)
            {
                _table[i] = new byte[1];
                _table[i][0] = (byte)i;
            }
            _tableIndex = 258;
            _bitsToGet = 9;
            _nextData = 0;
            _nextBits = 0;
            _oldCode = 0;
            _eof = false;
        }

        protected override int fillInternalBuffer()
        {
            if (_eof)
                return 0;

            int code = nextCode();
            if (code == -1 || code == 257)
            {
                _eof = true;
                return 0;
            }

            if (code == 256)
            {
                resetTable();
                code = nextCode();
                if (code == -1)
                {
                    _eof = true;
                    return 0;
                }

                if (code == 257)
                    return 0;
                _oldCode = code;
                if (_table[code] == null)
                {
                    _eof = true;
                    return 0;
                }

                return writeString(_table[code]);
            }
            else
            {
                if (code < _tableIndex)
                {
                    byte[] str = _table[code];
                    if (str == null)
                    {
                        _eof = true;
                        return 0;
                    }

                    addStringToTable(_table[_oldCode], str[0]);
                    _oldCode = code;
                    return writeString(str);
                }
                else
                {
                    byte[] str = _table[_oldCode];
                    if (str == null)
                    {
                        _eof = true;
                        return 0;
                    }

                    str = composeString(str, str[0]);
                    addStringToTable(str);
                    _oldCode = code;
                    return writeString(str);
                }
            }
        }

        private void resetTable()
        {
            _tableIndex = 258;
            _bitsToGet = 9;
            for (int i = 256; i < _table.Length; ++i)
                _table[i] = null;
        }

        private int writeString(byte[] str)
        {
            _buffer = str;
            return str.Length;
        }

        private void addStringToTable(byte[] oldstring, byte newstring)
        {
            byte[] str = composeString(oldstring, newstring);
            addStringToTable(str);
        }

        private void addStringToTable(byte[] str)
        {
            _table[_tableIndex++] = str;
            switch (_tableIndex + _earlyChange)
            {
                case 512:
                    _bitsToGet = 10;
                    break;
                case 1024:
                    _bitsToGet = 11;
                    break;
                case 2048:
                    _bitsToGet = 12;
                    break;
            }
        }

        private byte[] composeString(byte[] oldstring, byte newstring)
        {
            int length = oldstring.Length;
            byte[] str = new byte[length + 1];
            Array.Copy(oldstring, 0, str, 0, length);
            str[length] = newstring;
            return str;
        }

        private int nextCode()
        {
            int c, code;
            while (_nextBits < _bitsToGet)
            {
                if ((c = _inputStream.ReadByte()) == -1)
                    return -1;
                _nextData = (_nextData << 8) | (c & 0xff);
                _nextBits += 8;
            }
            code = (_nextData >> (_nextBits - _bitsToGet)) & ((1 << _bitsToGet) - 1);
            _nextBits -= _bitsToGet;
            return code;
        }
    }
}
