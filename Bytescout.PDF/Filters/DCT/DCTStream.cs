using System.IO;

namespace Bytescout.PDF
{
#if !EXCLUDE_UNSAFE
	internal unsafe class DCTStream : FilterStream
    {
	    private struct DCTCompInfo
	    {
		    public int ID;
		    public int HSample, VSample;
		    public int QuantTable;
		    public int PrevDC;
	    }

	    private class DCTScanInfo
	    {
		    public bool[] Comp = new bool[4];
		    public int NumComps;
		    public int[] DCHuffTable = new int[4];
		    public int[] ACHuffTable = new int[4];
		    public int FirstCoeff, LastCoeff;
		    public int Ah, Al;
	    }

	    private class DCTHuffTable
	    {
		    public byte[] FirstSym = new byte[17];
		    public short[] FirstCode = new short[17];
		    public short[] NumCodes = new short[17];
		    public byte[] Sym = new byte[256];
	    }

	    private static readonly byte[] DCTZigZag =
	    {
		    0,
		    1,  8,
		    16,  9,  2,
		    3, 10, 17, 24,
		    32, 25, 18, 11, 4,
		    5, 12, 19, 26, 33, 40,
		    48, 41, 34, 27, 20, 13,  6,
		    7, 14, 21, 28, 35, 42, 49, 56,
		    57, 50, 43, 36, 29, 22, 15,
		    23, 30, 37, 44, 51, 58,
		    59, 52, 45, 38, 31,
		    39, 46, 53, 60,
		    61, 54, 47,
		    55, 62,
		    63
	    };

	    private const int DCTCos1 = 4017;
	    private const int DCTSin1 = 799;
	    private const int DCTCos3 = 3406;
	    private const int DCTSin3 = 2276;
	    private const int DCTCos6 = 1567;
	    private const int DCTSin6 = 3784;
	    private const int DCTSqrt2 = 5793;
	    private const int DCTSqrt1D2 = 2896;
	    private const int DCTCrToR = 91881;
	    private const int DCTCbToG = -22553;
	    private const int DCTCrToG = -46802;
	    private const int DCTCbToB = 116130;
	    private const int DCTClipOffset = 256;
	    private static readonly byte[] DCTClip;
	    private readonly Stream _inputStream;

	    private bool _progressive;
	    private bool _interleaved;
	    private int _width, _height;
	    private int _mcuWidth, _mcuHeight;
	    private int _bufWidth, _bufHeight;
	    private DCTCompInfo[] _compInfo;
	    private DCTScanInfo _scanInfo;
	    private int _numComps;
	    private int _colorXform;

	    private bool _gotJFIFMarker;
	    private int _restartInterval;
	    private short[,] _quantTables;
	    private int _numQuantTables;
	    private DCTHuffTable[] _dcHuffTables;
	    private DCTHuffTable[] _acHuffTables;
	    private int _numDCHuffTables;
	    private int _numACHuffTables;
	    private byte[,][] _rowBuf;
	    private int[][] _frameBuf;
	    private int _comp, _x, _y, _dy;
	    private int _restartCtr;
	    private int _restartMarker;
	    private int _eobRun;
	    private int _inputBuf;
	    private int _inputBits;

	    internal DCTStream(Stream inputStream, DCTParameters param)
        {
            _inputStream = inputStream;

            _colorXform = param.ColorTransform;
            _mcuWidth = _mcuHeight = 0;
            _numComps = 0;
            _comp = 0;
            _x = _y = _dy = 0;

            _scanInfo = new DCTScanInfo();
            _compInfo = new DCTCompInfo[4];
            _quantTables = new short[4, 64];
            _dcHuffTables = new DCTHuffTable[4];
            _acHuffTables = new DCTHuffTable[4];
            _rowBuf = new byte[4, 32][];
            _frameBuf = new int[4][];

            for (int i = 0; i < 4; ++i)
            {
                _dcHuffTables[i] = new DCTHuffTable();
                _acHuffTables[i] = new DCTHuffTable();
            }

            reset();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int b;
            for (int i = 0; i < count; ++i)
            {
                b = ReadByte();
                if (b == -1)
                    return i;
                buffer[i + offset] = (byte)b;
            }
            return count;
        }

        public override int ReadByte()
        {
            return getByte();
        }

        private int getByte()
        {
            int c;
            if (_y >= _height)
                return -1;

            if (_progressive || !_interleaved)
            {
                c = _frameBuf[_comp][_y * _bufWidth + _x];
                if (++_comp == _numComps)
                {
                    _comp = 0;
                    if (++_x == _width)
                    {
                        _x = 0;
                        ++_y;
                    }
                }
            }
            else
            {
                if (_dy >= _mcuHeight)
                {
                    if (!readMCURow())
                    {
                        _y = _height;
                        return -1;
                    }

                    _comp = 0;
                    _x = 0;
                    _dy = 0;
                }

                c = _rowBuf[_comp, _dy][_x];
                if (++_comp == _numComps)
                {
                    _comp = 0;
                    if (++_x == _width)
                    {
                        _x = 0;
                        ++_y;
                        ++_dy;

                        if (_y == _height)
                            readTrailer();
                    }
                }
            }
            return c;
        }

