using System;
using System.Collections.Generic;

namespace Bytescout.PDF
{
    internal class GlyphTable : TTFTable
    {
	    private const int ARG_1_AND_2_ARE_WORDS = 0x0001;
	    private const int WE_HAVE_A_SCALE = 0x0008;
	    private const int WE_HAVE_AN_X_AND_Y_SCALE = 0x0040;
	    private const int WE_HAVE_A_TWO_BY_TWO = 0x0080;
	    private byte[] _data;
	    private readonly List<uint> _offsets;

	    public GlyphTable(List<uint> offsets)
        {
            _offsets = offsets;
        }

        public ushort[] GetUsedGlyfIndexes(int index)
        {
            if (index >= _offsets.Count - 1)
                return null;

            if (_offsets[index] != _offsets[index + 1])
            {
                int nGlyphBytes = (int)(_offsets[index + 1] - _offsets[index]);
                if (nGlyphBytes > 0)
                {
                    Reader reader = new Reader(_data);
                    reader.Position = (int)_offsets[index];
                    Int16 numberOfContours = (Int16)reader.ReadUint16();
                    if (numberOfContours >= 0)
                        return new ushort[] { (ushort)index };

                    List<ushort> indexes = new List<ushort>();
                    indexes.Add((ushort)index);

                    reader.Position += 8;
                    ushort flags = 0;

                    do
                    {
                        flags = (UInt16)reader.ReadUint16();
                        ushort glyphIndex = (ushort)reader.ReadUint16();
                        indexes.Add(glyphIndex);

                        if ((flags & /*_COMPOSITE_GLYPH_FLAGS::*/ARG_1_AND_2_ARE_WORDS) != 0)                        
                            reader.Position += 4;                        
                        else                        
                            reader.Position += 2;                        

                        if ((flags & WE_HAVE_A_SCALE) != 0)                        
                            reader.Position += 2;                        
                        else if ((flags & WE_HAVE_AN_X_AND_Y_SCALE) != 0)                        
                            reader.Position += 4;                        
                        else if ((flags & WE_HAVE_A_TWO_BY_TWO) != 0)                        
                            reader.Position += 8;
                    }
                    while ((flags & 0x0020) != 0);

                    return indexes.ToArray();
                }
            }

            return new ushort[] { (ushort)index };
        }

        public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            _data = new byte[length];
            reader.Position = offset;

            if (reader.Read(_data) < 0)
                throw new PDFWrongFontFileException();
        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            for (int i = 0; i < glyfIndexes.Length; ++i)
            {
                int index = glyfIndexes[i];
                if (index >= _offsets.Count)
                    break;

                int length = (int)(_offsets[index + 1] - _offsets[index]);
                if (length > 0)
                    ms.Write(_data, (int)_offsets[index], (int)length);
            }

            byte[] tempFontData = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(tempFontData, 0, tempFontData.Length);
            return tempFontData;
        }
    }
}
