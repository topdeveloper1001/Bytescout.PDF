using System.IO;

namespace Bytescout.PDF
{
    internal class LZWStream : FilterStream
    {
	    private readonly Stream _inputStream;

	    internal LZWStream(Stream inputStream, PredictorParameters predictorParam, LZWParameters lzwParam)
        {
            _inputStream = new LZWInputStream(inputStream, lzwParam.EarlyChange);
            if (predictorParam.Predictor != 1)
                _inputStream = new PredictorStream(_inputStream, predictorParam);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inputStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return _inputStream.ReadByte();
        }
    }
}
