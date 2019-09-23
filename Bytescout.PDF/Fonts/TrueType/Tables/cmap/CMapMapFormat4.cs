using System;
using System.Collections.Generic;
using System.IO;

namespace Bytescout.PDF
{
    internal class CMapMapFormat4 : CMapBase
    {
	    private UInt16 _segCountX2;
	    private UInt16 _searchRange;
	    private UInt16 _entrySelector;
	    private UInt16 _rangeShift;
	    private List<UInt16> _endCode = new List<ushort>();
	    private UInt16 _reservedPad;
	    private List<UInt16> _startCode = new List<ushort>();
	    private List<UInt16> _idDelta = new List<ushort>();
	    private List<UInt16> _idRangeOffset = new List<ushort>();
	    private List<UInt16> _glyphIndexArray = new List<ushort>();

	    public static byte[] CreateData(FontMap map)
        {
            List<char> characters = map.Characters;
            characters.Sort();

            List<UInt16> endCode = new List<ushort>();
            List<UInt16> startCode = new List<ushort>();
            List<UInt16> idDelta = new List<ushort>();
            List<UInt16> idRangeOffset = new List<ushort>();

            ushort delta = 0;
            for (int i = 0; i < characters.Count; ++i)
            {
                delta = (ushort)(map.GetGlyfIndex(characters[i]) - characters[i] + 65536);

                startCode.Add(characters[i]);
                idDelta.Add(delta);
                idRangeOffset.Add(0);

                int j;
                for (j = i; j < characters.Count; ++j)
                {
                    if (j == characters.Count - 1 || characters[j] + 1 != characters[j + 1] ||
                    delta != map.GetGlyfIndex(characters[j]) - characters[j] + 65536)
                        break;
                }

                i = j;
                endCode.Add(characters[i]);
            }

            endCode.Add(65535);
            startCode.Add(65535);
            idDelta.Add(1);
            idRangeOffset.Add(0);

            ushort segCountX2 = (ushort)(endCode.Count * 2);
            ushort power = 0;
            ushort maxPowOf2 = getMaxPowerOf2((ushort)(segCountX2 / 2), ref power);
            ushort searchRange = (ushort)(2 * maxPowOf2);
            ushort entrySelector = power;
            ushort rangeShift = (ushort)(segCountX2 - searchRange);

            MemoryStream stream = new MemoryStream();

            stream.Write(BinaryUtility.UInt16ToBytes(segCountX2), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(searchRange), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(entrySelector), 0, 2);
            stream.Write(BinaryUtility.UInt16ToBytes(rangeShift), 0, 2);

            for (int i = 0; i < endCode.Count; ++i)
                stream.Write(BinaryUtility.UInt16ToBytes(endCode[i]), 0, 2);

            stream.Write(BinaryUtility.UInt16ToBytes(0), 0, 2);

            for (int i = 0; i < startCode.Count; ++i)
                stream.Write(BinaryUtility.UInt16ToBytes(startCode[i]), 0, 2);

            for (int i = 0; i < idDelta.Count; ++i)
                stream.Write(BinaryUtility.UInt16ToBytes(idDelta[i]), 0, 2);

            for (int i = 0; i < idRangeOffset.Count; ++i)
                stream.Write(BinaryUtility.UInt16ToBytes(idRangeOffset[i]), 0, 2);

            byte[] buf = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(buf, 0, buf.Length);

            return buf;
        }

        public override void Read(Reader reader)
        {
            base.Read(reader);

            _segCountX2 = (UInt16)reader.ReadUint16();
            _searchRange = (UInt16)reader.ReadUint16();
            _entrySelector = (UInt16)reader.ReadUint16();
            _rangeShift = (UInt16)reader.ReadUint16();

            int segCount = _segCountX2 / 2;
            for (int i = 0; i < segCount; ++i)
                _endCode.Add((UInt16)reader.ReadUint16());

            _reservedPad = (UInt16)reader.ReadUint16();

            for (int i = 0; i < segCount; ++i)
                _startCode.Add((UInt16)reader.ReadUint16());

            for (int i = 0; i < segCount; ++i)
                _idDelta.Add((UInt16)reader.ReadUint16());

            for (int i = 0; i < segCount; ++i)
                _idRangeOffset.Add((UInt16)reader.ReadUint16());

            int length = 0;
            for (int i = 0; i < segCount - 1; ++i)
            {
                if (_idRangeOffset[i] != 0)
                    length += _endCode[i] - _startCode[i] + 1;
            }

            for (int i = 0; i < length; ++i)
                _glyphIndexArray.Add((UInt16)reader.ReadUint16());
        }

        public override ushort GetGlyphIndex(ushort characterCode)
        {
            if (characterCode == 0xFFFF)
                return 0;

            int segCount = _segCountX2 / 2;

            try
            {
                //"walking" through endCount
                ushort indexOfSegment = getFirstGreaterSegment(characterCode);
                if (_startCode[indexOfSegment] > characterCode)
                    return 0;

                ushort glyphIndex = 0;
                if (_idRangeOffset[indexOfSegment] == 0)
                {
                    glyphIndex = (ushort)(characterCode + _idDelta[indexOfSegment]);
                }
                else
                {
                    //calculating offset from idRangeOffset[indexOfSegment] (we visualizing 
                    //that idRangeOffset and glyphIdArray store sequentially as in font file)
                    int offset = _idRangeOffset[indexOfSegment] + (characterCode - _startCode[indexOfSegment]) * 2;

                    //calculating distance in bytes between dRangeOffset[indexOfSegment] and 
                    //beginning of glyphIdArray (as in font file)
                    int offsetToGlyphsArray = (segCount - indexOfSegment) * 2;

                    offset -= offsetToGlyphsArray;
                    int indexInGlyphsArray = offset / 2;
                    glyphIndex = (ushort)(_glyphIndexArray[indexInGlyphsArray] + _idDelta[indexOfSegment]);
                }

                return glyphIndex;
            }
            catch
            {
            }

            return 0;
        }

        private static ushort getMaxPowerOf2(ushort number, ref ushort power)
        {
            ushort res = 1;
            ushort _power = 0;
            for (; res <= number; res *= 2, _power += 1) ;

            power = (ushort)(_power - 1);
            return (ushort)(res / 2);
        }

        private ushort getFirstGreaterSegment(ushort characterCode)
        {
            int segCount = _segCountX2 / 2;
            for (ushort indexOfSegment = 0; indexOfSegment < segCount; ++indexOfSegment)
            {
                if (_endCode[indexOfSegment] >= characterCode)
                    return indexOfSegment;
            }

            throw new Exception();
        }
    }
}
