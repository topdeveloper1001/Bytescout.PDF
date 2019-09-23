using System;
using System.IO;

namespace Bytescout.PDF
{
    internal class Reader
    {
	    private readonly byte[] _buffer;
	    private readonly Lexeme _lexeme;
	    private int _position;

	    public int Position
	    {
		    get
		    {
			    return _position;
		    }
		    set
		    {
			    if (value < Length)
				    _position = value;
			    else
				    _position = Length - 1;
		    }
	    }

	    public int Length
	    {
		    get
		    {
			    return _buffer.Length;
		    }
	    }

	    public Lexeme Lexeme
	    {
		    get { return _lexeme; }
	    }

	    public Reader(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            _buffer = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(_buffer, 0, _buffer.Length);
            stream.Close();
            _lexeme = new Lexeme(_buffer);
        }

        public Reader(Stream stream)
        {
            _buffer = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(_buffer, 0, _buffer.Length);
            _lexeme = new Lexeme(_buffer);
        }

        public Reader(byte[] buffer)
        {
            _buffer = buffer;
            _lexeme = new Lexeme(_buffer);
        }

	    public byte[] GetBuffer()
        {
            return _buffer;
        }

        public int Read(byte[] buffer)
        {
            if (_position + buffer.Length > Length)
                return -1;

            Array.Copy(_buffer, _position, buffer, 0, buffer.Length);
            return _buffer.Length;
        }

        public int ReadByte()
        {
            if (_position > Length - 1 || Length == 0)
                return -1;

            ++_position;
            return _buffer[_position - 1];
        }

        public int ReadUint16()
        {
            int b1 = ReadByte();
            int b2 = ReadByte();
            if (b2 != -1)
                return b2 + 256 * b1;
            return 0;
        }

        public long ReadUint32()
        {
            int b1 = ReadByte();
            int b2 = ReadByte();
            int b3 = ReadByte();
            int b4 = ReadByte();

            if (b4 != -1)
                return b4 + 256 * b3 + 256 * 256 * b2 + 256 * 256 * 256 * b1;
            return 0;
        }

        public long ReadInt64()
        {
            byte[] bytes = new byte[8];
            for (int i = 0; i < bytes.Length; ++i)
            {
                int b = ReadByte();
                if (b == -1)
                    return 0;
                bytes[i] = (byte)b;
            }

            long res = 0;
            long m = 1;
            for (int i = bytes.Length - 1; i >= 0; --i)
            {
                res += bytes[i] * m;
                m *= 256;
            }

            return res;
        }

        public int FindSubstring(string str, int startIndex, int endIndex)
        {
            if (str.Length == 0 || Length < str.Length || endIndex < 0 || startIndex < 0 || startIndex + str.Length > endIndex)
                return -1;

            int len = str.Length;
            int end = (Length - len <= endIndex) ? endIndex : (Length - len);

            for (int i = startIndex; i < end; ++i)
            {
                if (_buffer[i] == str[0] && _buffer[i + len - 1] == str[len - 1])
                {
                    bool sucess = true;
                    for (long j = i + 1, k = 1; j < i + len - 2; ++j, ++k)
                    {
                        if (_buffer[j] != str[(int)k])
                        {
                            sucess = false;
                            break;
                        }
                    }

                    if (sucess)
                        return i;
                }
            }

            return -1;
        }

        public int FindNextSubstring(string str)
        {
            return FindSubstring(str, _position, Length);
        }

        public int FindLastSubstring(string str)
        {
            Position = Length - 1;
            return FindPreviewSubstring(str);
        }

        public int FindPreviewSubstring(string str)
        {
            if (str.Length == 0 || Length < str.Length)
                return -1;

            int len = str.Length;
            for (int i = _position; i > len; --i)
            {
                if (_buffer[i] == str[len - 1] && _buffer[i - len + 1] == str[0])
                {
                    bool sucess = true;
                    for (long j = i - len + 2, k = 1; j < i - 1; ++j, ++k)
                    {
                        if (_buffer[j] != str[(int)k])
                        {
                            sucess = false;
                            break;
                        }
                    }

                    if(sucess)
                        return i - len;
                }
            }

            return -1;
        }

        public void ReadLexeme()
        {
            SkipEOL();
            int start = _position;
            int count = 0;
            byte c;

            for (; ; )
            {
                ++count;
                ++_position;

                if (_position >= Length)
                {
                    if (_position > Length)
                    {
                        --count;
                        --_position;
                    }
                    break;
                }

                c = _buffer[_position];
                if (IsEOL(c) || specialCharacter(c))
                    break;
            }

            _lexeme.SetSize(start, count);
        }

        public void ReadLine()
        {
            byte c;
            for (; ; )
            {
                if (_position >= Length - 1)
                    break;

                c = _buffer[_position];
                ++_position;

                if (c == '\n')
                    break;
                else if (c == '\r')
                {
                    if (Peek() == '\n')
                        ++_position;
                    break;
                }
            }
        }

        public int Peek()
        {
            if (_position <= Length - 1)
                return _buffer[_position];
            return -1;
        }

        public void SkipEOL()
        {
            for (; ; )
            {
                if (_position >= Length - 1)
                    return;

                if (IsEOL(_buffer[_position]))
                    _position++;
                else
                    return;
            }
        }

        public static bool IsEOL(int ch)
        {
            if (ch == 0 || ch == 9 || ch == 10 || ch == 12 || ch == 13 || ch == 32)
                return true;
            return false;
        }

        private static bool specialCharacter(int ch)
        {
            if (ch == '/' || ch == '[' || ch == ']' || ch == '%' || ch == '}' ||
                ch == '(' || ch == ')' || ch == '<' || ch == '>' || ch == '{')
                return true;
            return false;
        }
    }
}
