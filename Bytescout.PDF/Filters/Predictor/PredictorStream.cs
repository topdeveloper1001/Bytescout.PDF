using System;
using System.IO;

namespace Bytescout.PDF
{
    internal class PredictorStream : InputStream
    {
	    private const byte PREDICTOR_NONE = 1;
	    private const byte PREDICTOR_TIFF_2 = 2;
	    private const byte PREDICTOR_PNG_NONE = 10;
	    private const byte PREDICTOR_PNG_SUB = 11;
	    private const byte PREDICTOR_PNG_UP = 12;
	    private const byte PREDICTOR_PNG_AVG = 13;
	    private const byte PREDICTOR_PNG_PAETH = 14;
	    private const byte PREDICTOR_PNG_OPTIMUM = 15;

	    private readonly Stream _inputStream;
	    private byte[] _aboveBuffer;

	    private int _width;
	    private int _numComponents;
	    private int _bitsPerComponent;
	    private int _bpp; // From RFC 2083 (PNG), it's bytes per pixel, rounded up to 1
	    private int _predictor;

	    internal PredictorStream(Stream inputStream, PredictorParameters param)
        {
            _inputStream = inputStream;
            setPredictor(param.Predictor);
            setBitsPerComponent(param.BitsPerComponent);
            setColors(param.Colors);
            _width = param.Columns;
            _bpp = Math.Max(1, numBytesToHoldBits(_numComponents * _bitsPerComponent));

            int bufSize = numBytesToHoldBits(_width * _numComponents * _bitsPerComponent);
            _aboveBuffer = new byte[bufSize];
            _buffer = new byte[bufSize];
        }

        protected override int fillInternalBuffer()
        {
            byte[] temp = _aboveBuffer;
            _aboveBuffer = _buffer;
            _buffer = temp;

            if (_predictor == PREDICTOR_NONE)
            {
                int numRead = fillBufferFromInputStream();
                if (numRead <= 0)
                    return -1;
                return numRead;
            }
            else if (_predictor == PREDICTOR_TIFF_2)
            {
                int numRead = fillBufferFromInputStream();
                if (numRead <= 0)
                    return -1;
                if (_bitsPerComponent == 8)
                {
                    for (int i = 0; i < numRead; i++)
                    {
                        int prevIndex = i - _numComponents;
                        if (prevIndex >= 0)
                        {
                            _buffer[i] += _buffer[prevIndex];
                        }
                    }
                }
                return numRead;
            }
            else if (_predictor >= PREDICTOR_PNG_NONE && _predictor <= PREDICTOR_PNG_OPTIMUM)
            {
                int currPredictor = _predictor;
                int cp = _inputStream.ReadByte();

                if (cp < 0)
                    return -1;

                currPredictor = cp + PREDICTOR_PNG_NONE;
                int numRead = fillBufferFromInputStream();
                if (numRead <= 0)
                    return -1;

                for (int i = 0; i < numRead; i++)
                {
                    // For current row, PNG predictor to do nothing
                    if (currPredictor == PREDICTOR_PNG_NONE)
                        break;
                    // For current row, derive each byte from byte left-by-bpp
                    else if (currPredictor == PREDICTOR_PNG_SUB)
                    {
                        if ((i - _bpp) >= 0)
                            _buffer[i] += _buffer[(i - _bpp)];
                    }
                    // For current row, derive each byte from byte above
                    else if (currPredictor == PREDICTOR_PNG_UP)
                    {
                        _buffer[i] += _aboveBuffer[i];
                    }
                    // For current row, derive each byte from average of byte left-by-bpp and byte above
                    else if (currPredictor == PREDICTOR_PNG_AVG)
                    {
                        // PNG AVG: output(x) = curr_line(x) + floor((curr_line(x-bpp)+above(x))/2)
                        // From RFC 2083 (PNG), sum with no overflow, using >= 9 bit arithmatic
                        int left = 0;
                        if ((i - _bpp) >= 0)
                            left = (((int)_buffer[(i - _bpp)]) & 0xFF);

                        int above = 0;
                        if (_aboveBuffer != null)
                            above = (((int)_aboveBuffer[i]) & 0xFF);

                        int sum = left + above;
                        byte avg = (byte)((sum >> 1) & 0xFF);
                        _buffer[i] += avg;
                    }
                    // For current row, derive each byte from non-linear function of
                    // byte left-by-bpp and byte above and byte left-by-bpp of above
                    else if (currPredictor == PREDICTOR_PNG_PAETH)
                    {
                        // From RFC 2083 (PNG)
                        // PNG PAETH:  output(x) = curr_line(x) + PaethPredictor(curr_line(x-bpp), above(x), above(x-bpp))
                        // PaethPredictor(left, above, aboveLeft)
                        // p          = left + above - aboveLeft
                        // pLeft      = abs(p - left)
                        // pAbove     = abs(p - above)
                        // pAboveLeft = abs(p - aboveLeft)
                        // if( pLeft <= pAbove && pLeft <= pAboveLeft ) return left
                        // if( pAbove <= pAboveLeft ) return above
                        // return aboveLeft
                        int left = 0;
                        if ((i - _bpp) >= 0)
                            left = (((int)_buffer[(i - _bpp)]) & 0xFF);

                        int above = 0;
                        if (_aboveBuffer != null)
                            above = (((int)_aboveBuffer[i]) & 0xFF);

                        int aboveLeft = 0;
                        if ((i - _bpp) >= 0 && _aboveBuffer != null)
                            aboveLeft = (((int)_aboveBuffer[i - _bpp]) & 0xFF);

                        int p = left + above - aboveLeft;
                        int pLeft = Math.Abs(p - left);
                        int pAbove = Math.Abs(p - above);
                        int pAboveLeft = Math.Abs(p - aboveLeft);
                        int paeth = ((pLeft <= pAbove && pLeft <= pAboveLeft) ? left : ((pAbove <= pAboveLeft) ? above : aboveLeft));

                        _buffer[i] += ((byte)(paeth & 0xFF));
                    }
                }
                return numRead;
            }
            return -1;
        }

        private int fillBufferFromInputStream()
        {
            return _inputStream.Read(_buffer, 0, _buffer.Length);
        }

        private void setPredictor(int Predictor)
        {
            if (Predictor != PREDICTOR_NONE && Predictor != PREDICTOR_TIFF_2 && Predictor != PREDICTOR_PNG_NONE && Predictor != PREDICTOR_PNG_SUB &&
                Predictor != PREDICTOR_PNG_UP && Predictor != PREDICTOR_PNG_AVG && Predictor != PREDICTOR_PNG_PAETH && Predictor != PREDICTOR_PNG_OPTIMUM)
                _predictor = PREDICTOR_NONE;
            else
                _predictor = Predictor;
        }

        private void setBitsPerComponent(int BitsPerComponent)
        {
            if (BitsPerComponent != 1 || BitsPerComponent != 2 || BitsPerComponent != 4 || BitsPerComponent != 8 || BitsPerComponent != 16)
                _bitsPerComponent = BitsPerComponent;
            else
                _bitsPerComponent = 8;
        }

        private void setColors(int Colors)
        {
            if (Colors >= 1 && Colors <= 4)
                _numComponents = Colors;
            else
                _numComponents = 1;
        }

        private static int numBytesToHoldBits(int numBits)
        {
            int numBytes = (numBits / 8);
            if ((numBits % 8) > 0)
                numBytes++;
            return numBytes;
        }
    }
}
