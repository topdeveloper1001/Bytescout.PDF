using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class NameTable : TTFTable
    {
	    List<NameRecord> _nameRecords = new List<NameRecord>();
	    private string _fontName = "";
	    private byte[] _data;
	    public string FontName { get { return _fontName; } }

        public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            reader.Position = offset;

            UInt16 format = (UInt16)reader.ReadUint16();
            UInt16 count = (UInt16)reader.ReadUint16();
            UInt16 stringOffset = (UInt16)reader.ReadUint16();

            _nameRecords = new List<NameRecord>();
            for (int i = 0; i < count; ++i)
                _nameRecords.Add(NameRecord.Read(reader));

            loadName(reader, reader.Position);
            _nameRecords.Clear();
            _nameRecords = null;

            _data = new byte[length];
            reader.Position = offset;
            reader.Read(_data);
        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            return _data;
        }
        
        private void loadName(Reader reader, long startPos)
        {
            string fontName = getFontName(3, 1, 0x409, reader, startPos);
            if (fontName == "")
            {
                fontName = getFontName(3, 0, 0x409, reader, startPos);
                if (fontName == "")
                {
                    fontName = getFontName(1, 0, 0, reader, startPos);
                    if (fontName == "")
                        fontName = getFontName(2, 2, 0, reader, startPos);
                }
            }

            if (fontName != "")
                _fontName = fontName;
            else
                _fontName = "fnt.tmp";
        }

        private string getFontName(ushort platformID, ushort encodingID, ushort languageID, Reader reader, long startPos)
        {
            NameRecord recordFullName = getNameRecord(platformID, encodingID, languageID, 4);
            if (recordFullName != null)
                return getStringData(recordFullName, reader, startPos);

            NameRecord postScriptName = getNameRecord(platformID, encodingID, languageID, 6);
            if (postScriptName != null)
                return getStringData(postScriptName, reader, startPos);

            NameRecord recordFamilyName = getNameRecord(platformID, encodingID, languageID, 1);
            if (recordFamilyName != null)
            {
                NameRecord recordSubfamilyName = getNameRecord(platformID, encodingID, languageID, 2);
                string strSubfamily = "";

                if (recordSubfamilyName != null)
                    strSubfamily = getStringData(recordSubfamilyName, reader, startPos);

                if (strSubfamily == "Regular")
                    return getStringData(recordFamilyName, reader, startPos);
                else
                    return getStringData(recordFamilyName, reader, startPos) + strSubfamily;
            }

            return "";
        }

        private string getStringData(NameRecord record, Reader reader, long startPos)
        {
            int len = record.Length;
            int off = record.Offset;
            StringBuilder result = new StringBuilder();

            byte[] buf = new byte[len];
            reader.Position = (int)(startPos + off);

            reader.Read(buf);
            for (int j = 0; j < buf.Length; ++j)
            {
                if (buf[j] != 0)
                    result.Append((char)buf[j]);
            }

            return result.ToString();
        }

        private NameRecord getNameRecord(ushort platformID, ushort encodingID, ushort languageID, ushort nameID)
        {
            for (int i = 0; i < _nameRecords.Count; ++i)
            {
                NameRecord record = _nameRecords[i];
                if (record.PlatformID == platformID && record.PlatformSpecificID == encodingID &&
                    record.LanguageID == languageID && record.NameID == nameID)
                    return record;
            }

            return null;
        }
    }
}
