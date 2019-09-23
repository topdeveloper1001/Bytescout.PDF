using System;
using System.Drawing;
using System.IO;

namespace Bytescout.PDF
{
    internal class MaxpTable : TTFTable
    {
	    private Point _version = Point.Empty;
	    private UInt16 _numGlyphs;
	    private UInt16 _maxPoints;
	    private UInt16 _maxContours;
	    private UInt16 _maxComponentPoints;
	    private UInt16 _maxComponentContours;
	    private UInt16 _maxZones;
	    private UInt16 _maxTwilightPoints;
	    private UInt16 _maxStorage;
	    private UInt16 _maxFunctionDefs;
	    private UInt16 _maxInstructionDefs;
	    private UInt16 _maxStackElements;
	    private UInt16 _maxSizeOfInstructions;
	    private UInt16 _maxComponentElements;
	    private UInt16 _maxComponentDepth;

	    public UInt16 NumGlyphs { get { return _numGlyphs; } }

	    public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            reader.Position = offset;

            _version.X = reader.ReadUint16();
            _version.Y = reader.ReadUint16();
            _numGlyphs = (UInt16)reader.ReadUint16();
            _maxPoints = (UInt16)reader.ReadUint16();
            _maxContours = (UInt16)reader.ReadUint16();
            _maxComponentPoints = (UInt16)reader.ReadUint16();
            _maxComponentContours = (UInt16)reader.ReadUint16();
            _maxZones = (UInt16)reader.ReadUint16();
            _maxTwilightPoints = (UInt16)reader.ReadUint16();
            _maxStorage = (UInt16)reader.ReadUint16();
            _maxFunctionDefs = (UInt16)reader.ReadUint16();
            _maxInstructionDefs = (UInt16)reader.ReadUint16();
            _maxStackElements = (UInt16)reader.ReadUint16();
            _maxSizeOfInstructions = (UInt16)reader.ReadUint16();
            _maxComponentElements = (UInt16)reader.ReadUint16();
            _maxComponentDepth = (UInt16)reader.ReadUint16();
        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            MemoryStream stream = new MemoryStream();

            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_version.X), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)_version.Y), 0, 2);
            
            ushort glyfsCount = (ushort)(glyfIndexes[glyfIndexes.Length - 1] + 1);
            stream.Write(BinaryUtility.UInt16ToBytes(glyfsCount), 0, 2);

            stream.Write(BinaryUtility.UInt16ToBytes(_maxPoints), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxContours), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxComponentPoints), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxComponentContours), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxZones), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxTwilightPoints), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxStorage), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxFunctionDefs), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxInstructionDefs), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxStackElements), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxSizeOfInstructions), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxComponentElements), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(_maxComponentDepth), 0, 2);

            stream.Position = 0;
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            return data;
        }
    }
}
