using System;
using System.Drawing;

namespace Bytescout.PDF
{
    internal class PostTable : TTFTable
    {
	    private Point _format = Point.Empty;
	    private Point _italicAngle = Point.Empty;
	    private int _underlinePosition;
	    private int _underlineThickness;
	    private UInt32 _isFixedPitch;
	    private UInt32 _minMemType42;
	    private UInt32 _maxMemType42;
	    private UInt32 _minMemType1;
	    private UInt32 _maxMemType1;

	    public Point ItalicAngle { get { return _italicAngle; } }

	    public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            reader.Position = offset;

            _format.X = reader.ReadUint16();
            _format.Y = reader.ReadUint16();
            _italicAngle.X = reader.ReadUint16();
            _italicAngle.Y = reader.ReadUint16();
            _underlinePosition = (short)reader.ReadUint16();
            _underlineThickness = (short)reader.ReadUint16();
            _isFixedPitch = (UInt32)reader.ReadUint32();
            _minMemType42 = (UInt32)reader.ReadUint32();
            _maxMemType42 = (UInt32)reader.ReadUint32();
            _minMemType1 = (UInt32)reader.ReadUint32();
            _maxMemType1 = (UInt32)reader.ReadUint32();
        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(BinaryUtility.UInt16ToBytes((ushort)_format.X), 0, 2);
            ms.Write(BinaryUtility.UInt16ToBytes((ushort)_format.Y), 0, 2);
            ms.Write(BinaryUtility.UInt16ToBytes((ushort)_italicAngle.X), 0, 2);
            ms.Write(BinaryUtility.UInt16ToBytes((ushort)_italicAngle.Y), 0, 2);
            ms.Write(BinaryUtility.UInt16ToBytes((ushort)_underlinePosition), 0, 2);
            ms.Write(BinaryUtility.UInt16ToBytes((ushort)_underlineThickness), 0, 2);
            ms.Write(BinaryUtility.UInt32ToBytes((uint)_isFixedPitch), 0, 4);
            ms.Write(BinaryUtility.UInt32ToBytes((uint)_minMemType42), 0, 4);
            ms.Write(BinaryUtility.UInt32ToBytes((uint)_maxMemType42), 0, 4);
            ms.Write(BinaryUtility.UInt32ToBytes((uint)_minMemType1), 0, 4);
            ms.Write(BinaryUtility.UInt32ToBytes((uint)_maxMemType1), 0, 4);
            ms.Write(BinaryUtility.UInt32ToBytes((uint)_maxMemType1), 0, 4);
            ms.Write(BinaryUtility.UInt16ToBytes(0), 0, 2);

            byte[] tempFontData = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(tempFontData, 0, tempFontData.Length);
            return tempFontData;
        }
    }
}
