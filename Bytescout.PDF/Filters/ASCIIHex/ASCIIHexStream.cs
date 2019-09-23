using System.IO;

namespace Bytescout.PDF
{
    internal class ASCIIHexStream : FilterStream
    {
	    private readonly Stream _inputStream;
	    private bool _eof;

	    public ASCIIHexStream(Stream inputStream)
        {
            _inputStream = inputStream;
            _eof = false;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_eof)
                return 0;

            int b;
            for (int i = 0; i < count; ++i)
            {
                b = ReadByte();
                if (b == -1)
                    return i;
                buffer[offset + i] = (byte)b;
            }
            return count;
        }

        public override int ReadByte()
        {
            if (_eof)
                return -1;
            int hi = readHexDigit();
            if (hi == -1)
                return -1;
            int lo = readHexDigit();

            if (lo == -1)
                return hi << 4;
            return (hi << 4) + lo;
        }

        private int readHexDigit()
        {
            int b = _inputStream.ReadByte();
            if (Lexer.IsEOL(b))
            {
                do
                {
                    b = _inputStream.ReadByte();
                } while (Lexer.IsEOL(b));
            }

            if (b == -1 || b == '>')
            {
                _eof = true;
                return -1;
            }
            else if (b >= '0' && b <= '9')
                return b - '0';
            else if (b >= 'a' && b <= 'f')
                return b - 'a' + 10;
            else if (b >= 'A' && b <= 'F')
                return b - 'A' + 10;

            _eof = true;
            return -1;
        }
    }
}