        private bool readMCURow()
        {
            int* data1 = stackalloc int[64];
            byte* data2 = stackalloc byte[64];
            int h, v, horiz, vert, hSub, vSub, c;

            for (int x1 = 0; x1 < _width; x1 += _mcuWidth)
            {
                if (_restartInterval > 0 && _restartCtr == 0)
                {
                    c = readMarker();
                    if (c != _restartMarker)
                        return false;
                    if (++_restartMarker == 0xd8)
                        _restartMarker = 0xd0;
                    restart();
                }

                for (int cc = 0; cc < _numComps; ++cc)
                {
                    h = _compInfo[cc].HSample;
                    v = _compInfo[cc].VSample;
                    horiz = _mcuWidth / h;
                    vert = _mcuHeight / v;
                    hSub = horiz / 8;
                    vSub = vert / 8;

                    for (int y2 = 0; y2 < _mcuHeight; y2 += vert)
                    {
                        for (int x2 = 0; x2 < _mcuWidth; x2 += horiz)
                        {
                            fixed (int* dc = &_compInfo[cc].PrevDC)
                                if (!readDataUnit(_dcHuffTables[_scanInfo.DCHuffTable[cc]], _acHuffTables[_scanInfo.ACHuffTable[cc]], dc, data1))
                                    return false;

                            fixed (short* quantTable = &_quantTables[_compInfo[cc].QuantTable, 0])
                                transformDataUnit(quantTable, data1, data2);

                            if (hSub == 1 && vSub == 1)
                            {
                                for (int y3 = 0, i = 0; y3 < 8; ++y3, i += 8)
                                {
                                    fixed (byte* p1 = &_rowBuf[cc, y2 + y3][x1 + x2])
                                    {
                                        p1[0] = data2[i];
                                        p1[1] = data2[i + 1];
                                        p1[2] = data2[i + 2];
                                        p1[3] = data2[i + 3];
                                        p1[4] = data2[i + 4];
                                        p1[5] = data2[i + 5];
                                        p1[6] = data2[i + 6];
                                        p1[7] = data2[i + 7];
                                    }
                                }
                            }
                            else if (hSub == 2 && vSub == 2)
                            {
                                for (int y3 = 0, i = 0; y3 < 16; y3 += 2, i += 8)
                                {
                                    fixed (byte* p1 = &_rowBuf[cc, y2 + y3][x1 + x2], p2 = &_rowBuf[cc, y2 + y3 + 1][x1 + x2])
                                    {
                                        p1[0] = p1[1] = p2[0] = p2[1] = data2[i];
                                        p1[2] = p1[3] = p2[2] = p2[3] = data2[i + 1];
                                        p1[4] = p1[5] = p2[4] = p2[5] = data2[i + 2];
                                        p1[6] = p1[7] = p2[6] = p2[7] = data2[i + 3];
                                        p1[8] = p1[9] = p2[8] = p2[9] = data2[i + 4];
                                        p1[10] = p1[11] = p2[10] = p2[11] = data2[i + 5];
                                        p1[12] = p1[13] = p2[12] = p2[13] = data2[i + 6];
                                        p1[14] = p1[15] = p2[14] = p2[15] = data2[i + 7];
                                    }
                                }
                            }
                            else
                            {
                                int i = 0;
                                for (int y3 = 0, y4 = 0; y3 < 8; ++y3, y4 += vSub)
                                {
                                    for (int x3 = 0, x4 = 0; x3 < 8; ++x3, x4 += hSub)
                                    {
                                        for (int y5 = 0; y5 < vSub; ++y5)
                                            for (int x5 = 0; x5 < hSub; ++x5)
                                                _rowBuf[cc, y2 + y4 + y5][x1 + x2 + x4 + x5] = data2[i];
                                        ++i;
                                    }
                                }
                            }
                        }
                    }
                }
                --_restartCtr;

                if (_colorXform != 0)
                {
                    int pY, pCb, pCr, pR, pG, pB;
                    if (_numComps == 3)
                    {
                        for (int y2 = 0; y2 < _mcuHeight; ++y2)
                        {
                            for (int x2 = 0; x2 < _mcuWidth; ++x2)
                            {
                                pY = _rowBuf[0, y2][x1 + x2];
                                pCb = _rowBuf[1, y2][x1 + x2] - 128;
                                pCr = _rowBuf[2, y2][x1 + x2] - 128;
                                pR = ((pY << 16) + DCTCrToR * pCr + 32768) >> 16;
                                _rowBuf[0, y2][x1 + x2] = DCTClip[DCTClipOffset + pR];
                                pG = ((pY << 16) + DCTCbToG * pCb + DCTCrToG * pCr + 32768) >> 16;
                                _rowBuf[1, y2][x1 + x2] = DCTClip[DCTClipOffset + pG];
                                pB = ((pY << 16) + DCTCbToB * pCb + 32768) >> 16;
                                _rowBuf[2, y2][x1 + x2] = DCTClip[DCTClipOffset + pB];
                            }
                        }
                    }
                    else if (_numComps == 4)
                    {
                        for (int y2 = 0; y2 < _mcuHeight; ++y2)
                        {
                            for (int x2 = 0; x2 < _mcuWidth; ++x2)
                            {
                                pY = _rowBuf[0, y2][x1 + x2];
                                pCb = _rowBuf[1, y2][x1 + x2] - 128;
                                pCr = _rowBuf[2, y2][x1 + x2] - 128;
                                pR = ((pY << 16) + DCTCrToR * pCr + 32768) >> 16;
                                _rowBuf[0, y2][x1 + x2] = (byte)(255 - DCTClip[DCTClipOffset + pR]);
                                pG = ((pY << 16) + DCTCbToG * pCb + DCTCrToG * pCr + 32768) >> 16;
                                _rowBuf[1, y2][x1 + x2] = (byte)(255 - DCTClip[DCTClipOffset + pG]);
                                pB = ((pY << 16) + DCTCbToB * pCb + 32768) >> 16;
                                _rowBuf[2, y2][x1 + x2] = (byte)(255 - DCTClip[DCTClipOffset + pB]);
                            }
                        }
                    }
                }
            }
            return true;
        }

