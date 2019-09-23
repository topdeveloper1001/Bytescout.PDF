using System;
using System.Collections.Generic;
using System.IO;

namespace Bytescout.PDF
{
    internal class CMapTable : TTFTable
    {
	    private UInt16 _version;
	    private UInt16 _numberSubtables;
	    private readonly List<CMapEncodingRecord> _records = new List<CMapEncodingRecord>();

	    public CMapEncodingRecord GetEncodingRecord(ushort platformID, ushort encodingID)
        {
            for (int i = 0; i < _records.Count; ++i)
            {
                if (_records[i].PlatformID == platformID && _records[i].EncodingID == encodingID)
                    return _records[i];
            }

            return null;
        }

        public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            reader.Position = offset;
            
            _version = (UInt16)reader.ReadUint16();
            _numberSubtables = (UInt16)reader.ReadUint16();

            for (int i = 0; i < _numberSubtables; ++i)
            {
                CMapEncodingRecord rec = new CMapEncodingRecord();
                rec.Read(reader, offset);
                _records.Add(rec);
            }
        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            return new byte[0];
        }

        public byte[] GetData(FontMap map)
        {
            MemoryStream stream = new MemoryStream();
            byte[] cmapData = CMapMapFormat4.CreateData(map);

            stream.Write(BinaryUtility.UInt16ToBytes(0), 0, 2); //version
            stream.Write(BinaryUtility.UInt16ToBytes(1), 0, 2); //number subtables
            stream.Write(BinaryUtility.UInt16ToBytes(3), 0, 2); //platformID
            stream.Write(BinaryUtility.UInt16ToBytes(1), 0, 2); //platformSpecificID
            stream.Write(BinaryUtility.UInt32ToBytes(12), 0, 4); //offset
            stream.Write(BinaryUtility.UInt16ToBytes(4), 0, 2); //format
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)cmapData.Length), 0, 2); //length
            stream.Write(BinaryUtility.UInt16ToBytes(0), 0, 2); //_language

            stream.Write(cmapData, 0, cmapData.Length);

            byte[] fontData = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(fontData, 0, fontData.Length);

            return fontData;
        }

        public ushort GetGlyphIndex(char ch)
        {
            if (ch < 32)
            {
                CMapEncodingRecord byteEncodingRecord = GetEncodingRecord(1, 0);
                if (byteEncodingRecord != null)
                    return byteEncodingRecord.GetGlyphIndex(ch);
            }

            CMapEncodingRecord unicodeEncodingRecord = GetEncodingRecord(3, 1);
            if (unicodeEncodingRecord != null)
            {
                ushort result = unicodeEncodingRecord.GetGlyphIndex(ch);
                if (result != 0)
                    return result;
            }

            for (int i = 0; i < _records.Count; ++i)
            {
                CMapEncodingRecord encRecord = _records[i];
                ushort result = encRecord.GetGlyphIndex(ch);
                if (result != 0)
                    return result;
            }

            if (ch < 0xf000)
            {
                // Some fonts contains glyphs in Private Use Area.
                // Try to load from it.
                if (0xf000 + ch <= ushort.MaxValue)
                    return GetGlyphIndex((char)(0xf000 + ch));
            }

            return 0;
        }
    }
}
