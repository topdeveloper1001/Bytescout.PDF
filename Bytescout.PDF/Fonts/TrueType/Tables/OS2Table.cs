using System;
using System.IO;

namespace Bytescout.PDF
{
    internal class OS2Table : TTFTable
    {
	    private UInt16 _version;
	    private Int16 _xAvgCharWidth;
	    private UInt16 _usWeightClass;
	    private UInt16 _usWidthClass;
	    private Int16 _fsType;
	    private Int16 _ySubscriptXSize;
	    private Int16 _ySubscriptYSize;
	    private Int16 _ySubscriptXOffset;
	    private Int16 _ySubscriptYOffset;
	    private Int16 _ySuperscriptXSize;
	    private Int16 _ySuperscriptYSize;
	    private Int16 _ySuperscriptXOffset;
	    private Int16 _ySuperscriptYOffset;
	    private Int16 _yStrikeoutSize;
	    private Int16 _yStrikeoutPosition;
	    private Int16 _sFamilyClass;
	    private byte[] _panose = new byte[10];
	    private UInt32[] _ulCharRange = new UInt32[4];
	    private byte[] _achVendID = new byte[4];
	    private UInt16 _fsSelection;
	    private UInt16 _fsFirstCharIndex;
	    private UInt16 _fsLastCharIndex;
	    //version > 0
	    private Int16 _sTypoAscender;
	    private Int16 _sTypoDescender;
	    private Int16 _sTypoLineGap;
	    private UInt16 _usWinAscent;
	    private UInt16 _usWinDescent;
	    private UInt32 _ulCodePageRange1;
	    private UInt32 _ulCodePageRange2;
	    private Int16 _sxHeight;
	    private Int16 _sCapHeight;
	    private UInt16 _usDefaultChar;
	    private UInt16 _usBreakChar;
	    private UInt16 _usMaxContext;
	    public ushort UsWeightClass { get { return _usWeightClass; } }

        public byte GetPanoseByte(ushort byteNumber)
        {
            return _panose[byteNumber];
        }

        public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            reader.Position = offset;

            _version = (UInt16)reader.ReadUint16();
            _xAvgCharWidth = (Int16)reader.ReadUint16();
            _usWeightClass = (UInt16)reader.ReadUint16();
            _usWidthClass = (UInt16)reader.ReadUint16();
            _fsType = (Int16)reader.ReadUint16();
            _ySubscriptXSize = (Int16)reader.ReadUint16();
            _ySubscriptYSize = (Int16)reader.ReadUint16();
            _ySubscriptXOffset = (Int16)reader.ReadUint16();
            _ySubscriptYOffset = (Int16)reader.ReadUint16();
            _ySuperscriptXSize = (Int16)reader.ReadUint16();
            _ySuperscriptYSize = (Int16)reader.ReadUint16();
            _ySuperscriptXOffset = (Int16)reader.ReadUint16();
            _ySuperscriptYOffset = (Int16)reader.ReadUint16();
            _yStrikeoutSize = (Int16)reader.ReadUint16();
            _yStrikeoutPosition = (Int16)reader.ReadUint16();
            _sFamilyClass = (Int16)reader.ReadUint16();

            reader.Read(_panose);
            for (int i = 0; i < _ulCharRange.Length; ++i)
                _ulCharRange[i] = (UInt32)reader.ReadUint32();
            for (int i = 0; i < _achVendID.Length; ++i)
                _achVendID[i] = (byte)reader.ReadByte();

            _fsSelection = (UInt16)reader.ReadUint16();
            _fsFirstCharIndex = (UInt16)reader.ReadUint16();
            _fsLastCharIndex = (UInt16)reader.ReadUint16();

            if (_version > 0)
            {
                _sTypoAscender = (Int16)reader.ReadUint16();
                _sTypoDescender = (Int16)reader.ReadUint16();
                _sTypoLineGap = (Int16)reader.ReadUint16();
                _usWinAscent = (UInt16)reader.ReadUint16();
                _usWinDescent = (UInt16)reader.ReadUint16();
                _ulCodePageRange1 = (UInt32)reader.ReadUint32();
                _ulCodePageRange2 = (UInt32)reader.ReadUint32();
                _sxHeight = (Int16)reader.ReadUint16();
                _sCapHeight = (Int16)reader.ReadUint16();
                _usDefaultChar = (UInt16)reader.ReadUint16();
                _usBreakChar = (UInt16)reader.ReadUint16();
                _usMaxContext = (UInt16)reader.ReadUint16();
            }

        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            MemoryStream stream = new MemoryStream();

            stream.Write(BinaryUtility.UInt16ToBytes(_version), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_xAvgCharWidth), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_usWeightClass), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_usWidthClass), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_fsType), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_ySubscriptXSize), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_ySubscriptYSize), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_ySubscriptXOffset), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_ySubscriptYOffset), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_ySuperscriptXSize), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_ySuperscriptYSize), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_ySuperscriptXOffset), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_ySuperscriptYOffset), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_yStrikeoutSize), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_yStrikeoutPosition), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_sFamilyClass), 0, 2);

            stream.Write(_panose, 0, _panose.Length);
            for (int i = 0; i < _ulCharRange.Length; ++i)
                stream.Write(BinaryUtility.UInt32ToBytes(_ulCharRange[i]), 0, 4);
            stream.Write(_achVendID, 0, _achVendID.Length);

            stream.Write(BinaryUtility.UInt16ToBytes(_fsSelection), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_fsFirstCharIndex), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_fsLastCharIndex), 0, 2);

            if (_version > 0)
            {
                stream.Write(BinaryUtility.UInt16ToBytes((ushort)_sTypoAscender), 0, 2);
                stream.Write(BinaryUtility.UInt16ToBytes((ushort)_sTypoDescender), 0, 2);
                stream.Write(BinaryUtility.UInt16ToBytes((ushort)_sTypoLineGap), 0, 2);
                stream.Write(BinaryUtility.UInt16ToBytes(_usWinAscent), 0, 2);
                stream.Write(BinaryUtility.UInt16ToBytes(_usWinDescent), 0, 2);
                stream.Write(BinaryUtility.UInt32ToBytes(_ulCodePageRange1), 0, 4);
                stream.Write(BinaryUtility.UInt32ToBytes(_ulCodePageRange2), 0, 4);
                stream.Write(BinaryUtility.UInt16ToBytes((ushort)_sxHeight), 0, 2);
                stream.Write(BinaryUtility.UInt16ToBytes((ushort)_sCapHeight), 0, 2);
                stream.Write(BinaryUtility.UInt16ToBytes(_usDefaultChar), 0, 2);
                stream.Write(BinaryUtility.UInt16ToBytes(_usBreakChar), 0, 2);
                stream.Write(BinaryUtility.UInt16ToBytes(_usMaxContext), 0, 2);
            }

            stream.Position = 0;
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            return data;
        }

        //version 0
    }
}