        private void reset()
        {
            _progressive = _interleaved = false;
            _width = _height = 0;
            _numComps = 0;
            _numQuantTables = 0;
            _numDCHuffTables = 0;
            _numACHuffTables = 0;
            _gotJFIFMarker = false;
            _restartInterval = 0;

            if (!readHeader())
            {
                _y = _height;
                return;
            }

            if (_numComps == 1)
                _compInfo[0].HSample = _compInfo[0].VSample = 1;

            _mcuWidth = _compInfo[0].HSample;
            _mcuHeight = _compInfo[0].VSample;

            for (int i = 1; i < _numComps; ++i)
            {
                if (_compInfo[i].HSample > _mcuWidth)
                    _mcuWidth = _compInfo[i].HSample;
                if (_compInfo[i].VSample > _mcuHeight)
                    _mcuHeight = _compInfo[i].VSample;
            }

            _mcuWidth *= 8;
            _mcuHeight *= 8;

            if (_colorXform == -1)
            {
                if (_numComps == 3)
                {
                    if (_gotJFIFMarker)
                        _colorXform = 1;
                    else if (_compInfo[0].ID == 82 && _compInfo[1].ID == 71 && _compInfo[2].ID == 66)
                        _colorXform = 0;
                    else
                        _colorXform = 1;
                }
                else
                {
                    _colorXform = 0;
                }
            }

            if (_progressive || !_interleaved)
            {
                _bufWidth = ((_width + _mcuWidth - 1) / _mcuWidth) * _mcuWidth;
                _bufHeight = ((_height + _mcuHeight - 1) / _mcuHeight) * _mcuHeight;

                if (_bufWidth <= 0 || _bufHeight <= 0 || _bufWidth > int.MaxValue / _bufWidth / (int)sizeof(int))
                {
                    _y = _height;
                    return;
                }

                for (int i = 0; i < _numComps; ++i)
                    _frameBuf[i] = new int[_bufWidth * _bufHeight];

                do
                {
                    _restartMarker = 0xd0;
                    restart();
                    readScan();
                } while (readHeader());

                decodeImage();
                _comp = 0;
                _x = 0;
                _y = 0;
            }
            else
            {
                _bufWidth = ((_width + _mcuWidth - 1) / _mcuWidth) * _mcuWidth;
                for (int i = 0; i < _numComps; ++i)
                    for (int j = 0; j < _mcuHeight; ++j)
                        _rowBuf[i, j] = new byte[_bufWidth];

                _comp = 0;
                _x = 0;
                _y = 0;
                _dy = _mcuHeight;
                _restartMarker = 0xd0;
                restart();
            }
        }

        private void readScan()
        {
            int* data = stackalloc int[64];
            int h, v, horiz, vert, vSub, dx1, dy1;
            int c, cc;

            if (_scanInfo.NumComps == 1)
            {
                for (cc = 0; cc < _numComps; ++cc)
                {
                    if (_scanInfo.Comp[cc])
                        break;
                }

                dx1 = _mcuWidth / _compInfo[cc].HSample;
                dy1 = _mcuHeight / _compInfo[cc].VSample;
            }
            else
            {
                dx1 = _mcuWidth;
                dy1 = _mcuHeight;
            }

            for (int y1 = 0; y1 < _height; y1 += dy1)
            {
                for (int x1 = 0; x1 < _width; x1 += dx1)
                {
                    if (_restartInterval > 0 && _restartCtr == 0)
                    {
                        c = readMarker();
                        if (c != _restartMarker)
                            return;
                        if (++_restartMarker == 0xd8)
                            _restartMarker = 0xd0;
                        restart();
                    }

                    for (cc = 0; cc < _numComps; ++cc)
                    {
                        if (!_scanInfo.Comp[cc])
                            continue;

                        h = _compInfo[cc].HSample;
                        v = _compInfo[cc].VSample;
                        horiz = _mcuWidth / h;
                        vert = _mcuHeight / v;
                        vSub = vert / 8;

                        int* p1;
                        for (int y2 = 0; y2 < dy1; y2 += vert)
                        {
                            for (int x2 = 0; x2 < dx1; x2 += horiz)
                            {
                                fixed (int* pp = &_frameBuf[cc][(y1 + y2) * _bufWidth + (x1 + x2)])
                                {
                                    p1 = pp;
                                    for (int y3 = 0, i = 0; y3 < 8; ++y3, i += 8)
                                    {
                                        data[i] = p1[0];
                                        data[i + 1] = p1[1];
                                        data[i + 2] = p1[2];
                                        data[i + 3] = p1[3];
                                        data[i + 4] = p1[4];
                                        data[i + 5] = p1[5];
                                        data[i + 6] = p1[6];
                                        data[i + 7] = p1[7];
                                        p1 += _bufWidth * vSub;
                                    }

                                    // read one data unit
                                    if (_progressive)
                                    {
                                        fixed (int* dc = &_compInfo[cc].PrevDC)
                                            if (!readProgressiveDataUnit(_dcHuffTables[_scanInfo.DCHuffTable[cc]], _acHuffTables[_scanInfo.ACHuffTable[cc]], dc, data))
                                                return;
                                    }
                                    else
                                    {
                                        fixed (int* dc = &_compInfo[cc].PrevDC)
                                            if (!readDataUnit(_dcHuffTables[_scanInfo.DCHuffTable[cc]], _acHuffTables[_scanInfo.ACHuffTable[cc]], dc, data))
                                                return;
                                    }
                                }

                                // add the data unit into _frameBuf
                                fixed (int* pp = &_frameBuf[cc][(y1 + y2) * _bufWidth + (x1 + x2)])
                                {
                                    p1 = pp;
                                    for (int y3 = 0, i = 0; y3 < 8; ++y3, i += 8)
                                    {
                                        p1[0] = data[i];
                                        p1[1] = data[i + 1];
                                        p1[2] = data[i + 2];
                                        p1[3] = data[i + 3];
                                        p1[4] = data[i + 4];
                                        p1[5] = data[i + 5];
                                        p1[6] = data[i + 6];
                                        p1[7] = data[i + 7];
                                        p1 += _bufWidth * vSub;
                                    }
                                }
                            }
                        }
                    }
                    --_restartCtr;
                }
            }
        }

