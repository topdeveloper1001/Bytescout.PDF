using System;

namespace Bytescout.PDF
{
    internal abstract class InputStream : FilterStream
    {
	    protected byte[] _buffer;
	    private int _bufPosition;
	    private int _bufLength;

	    protected InputStream()
	    {
		    _bufPosition = 0;
		    _bufLength = 0;
	    }

	    public override int Read(byte[] buffer, int offset, int count)
        {
            int numRead = 0;
            while (numRead < count)
            {
                if (needUpdate() && !updateBuffer())
                    return numRead;
                int toRead = _bufLength - _bufPosition;
                numRead += toRead;
                if (numRead <= count)
                {
                    Array.Copy(_buffer, _bufPosition, buffer, offset, toRead);
                    _bufPosition = _bufLength;
                    offset += toRead;
                }
                else
                {
                    int r = toRead - numRead + count;
                    Array.Copy(_buffer, _bufPosition, buffer, offset, r);
                    numRead = count;
                    _bufPosition += r;
                }
            }
            return numRead;
        }

        public override int ReadByte()
        {
            if (needUpdate() && !updateBuffer())
                return -1;
            return _buffer[_bufPosition++];
        }

        protected abstract int fillInternalBuffer();

        private bool updateBuffer()
        {
            if ((_bufLength = fillInternalBuffer()) <= 0)
            {
                _bufPosition = 0;
                _bufLength = 0;
                return false;
            }
            _bufPosition = 0;
            return true;
        }

        private bool needUpdate()
        {
            return _bufPosition == _bufLength;
        }
    }
}
