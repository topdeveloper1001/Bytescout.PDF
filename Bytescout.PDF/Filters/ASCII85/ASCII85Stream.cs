using System.IO;

namespace Bytescout.PDF
{
    internal class ASCII85Stream : InputStream
    {
	    private readonly Stream _inputStream;
	    private bool _eof;

	    internal ASCII85Stream(Stream inputStream)
        {
            _inputStream = inputStream;
            _eof = false;
            _buffer = new byte[4];
        }

        protected override int fillInternalBuffer()
        {
            return read5Symbols(_buffer);
        }

        private int read5Symbols(byte[] buf)
        {
            if (_eof)
                return 0;

            long value = 0;
            int count = 0, b;
            for (; ; )
            {
                b = _inputStream.ReadByte();
                if (b < 0)
                {
                    _eof = true;
                    break;
                }
                if (Lexer.IsEOL(b))
                    continue;

                if (b == '~')
                {
                    _eof = true;
                    break;
                }
                if (b == 'z')
                {
                    buf[0] = 0;
                    buf[1] = 0;
                    buf[2] = 0;
                    buf[3] = 0;
                    return 4;
                }
                if (!('!' <= b && b <= 'u'))
                {
                    _eof = true;
                    break;
                }

                count++;
                value = value * 85 + (b - 33);
                if (count == 5)
                {
                    buf[0] = (byte)((value >> 24) & 0xFF);
                    buf[1] = (byte)((value >> 16) & 0xFF);
                    buf[2] = (byte)((value >> 8) & 0xFF);
                    buf[3] = (byte)(value & 0xFF);
                    return 4;
                }
            }

            if (count == 2)
                value = value * (85L * 85 * 85) + 0xFFFFFF;
            else if (count == 3)
                value = value * (85L * 85) + 0xFFFF;
            else if (count == 4)
                value = value * (85L) + 0xFF;
            
            if (count >= 2)
                buf[0] = (byte)((value >> 24) & 0xFF);
            if (count >= 3)
                buf[1] = (byte)((value >> 16) & 0xFF);
            if (count >= 4)
                buf[2] = (byte)((value >> 8) & 0xFF);

            return count - 1;
        }
    }
}
