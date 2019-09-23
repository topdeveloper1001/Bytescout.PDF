using System;
using System.Collections.Generic;

namespace Bytescout.PDF
{
    internal class LocaTable : TTFTable
    {
	    private readonly List<UInt32> _offsets = new List<uint>();
	    private int _indexToLocFormat;
	    private int _numGlyphs;

	    public List<uint> Offsets { get { return _offsets; } }

	    public LocaTable(int indexToLocFormat, int numGlyphs)
	    {
		    _indexToLocFormat = indexToLocFormat;
		    _numGlyphs = numGlyphs;
	    }

	    public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            reader.Position = offset;
            switch (_indexToLocFormat)
            {
                case 0:
                    for (int i = 0; i <= _numGlyphs; ++i)
                        _offsets.Add((UInt32)(reader.ReadUint16() * 2));
                    break;
                case 1:
                    for (int i = 0; i <= _numGlyphs; ++i)
                        _offsets.Add((UInt32)reader.ReadUint32());
                    break;
            }
        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            uint length = 0;
            int count = glyfIndexes[glyfIndexes.Length - 1];
            int curIndex = 0;
            for (int i = 0; i <= count; ++i)
            {
                if (i >= _offsets.Count)
                    break;

                writeOffset(length, ms);
                if (i == glyfIndexes[curIndex])
                {
                    length += _offsets[i + 1] - _offsets[i];
                    curIndex++;
                }
            }
            writeOffset(length, ms);

            byte[] tempFontData = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(tempFontData, 0, tempFontData.Length);
            return tempFontData;
        }

        private void writeOffset(uint value, System.IO.MemoryStream stream)
        {
            if (_indexToLocFormat == 0)
                stream.Write(BinaryUtility.UInt16ToBytes((ushort)(value / 2)), 0, 2);
            else
                stream.Write(BinaryUtility.UInt32ToBytes(value), 0, 4);
        }
    }
}
