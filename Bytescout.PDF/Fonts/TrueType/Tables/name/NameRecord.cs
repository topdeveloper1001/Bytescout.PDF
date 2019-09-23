using System;

namespace Bytescout.PDF
{
    internal class NameRecord
    {
	    private UInt16 _platformID;
	    private UInt16 _platformSpecificID;
	    private UInt16 _languageID;
	    private UInt16 _nameID;
	    private UInt16 _length;
	    private UInt16 _offset;
	    public int Length { get { return _length; } }

        public int Offset { get { return _offset; } }

        public ushort PlatformID { get { return _platformID; } }

        public ushort PlatformSpecificID { get { return _platformSpecificID; } }

        public ushort LanguageID { get { return _languageID; } }

        public ushort NameID { get { return _nameID; } }

        public static NameRecord Read(Reader reader)
        {
            NameRecord nameRecord = new NameRecord();

            nameRecord._platformID = (UInt16)reader.ReadUint16();
            nameRecord._platformSpecificID = (UInt16)reader.ReadUint16();
            nameRecord._languageID = (UInt16)reader.ReadUint16();
            nameRecord._nameID = (UInt16)reader.ReadUint16();
            nameRecord._length = (UInt16)reader.ReadUint16();
            nameRecord._offset = (UInt16)reader.ReadUint16();

            return nameRecord;
        }
    }
}
