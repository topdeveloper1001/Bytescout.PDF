using System;
using System.Drawing;
using System.IO;

namespace Bytescout.PDF
{
    internal class HeadTable : TTFTable
    {
	    private Point _version = Point.Empty;
	    private Point _fontRevision = Point.Empty;
	    private UInt32 _checkSumAdjustment;
	    private UInt32 _magicNumber;
	    private UInt16 _flags;
	    private UInt16 _unitsPerEm;
	    private long _created;
	    private long _modified;
	    private int _xMin;
	    private int _yMin;
	    private int _xMax;
	    private int _yMax;
	    private UInt16 _macStyle;
	    private UInt16 _lowestRecPPEM;
	    private Int16 _fontDirectionHint;
	    private Int16 _indexToLocFormat;
	    private Int16 _glyphDataFormat;
	    public Int16 IndexToLocFormat { get { return _indexToLocFormat; } }

        public ushort UnitsPerEm { get { return _unitsPerEm; } }

        public bool Bold
        {
            get
            {
                return _macStyle % 2 != 0;
            }
        }

        public System.Drawing.Rectangle GetBBox()
        {
            return new System.Drawing.Rectangle(_xMin, _yMin, _xMax, _yMax);
        }

        public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            reader.Position = offset;

            _version.X = reader.ReadUint16();
            _version.Y = reader.ReadUint16();
            _fontRevision.X = reader.ReadUint16();
            _fontRevision.Y = reader.ReadUint16();
            _checkSumAdjustment = (UInt32)reader.ReadUint32();
            _magicNumber = (UInt32)reader.ReadUint32();//1594834165
            if (_magicNumber != 1594834165)
                throw new PDFWrongFontFileException();

            _flags = (UInt16)reader.ReadUint16();
            _unitsPerEm = (UInt16)reader.ReadUint16();
            _created = reader.ReadInt64();
            _modified = reader.ReadInt64();
            _xMin = (short)reader.ReadUint16();
            _yMin = (short)reader.ReadUint16();
            _xMax = (short)reader.ReadUint16();
            _yMax = (short)reader.ReadUint16();
            _macStyle = (UInt16)reader.ReadUint16();
            _lowestRecPPEM = (UInt16)reader.ReadUint16();
            _fontDirectionHint = (Int16)reader.ReadUint16();
            _indexToLocFormat = (Int16)reader.ReadUint16();
            _glyphDataFormat = (Int16)reader.ReadUint16();
        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            MemoryStream stream = new MemoryStream();

            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_version.X), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_version.Y), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_fontRevision.X), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_fontRevision.Y), 0, 2);
            stream.Write(BinaryUtility.UInt32ToBytes(_checkSumAdjustment), 0, 4);
            stream.Write(BinaryUtility.UInt32ToBytes(_magicNumber), 0, 4);
            stream.Write(BinaryUtility.UInt16ToBytes(_flags), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_unitsPerEm), 0, 2);
            stream.Write(BinaryUtility.UInt64ToBytes((ulong)_created), 0, 8);
            stream.Write(BinaryUtility.UInt64ToBytes((ulong)_modified), 0, 8);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_xMin), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_yMin), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_xMax), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_yMax), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_macStyle), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_lowestRecPPEM), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_fontDirectionHint), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_indexToLocFormat), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_glyphDataFormat), 0, 2);

            stream.Position = 0;
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            return data;
        }
    }
}