        private bool readDataUnit(DCTHuffTable dcHuffTable, DCTHuffTable acHuffTable, int* prevDC, int* data)
        {
            int run, size, amp, i, j, c;
            if ((size = readHuffSym(dcHuffTable)) == 9999)
                return false;

            if (size > 0)
            {
                if ((amp = readAmp(size)) == 9999)
                    return false;
            }
            else
            {
                amp = 0;
            }

            data[0] = *prevDC += amp;
            for (i = 1; i < 64; ++i)
                data[i] = 0;

            i = 1;
            while (i < 64)
            {
                run = 0;
                while ((c = readHuffSym(acHuffTable)) == 0xf0 && run < 0x30)
                    run += 0x10;
                if (c == 9999)
                    return false;

                if (c == 0x00)
                {
                    break;
                }
                else
                {
                    run += (c >> 4) & 0x0f;
                    size = c & 0x0f;
                    amp = readAmp(size);

                    if (amp == 9999)
                        return false;
                    i += run;

                    if (i < 64)
                    {
                        j = DCTZigZag[i++];
                        data[j] = amp;
                    }
                }
            }
            return true;
        }

        private bool readProgressiveDataUnit(DCTHuffTable dcHuffTable, DCTHuffTable acHuffTable, int* prevDC, int* data)
        {
            int run, size, amp, bit, c, i, j, k;
            i = _scanInfo.FirstCoeff;

            if (i == 0)
            {
                if (_scanInfo.Ah == 0)
                {
                    if ((size = readHuffSym(dcHuffTable)) == 9999)
                        return false;

                    if (size > 0)
                    {
                        if ((amp = readAmp(size)) == 9999)
                            return false;
                    }
                    else
                    {
                        amp = 0;
                    }

                    data[0] += (*prevDC += amp) << _scanInfo.Al;
                }
                else
                {
                    if ((bit = readBit()) == 9999)
                        return false;
                    data[0] += bit << _scanInfo.Al;
                }
                ++i;
            }

            if (_scanInfo.LastCoeff == 0)
                return true;

            // check for an EOB run
            if (_eobRun > 0)
            {
                while (i <= _scanInfo.LastCoeff)
                {
                    j = DCTZigZag[i++];
                    if (data[j] != 0)
                    {
                        if ((bit = readBit()) == -1)
                            return false;
                        if (bit != 0)
                            data[j] += 1 << _scanInfo.Al;
                    }
                }
                --_eobRun;
                return true;
            }

            // read the AC coefficients
            while (i <= _scanInfo.LastCoeff)
            {
                if ((c = readHuffSym(acHuffTable)) == 9999)
                    return false;

                if (c == 0xf0)
                {
                    k = 0;
                    while (k < 16 && i <= _scanInfo.LastCoeff)
                    {
                        j = DCTZigZag[i++];
                        if (data[j] == 0)
                            ++k;
                        else
                        {
                            if ((bit = readBit()) == -1)
                                return false;
                            if (bit != 0)
                                data[j] += 1 << _scanInfo.Al;
                        }
                    }
                }
                else if ((c & 0x0f) == 0x00)
                {
                    j = c >> 4;
                    _eobRun = 0;
                    for (k = 0; k < j; ++k)
                    {
                        if ((bit = readBit()) == -1)
                            return false;
                        _eobRun = (_eobRun << 1) | bit;
                    }

                    _eobRun += 1 << j;
                    while (i <= _scanInfo.LastCoeff)
                    {
                        j = DCTZigZag[i++];
                        if (data[j] != 0)
                        {
                            if ((bit = readBit()) == -1)
                                return false;
                            if (bit != 0)
                                data[j] += 1 << _scanInfo.Al;
                        }
                    }
                    --_eobRun;
                    break;
                }
                else
                {
                    run = (c >> 4) & 0x0f;
                    size = c & 0x0f;
                    if ((amp = readAmp(size)) == 9999)
                        return false;

                    j = 0;
                    for (k = 0; k <= run && i <= _scanInfo.LastCoeff; ++k)
                    {
                        j = DCTZigZag[i++];
                        while (data[j] != 0 && i <= _scanInfo.LastCoeff)
                        {
                            if ((bit = readBit()) == -1)
                                return false;
                            if (bit != 0)
                                data[j] += 1 << _scanInfo.Al;
                            j = DCTZigZag[i++];
                        }
                    }
                    data[j] = amp << _scanInfo.Al;
                }
            }

            return true;
        }

