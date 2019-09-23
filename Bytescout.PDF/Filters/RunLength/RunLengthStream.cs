using System.IO;

namespace Bytescout.PDF
{
    internal class RunLengthStream : FilterStream
    {
	    private readonly Stream _inputStream;
	    private bool _eof;
	    private int _bytesCount;
	    private byte _curByte;
	    private bool _useCurByte;

	    internal RunLengthStream(Stream inputStream)
        {
            _inputStream = inputStream;
            _eof = false;
            _bytesCount = 0;
            _curByte = 0;
            _useCurByte = false;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_eof)
                return 0;

            int numRead = 0;
            while (numRead < count)
            {
                if (needUpdate() && !readRun())
                    return numRead;
                numRead += _bytesCount;
                if (numRead <= count)
                {
                    if (_useCurByte)
                    {
                        for (int i = 0; i < _bytesCount; ++i)
                            buffer[offset + i] = _curByte;
                    }
                    else
                    {
                        int tmp = _inputStream.Read(buffer, offset, _bytesCount);
                        if (tmp != _bytesCount)
                        {
                            numRead = numRead - _bytesCount + tmp;
                            _eof = true;
                            return numRead;
                        }
                    }
                    _bytesCount = 0;
                    offset += _bytesCount;
                }
                else
                {
                    int r = _bytesCount - numRead + count;
                    if (_useCurByte)
                    {
                        for (int i = 0; i < r; ++i)
                            buffer[offset + i] = _curByte;
                    }
                    else
                    {
                        int tmp = _inputStream.Read(buffer, offset, r);
                        if (tmp != r)
                        {
                            _eof = true;
                            return count - (tmp - r);
                        }
                    }
                    numRead = count;
                    _bytesCount -= r;
                }
            }
            return numRead;
        }

        public override int ReadByte()
        {
            if (_eof)
                return -1;
            if (needUpdate() && !readRun())
                return -1;
            
            _bytesCount--;
            if (_useCurByte)
                return _curByte;
            return _inputStream.ReadByte();
        }

        private bool needUpdate()
        {
            return _bytesCount == 0;
        }

        private bool readRun()
        {
            int b = _inputStream.ReadByte();
            if (b == -1 || b == 128)
            {
                _eof = true;
                return false;
            }
            if (b < 128)
            {
                _bytesCount = b + 1;
                _useCurByte = false;
                return true;
            }
            else
            {
                _bytesCount = 0x101 - b;
                int tmp = _inputStream.ReadByte();
                if (tmp == -1)
                {
                    _bytesCount = 0;
                    return false;
                }
                _useCurByte = true;
                _curByte = (byte)tmp;
                return true;
            }
        }
    }
}
