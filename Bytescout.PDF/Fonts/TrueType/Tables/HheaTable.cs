using System;
using System.Drawing;
using System.IO;

namespace Bytescout.PDF
{
    internal class HHeaTable : TTFTable
    {
	    private Point _version = Point.Empty;
	    private int _ascent;
	    private int _descent;
	    private int _lineGap;
	    private int _advanceWidthMax;
	    private int _minLeftSideBearing;
	    private int _minRightSideBearing;
	    private int _xMaxExtent;
	    private Int16 _caretSlopeRise;
	    private Int16 _caretSlopeRun;
	    private int _caretOffset;
	    private Int16 _reserved1;
	    private Int16 _reserved2;
	    private Int16 _reserved3;
	    private Int16 _reserved4;
	    private Int16 _metricDataFormat;
	    private UInt16 _numOfLongHorMetrics;
	    public UInt16 NumOfLongHorMetrics { get { return _numOfLongHorMetrics; } }

        public int Ascent { get { return _ascent; } }

        public int Descent { get { return _descent; } }

        public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            reader.Position = offset;

            _version.X = reader.ReadUint16();
            _version.Y = reader.ReadUint16();
            _ascent = (short)reader.ReadUint16();
            _descent = (short)reader.ReadUint16();
            _lineGap = (short)reader.ReadUint16();
            _advanceWidthMax = reader.ReadUint16();
            _minLeftSideBearing = (short)reader.ReadUint16();
            _minRightSideBearing = (short)reader.ReadUint16();
            _xMaxExtent = (short)reader.ReadUint16();
            _caretSlopeRise = (Int16)reader.ReadUint16();
            _caretSlopeRun = (Int16)reader.ReadUint16();
            _caretOffset = (short)reader.ReadUint16();
            _reserved1 = (Int16)reader.ReadUint16();
            _reserved2 = (Int16)reader.ReadUint16();
            _reserved3 = (Int16)reader.ReadUint16();
            _reserved4 = (Int16)reader.ReadUint16();
            _metricDataFormat = (Int16)reader.ReadUint16();
            _numOfLongHorMetrics = (UInt16)reader.ReadUint16();
        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            MemoryStream stream = new MemoryStream();

            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_version.X), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_version.Y), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_ascent), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_descent), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_lineGap), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_advanceWidthMax), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_minLeftSideBearing), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_minRightSideBearing), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_xMaxExtent), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_caretSlopeRise), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_caretSlopeRun), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_caretOffset), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_reserved1), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_reserved2), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_reserved3), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_reserved4), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_metricDataFormat), 0, 2);
            
            ushort hMetricsCount = calculateNumOfLongHorMetrics(glyfIndexes);
            stream.Write(BinaryUtility.UInt16ToBytes(hMetricsCount), 0, 2);

            stream.Position = 0;
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            return data;
        }

        private ushort calculateNumOfLongHorMetrics(ushort[] glyfIndexes)
        {
            for (int i = glyfIndexes.Length - 1; i >= 0; --i)
            {
                if (glyfIndexes[i] < _numOfLongHorMetrics)
                    return (ushort)(glyfIndexes[i] + 1);
            }

            return 0;
        }
    }
}