        private void decodeImage()
        {
            int* dataIn = stackalloc int[64];
            byte* dataOut = stackalloc byte[64];
            int pY, pCb, pCr, pR, pG, pB;
            int x1, y1, x2, y2, x3, y3, x4, y4, x5, y5, cc, i;
            int h, v, horiz, vert, hSub, vSub;
            int* p0, p1, p2;

            for (y1 = 0; y1 < _bufHeight; y1 += _mcuHeight)
            {
                for (x1 = 0; x1 < _bufWidth; x1 += _mcuWidth)
                {
                    for (cc = 0; cc < _numComps; ++cc)
                    {
                        fixed (short* quantTable = &_quantTables[_compInfo[cc].QuantTable, 0])
                        {
                            h = _compInfo[cc].HSample;
                            v = _compInfo[cc].VSample;
                            horiz = _mcuWidth / h;
                            vert = _mcuHeight / v;
                            hSub = horiz / 8;
                            vSub = vert / 8;

                            for (y2 = 0; y2 < _mcuHeight; y2 += vert)
                            {
                                for (x2 = 0; x2 < _mcuWidth; x2 += horiz)
                                {
                                    fixed (int* p = &_frameBuf[cc][(y1 + y2) * _bufWidth + (x1 + x2)])
                                    {
                                        p1 = p;
                                        for (y3 = 0, i = 0; y3 < 8; ++y3, i += 8)
                                        {
                                            dataIn[i] = p1[0];
                                            dataIn[i + 1] = p1[1];
                                            dataIn[i + 2] = p1[2];
                                            dataIn[i + 3] = p1[3];
                                            dataIn[i + 4] = p1[4];
                                            dataIn[i + 5] = p1[5];
                                            dataIn[i + 6] = p1[6];
                                            dataIn[i + 7] = p1[7];
                                            p1 += _bufWidth * vSub;
                                        }
                                    }

                                    transformDataUnit(quantTable, dataIn, dataOut);
                                    fixed (int* p = &_frameBuf[cc][(y1 + y2) * _bufWidth + (x1 + x2)])
                                    {
                                        p1 = p;
                                        if (hSub == 1 && vSub == 1)
                                        {
                                            for (y3 = 0, i = 0; y3 < 8; ++y3, i += 8)
                                            {
                                                p1[0] = dataOut[i] & 0xff;
                                                p1[1] = dataOut[i + 1] & 0xff;
                                                p1[2] = dataOut[i + 2] & 0xff;
                                                p1[3] = dataOut[i + 3] & 0xff;
                                                p1[4] = dataOut[i + 4] & 0xff;
                                                p1[5] = dataOut[i + 5] & 0xff;
                                                p1[6] = dataOut[i + 6] & 0xff;
                                                p1[7] = dataOut[i + 7] & 0xff;
                                                p1 += _bufWidth;
                                            }
                                        }
                                        else if (hSub == 2 && vSub == 2)
                                        {
                                            p2 = p1 + _bufWidth;
                                            for (y3 = 0, i = 0; y3 < 16; y3 += 2, i += 8)
                                            {
                                                p1[0] = p1[1] = p2[0] = p2[1] = dataOut[i] & 0xff;
                                                p1[2] = p1[3] = p2[2] = p2[3] = dataOut[i + 1] & 0xff;
                                                p1[4] = p1[5] = p2[4] = p2[5] = dataOut[i + 2] & 0xff;
                                                p1[6] = p1[7] = p2[6] = p2[7] = dataOut[i + 3] & 0xff;
                                                p1[8] = p1[9] = p2[8] = p2[9] = dataOut[i + 4] & 0xff;
                                                p1[10] = p1[11] = p2[10] = p2[11] = dataOut[i + 5] & 0xff;
                                                p1[12] = p1[13] = p2[12] = p2[13] = dataOut[i + 6] & 0xff;
                                                p1[14] = p1[15] = p2[14] = p2[15] = dataOut[i + 7] & 0xff;
                                                p1 += _bufWidth * 2;
                                                p2 += _bufWidth * 2;
                                            }
                                        }
                                        else
                                        {
                                            i = 0;
                                            for (y3 = 0, y4 = 0; y3 < 8; ++y3, y4 += vSub)
                                            {
                                                for (x3 = 0, x4 = 0; x3 < 8; ++x3, x4 += hSub)
                                                {
                                                    p2 = p1 + x4;
                                                    for (y5 = 0; y5 < vSub; ++y5)
                                                    {
                                                        for (x5 = 0; x5 < hSub; ++x5)
                                                            p2[x5] = dataOut[i] & 0xff;
                                                        p2 += _bufWidth;
                                                    }
                                                    ++i;
                                                }
                                                p1 += _bufWidth * vSub;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (_colorXform != 0)
                    {
                        if (_numComps == 3)
                        {
                            for (y2 = 0; y2 < _mcuHeight; ++y2)
                            {
                                fixed (int* t1 = &_frameBuf[0][(y1 + y2) * _bufWidth + x1], t2 = &_frameBuf[1][(y1 + y2) * _bufWidth + x1], t3 = &_frameBuf[2][(y1 + y2) * _bufWidth + x1])
                                {
                                    p0 = t1;
                                    p1 = t2;
                                    p2 = t3;
                                    for (x2 = 0; x2 < _mcuWidth; ++x2)
                                    {
                                        pY = *p0;
                                        pCb = *p1 - 128;
                                        pCr = *p2 - 128;
                                        pR = ((pY << 16) + DCTCrToR * pCr + 32768) >> 16;
                                        *p0++ = DCTClip[DCTClipOffset + pR];
                                        pG = ((pY << 16) + DCTCbToG * pCb + DCTCrToG * pCr +
                                          32768) >> 16;
                                        *p1++ = DCTClip[DCTClipOffset + pG];
                                        pB = ((pY << 16) + DCTCbToB * pCb + 32768) >> 16;
                                        *p2++ = DCTClip[DCTClipOffset + pB];
                                    }
                                }
                            }
                        }
                        else if (_numComps == 4)
                        {
                            for (y2 = 0; y2 < _mcuHeight; ++y2)
                            {
                                fixed (int* t1 = &_frameBuf[0][(y1 + y2) * _bufWidth + x1], t2 = &_frameBuf[1][(y1 + y2) * _bufWidth + x1], t3 = &_frameBuf[2][(y1 + y2) * _bufWidth + x1])
                                {
                                    p0 = t1;
                                    p1 = t2;
                                    p2 = t3;
                                    for (x2 = 0; x2 < _mcuWidth; ++x2)
                                    {
                                        pY = *p0;
                                        pCb = *p1 - 128;
                                        pCr = *p2 - 128;
                                        pR = ((pY << 16) + DCTCrToR * pCr + 32768) >> 16;
                                        *p0++ = 255 - DCTClip[DCTClipOffset + pR];
                                        pG = ((pY << 16) + DCTCbToG * pCb + DCTCrToG * pCr +
                                          32768) >> 16;
                                        *p1++ = 255 - DCTClip[DCTClipOffset + pG];
                                        pB = ((pY << 16) + DCTCbToB * pCb + 32768) >> 16;
                                        *p2++ = 255 - DCTClip[DCTClipOffset + pB];
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void transformDataUnit(short* quantTable, int* dataIn, byte* dataOut)
        {
            int v0, v1, v2, v3, v4, v5, v6, v7, t, i;
            int* p;

            for (i = 0; i < 64; ++i)
                dataIn[i] *= quantTable[i];

            // inverse DCT on rows
            for (i = 0; i < 64; i += 8)
            {
                p = dataIn + i;
                if (p[1] == 0 && p[2] == 0 && p[3] == 0 && p[4] == 0 && p[5] == 0 && p[6] == 0 && p[7] == 0)
                {
                    t = (DCTSqrt2 * p[0] + 512) >> 10;
                    p[0] = t;
                    p[1] = t;
                    p[2] = t;
                    p[3] = t;
                    p[4] = t;
                    p[5] = t;
                    p[6] = t;
                    p[7] = t;
                    continue;
                }

                // stage 4
                v0 = (DCTSqrt2 * p[0] + 128) >> 8;
                v1 = (DCTSqrt2 * p[4] + 128) >> 8;
                v2 = p[2];
                v3 = p[6];
                v4 = (DCTSqrt1D2 * (p[1] - p[7]) + 128) >> 8;
                v7 = (DCTSqrt1D2 * (p[1] + p[7]) + 128) >> 8;
                v5 = p[3] << 4;
                v6 = p[5] << 4;

                // stage 3
                t = (v0 - v1 + 1) >> 1;
                v0 = (v0 + v1 + 1) >> 1;
                v1 = t;
                t = (v2 * DCTSin6 + v3 * DCTCos6 + 128) >> 8;
                v2 = (v2 * DCTCos6 - v3 * DCTSin6 + 128) >> 8;
                v3 = t;
                t = (v4 - v6 + 1) >> 1;
                v4 = (v4 + v6 + 1) >> 1;
                v6 = t;
                t = (v7 + v5 + 1) >> 1;
                v5 = (v7 - v5 + 1) >> 1;
                v7 = t;

                // stage 2
                t = (v0 - v3 + 1) >> 1;
                v0 = (v0 + v3 + 1) >> 1;
                v3 = t;
                t = (v1 - v2 + 1) >> 1;
                v1 = (v1 + v2 + 1) >> 1;
                v2 = t;
                t = (v4 * DCTSin3 + v7 * DCTCos3 + 2048) >> 12;
                v4 = (v4 * DCTCos3 - v7 * DCTSin3 + 2048) >> 12;
                v7 = t;
                t = (v5 * DCTSin1 + v6 * DCTCos1 + 2048) >> 12;
                v5 = (v5 * DCTCos1 - v6 * DCTSin1 + 2048) >> 12;
                v6 = t;

                // stage 1
                p[0] = v0 + v7;
                p[7] = v0 - v7;
                p[1] = v1 + v6;
                p[6] = v1 - v6;
                p[2] = v2 + v5;
                p[5] = v2 - v5;
                p[3] = v3 + v4;
                p[4] = v3 - v4;
            }

            // inverse DCT on columns
            for (i = 0; i < 8; ++i)
            {
                p = dataIn + i;
                if (p[1 * 8] == 0 && p[2 * 8] == 0 && p[3 * 8] == 0 && p[4 * 8] == 0 && p[5 * 8] == 0 && p[6 * 8] == 0 && p[7 * 8] == 0)
                {
                    t = (DCTSqrt2 * dataIn[i + 0] + 8192) >> 14;
                    p[0 * 8] = t;
                    p[1 * 8] = t;
                    p[2 * 8] = t;
                    p[3 * 8] = t;
                    p[4 * 8] = t;
                    p[5 * 8] = t;
                    p[6 * 8] = t;
                    p[7 * 8] = t;
                    continue;
                }

                // stage 4
                v0 = (DCTSqrt2 * p[0 * 8] + 2048) >> 12;
                v1 = (DCTSqrt2 * p[4 * 8] + 2048) >> 12;
                v2 = p[2 * 8];
                v3 = p[6 * 8];
                v4 = (DCTSqrt1D2 * (p[1 * 8] - p[7 * 8]) + 2048) >> 12;
                v7 = (DCTSqrt1D2 * (p[1 * 8] + p[7 * 8]) + 2048) >> 12;
                v5 = p[3 * 8];
                v6 = p[5 * 8];

                // stage 3
                t = (v0 - v1 + 1) >> 1;
                v0 = (v0 + v1 + 1) >> 1;
                v1 = t;
                t = (v2 * DCTSin6 + v3 * DCTCos6 + 2048) >> 12;
                v2 = (v2 * DCTCos6 - v3 * DCTSin6 + 2048) >> 12;
                v3 = t;
                t = (v4 - v6 + 1) >> 1;
                v4 = (v4 + v6 + 1) >> 1;
                v6 = t;
                t = (v7 + v5 + 1) >> 1;
                v5 = (v7 - v5 + 1) >> 1;
                v7 = t;

                // stage 2
                t = (v0 - v3 + 1) >> 1;
                v0 = (v0 + v3 + 1) >> 1;
                v3 = t;
                t = (v1 - v2 + 1) >> 1;
                v1 = (v1 + v2 + 1) >> 1;
                v2 = t;
                t = (v4 * DCTSin3 + v7 * DCTCos3 + 2048) >> 12;
                v4 = (v4 * DCTCos3 - v7 * DCTSin3 + 2048) >> 12;
                v7 = t;
                t = (v5 * DCTSin1 + v6 * DCTCos1 + 2048) >> 12;
                v5 = (v5 * DCTCos1 - v6 * DCTSin1 + 2048) >> 12;
                v6 = t;

                // stage 1
                p[0 * 8] = v0 + v7;
                p[7 * 8] = v0 - v7;
                p[1 * 8] = v1 + v6;
                p[6 * 8] = v1 - v6;
                p[2 * 8] = v2 + v5;
                p[5 * 8] = v2 - v5;
                p[3 * 8] = v3 + v4;
                p[4 * 8] = v3 - v4;
            }

            // convert to 8-bit integers
            for (i = 0; i < 64; ++i)
                dataOut[i] = DCTClip[DCTClipOffset + 128 + ((dataIn[i] + 8) >> 4)];
        }

        private int readHuffSym(DCTHuffTable table)
        {
            short code = 0;
            int bit, codeBits = 0;

            do
            {
                if ((bit = readBit()) == -1)
                    return 9999;

                code = (short)((code << 1) + bit);
                ++codeBits;

                if (code < table.FirstCode[codeBits])
                    break;
                if (code - table.FirstCode[codeBits] < table.NumCodes[codeBits])
                {
                    code -= table.FirstCode[codeBits];
                    return table.Sym[table.FirstSym[codeBits] + code];
                }

            } while (codeBits < 16);

            return 9999;
        }

        private int readAmp(int size)
        {
            int amp = 0, bit;
            for (int bits = 0; bits < size; ++bits)
            {
                if ((bit = readBit()) == -1)
                    return 9999;
                amp = (amp << 1) + bit;
            }

            if (amp < (1 << (size - 1)))
                amp -= (1 << size) - 1;
            return amp;
        }

        private void restart()
        {
            _inputBits = 0;
            _restartCtr = _restartInterval;
            for (int i = 0; i < _numComps; ++i)
                _compInfo[i].PrevDC = 0;
            _eobRun = 0;
        }

        private int readBit()
        {
            int bit, c, c2;
            if (_inputBits == 0)
            {
                if ((c = _inputStream.ReadByte()) == -1)
                    return -1;
                if (c == 0xff)
                {
                    do
                        c2 = _inputStream.ReadByte();
                    while (c2 == 0xff);

                    if (c2 != 0x00)
                        return -1;
                }

                _inputBuf = c;
                _inputBits = 8;
            }

            bit = (_inputBuf >> (_inputBits - 1)) & 1;
            --_inputBits;

            return bit;
        }

        private bool readHeader()
        {
            bool doScan = false;
            int n, c = 0, i;

            while (!doScan)
            {
                c = readMarker();
                switch (c)
                {
                    case 0xc0:
                    case 0xc1:
                        if (!readBaselineSOF())
                            return false;
                        break;
                    case 0xc2:
                        if (!readProgressiveSOF())
                            return false;
                        break;
                    case 0xc4:
                        if (!readHuffmanTables())
                            return false;
                        break;
                    case 0xd8:
                        break;
                    case 0xd9:
                        return false;
                    case 0xda:
                        if (!readScanInfo())
                            return false;
                        doScan = true;
                        break;
                    case 0xdb:
                        if (!readQuantTables())
                            return false;
                        break;
                    case 0xdd:
                        if (!readRestartInterval())
                            return false;
                        break;
                    case 0xe0:
                        if (!readJFIFMarker())
                            return false;
                        break;
                    case 0xee:
                        if (!readAdobeMarker())
                            return false;
                        break;
                    case -1:
                        return false;
                    default:
                        // skip APPn / COM / etc.
                        if (c >= 0xe0)
                        {
                            n = read16() - 2;
                            for (i = 0; i < n; ++i)
                                _inputStream.ReadByte();
                        }
                        else
                        {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        private bool readBaselineSOF()
        {
            int length = read16();
            int prec = _inputStream.ReadByte();
            _height = read16();
            _width = read16();
            _numComps = _inputStream.ReadByte();

            if (_numComps <= 0 || _numComps > 4)
            {
                _numComps = 0;
                return false;
            }

            if (prec != 8)
                return false;

            int c;
            for (int i = 0; i < _numComps; ++i)
            {
                _compInfo[i].ID = _inputStream.ReadByte();
                c = _inputStream.ReadByte();
                _compInfo[i].HSample = (c >> 4) & 0x0f;
                _compInfo[i].VSample = c & 0x0f;
                _compInfo[i].QuantTable = _inputStream.ReadByte();

                if (_compInfo[i].HSample < 1 || _compInfo[i].HSample > 4 || _compInfo[i].VSample < 1 || _compInfo[i].VSample > 4)
                    return false;

                if (_compInfo[i].QuantTable < 0 || _compInfo[i].QuantTable > 3)
                    return false;
            }

            _progressive = false;
            return true;
        }

        private bool readProgressiveSOF()
        {
            int length = read16();
            int prec = _inputStream.ReadByte();
            _height = read16();
            _width = read16();
            _numComps = _inputStream.ReadByte();

            if (_numComps <= 0 || _numComps > 4)
            {
                _numComps = 0;
                return false;
            }

            if (prec != 8)
                return false;

            int c;
            for (int i = 0; i < _numComps; ++i)
            {
                _compInfo[i].ID = _inputStream.ReadByte();
                c = _inputStream.ReadByte();
                _compInfo[i].HSample = (c >> 4) & 0x0f;
                _compInfo[i].VSample = c & 0x0f;
                _compInfo[i].QuantTable = _inputStream.ReadByte();

                if (_compInfo[i].HSample < 1 || _compInfo[i].HSample > 4 || _compInfo[i].VSample < 1 || _compInfo[i].VSample > 4)
                    return false;

                if (_compInfo[i].QuantTable < 0 || _compInfo[i].QuantTable > 3)
                    return false;
            }

            _progressive = true;
            return true;
        }

        private bool readScanInfo()
        {
            int length = read16() - 2;
            _scanInfo.NumComps = _inputStream.ReadByte();

            if (_scanInfo.NumComps <= 0 || _scanInfo.NumComps > 4)
            {
                _scanInfo.NumComps = 0;
                return false;
            }
            --length;

            if (length != 2 * _scanInfo.NumComps + 3)
                return false;
            _interleaved = _scanInfo.NumComps == _numComps;

            int id, c, j;
            for (j = 0; j < _numComps; ++j)
                _scanInfo.Comp[j] = false;

            for (int i = 0; i < _scanInfo.NumComps; ++i)
            {
                id = _inputStream.ReadByte();
                if (id == _compInfo[i].ID)
                {
                    j = i;
                }
                else
                {
                    for (j = 0; j < _numComps; ++j)
                    {
                        if (id == _compInfo[j].ID)
                            break;
                    }

                    if (j == _numComps)
                        return false;
                }

                _scanInfo.Comp[j] = true;
                c = _inputStream.ReadByte();
                _scanInfo.DCHuffTable[j] = (c >> 4) & 0x0f;
                _scanInfo.ACHuffTable[j] = c & 0x0f;
            }

            _scanInfo.FirstCoeff = _inputStream.ReadByte();
            _scanInfo.LastCoeff = _inputStream.ReadByte();
            if (_scanInfo.FirstCoeff < 0 || _scanInfo.LastCoeff > 63 || _scanInfo.FirstCoeff > _scanInfo.LastCoeff)
                return false;

            c = _inputStream.ReadByte();
            _scanInfo.Ah = (c >> 4) & 0x0f;
            _scanInfo.Al = c & 0x0f;
            return true;
        }

        private bool readQuantTables()
        {
            int prec, i, index;
            int length = read16() - 2;

            while (length > 0)
            {
                index = _inputStream.ReadByte();
                prec = (index >> 4) & 0x0f;
                index &= 0x0f;

                if (prec > 1 || index >= 4)
                    return false;
                if (index == _numQuantTables)
                    _numQuantTables = index + 1;

                for (i = 0; i < 64; ++i)
                {
                    if (prec != 0)
                        _quantTables[index, DCTZigZag[i]] = (short)read16();
                    else
                        _quantTables[index, DCTZigZag[i]] = (short)_inputStream.ReadByte();
                }

                if (prec != 0)
                    length -= 129;
                else
                    length -= 65;
            }

            return true;
        }

        private bool readHuffmanTables()
        {
            DCTHuffTable tbl;
            int index;
            short code;
            byte sym;
            int i, c;
            int length = read16() - 2;

            while (length > 0)
            {
                index = _inputStream.ReadByte();
                --length;

                if ((index & 0x0f) >= 4)
                    return false;
                if ((index & 0x10) != 0)
                {
                    index &= 0x0f;
                    if (index >= _numACHuffTables)
                        _numACHuffTables = index + 1;
                    tbl = _acHuffTables[index];
                }
                else
                {
                    index &= 0x0f;
                    if (index >= _numDCHuffTables)
                        _numDCHuffTables = index + 1;
                    tbl = _dcHuffTables[index];
                }

                sym = 0;
                code = 0;
                for (i = 1; i <= 16; ++i)
                {
                    c = _inputStream.ReadByte();
                    tbl.FirstSym[i] = sym;
                    tbl.FirstCode[i] = code;
                    tbl.NumCodes[i] = (short)c;
                    sym += (byte)c;
                    code = (short)((code + c) << 1);
                }

                length -= 16;
                for (i = 0; i < sym; ++i)
                    tbl.Sym[i] = (byte)_inputStream.ReadByte();
                length -= sym;
            }

            return true;
        }

        private bool readRestartInterval()
        {
            int length = read16();
            if (length != 4)
                return false;
            _restartInterval = read16();
            return true;
        }

        private bool readJFIFMarker()
        {
            int length = read16() - 2;
            if (length >= 5)
            {
                byte[] buf = new byte[5];
                if (_inputStream.Read(buf, 0, 5) != 5)
                    return false;
                if (System.Text.Encoding.ASCII.GetString(buf, 0, 5) != "JFIF\0")
                    return false;
                length -= 5;
            }

            while (length > 0)
            {
                if (_inputStream.ReadByte() == -1)
                    return false;
                --length;
            }
            return true;
        }

        private bool readAdobeMarker()
        {
            int length = read16();
            if (length < 14)
                return false;

            byte[] buf = new byte[12];
            if (_inputStream.Read(buf, 0, 12) != 12)
                return false;
            if (System.Text.Encoding.ASCII.GetString(buf, 0, 5) != "Adobe")
                return false;

            _colorXform = buf[11];
            for (int i = 14; i < length; ++i)
            {
                if (_inputStream.ReadByte() == -1)
                    return false;
            }

            return true;
        }

        private bool readTrailer()
        {
            int c = readMarker();
            if (c != 0xd9)
                return false;
            return true;
        }

        private int readMarker()
        {
            int c;
            do
            {
                do
                    c = _inputStream.ReadByte();
                while (c != 0xff && c != -1);

                do
                    c = _inputStream.ReadByte();
                while (c == 0xff);
            } while (c == 0x00);

            return c;
        }

        private int read16()
        {
            int c1, c2;
            if ((c1 = _inputStream.ReadByte()) == -1)
                return -1;
            if ((c2 = _inputStream.ReadByte()) == -1)
                return -1;
            return (c1 << 8) + c2;
        }

	    static DCTStream()
        {
            DCTClip = new byte[768];
            for (int i = -256; i < 0; ++i)
                DCTClip[DCTClipOffset + i] = 0;
            for (int i = 0; i < 256; ++i)
                DCTClip[DCTClipOffset + i] = (byte)i;
            for (int i = 256; i < 512; ++i)
                DCTClip[DCTClipOffset + i] = 255;
        }
    }
#endif
}
