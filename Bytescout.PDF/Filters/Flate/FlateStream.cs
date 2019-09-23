using System.IO;
using System.IO.Compression;

namespace Bytescout.PDF
{
    internal class FlateStream : FilterStream
    {
	    private readonly Stream _inputStream;
	    private bool _eof;

	    internal FlateStream(Stream inputStream, PredictorParameters param)
        {
            inputStream.ReadByte();
            inputStream.ReadByte();

            _inputStream = new DeflateStream(inputStream, CompressionMode.Decompress, true);
            if (param.Predictor != 1)
                _inputStream = new PredictorStream(_inputStream, param);

            _eof = false;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_eof)
                return 0;

            try
            {
                return _inputStream.Read(buffer, offset, count);
            }
            catch
            {
                _eof = true;
                return 0;
            }
        }

        public override int ReadByte()
        {
            if (_eof)
                return -1;

            try
            {
                return _inputStream.ReadByte();
            }
            catch
            {
                _eof = true;
                return -1;
            }
        }
    }
}
