using System;
using System.Collections.Generic;
using System.IO;

namespace Bytescout.PDF
{
    internal class CMapMapFormat6 : CMapBase
    {
	    private UInt16 _firstCode;
	    private UInt16 _entryCount;
	    private List<UInt16> _glyphIndexArray = new List<ushort>();

	    public static byte[] CreateData(FontMap map)
        {
            MemoryStream stream = new MemoryStream();
            List<char> characters = map.Characters;
            characters.Sort();

            ushort firstCode = characters[0];
            stream.Write(BinaryUtility.UInt16ToBytes(firstCode), 0, 2);

            ushort count = (ushort)(characters[characters.Count - 1] - characters[0] + 1);
            stream.Write(BinaryUtility.UInt16ToBytes(count), 0, 2);

            ushort curIndex = 0;
            for (int i = 0; i < count; ++i)
            {
                if (i + firstCode == characters[curIndex])
                {
                    stream.Write(BinaryUtility.UInt16ToBytes(map.GetGlyfIndex(characters[curIndex])), 0, 2);
                    ++curIndex;
                }
                else
                    stream.Write(BinaryUtility.UInt16ToBytes(0), 0, 2);
            }

            byte[] data = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(data, 0, data.Length);

            return data;
        }

        public override void Read(Reader reader)
        {
            base.Read(reader);

            _firstCode = (UInt16)reader.ReadUint16();
            _entryCount = (UInt16)reader.ReadUint16();

            for (int i = 0; i < _entryCount; ++i)
                _glyphIndexArray.Add((UInt16)reader.ReadUint16());
        }

        public override ushort GetGlyphIndex(ushort characterCode)
        {
            ushort entryIndex = getEntryIndexByFirstCode(characterCode);
            if (entryIndex >= _entryCount)
                return 0;

            return _glyphIndexArray[entryIndex];
        }
       
        private ushort getEntryIndexByFirstCode(ushort characterCode)
        {
            if (characterCode >= _firstCode)
                return (ushort)(characterCode - _firstCode);
            else
                return (ushort)((ushort.MaxValue - _firstCode) + characterCode);
            
        }
    }
}
