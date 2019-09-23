using System;

namespace Bytescout.PDF
{
    internal class CMapEncodingRecord
    {
	    private UInt16 _platformID;
	    private UInt16 _platformSpecificID;
	    private UInt32 _offset;
	    private CMapBase _cmap;
	    public UInt16 PlatformID { get { return _platformID; } }

        public UInt16 EncodingID { get { return _platformSpecificID; } }

        public void Read(Reader reader, long tableStartPos)
        {
            _platformID = (UInt16)reader.ReadUint16();
            _platformSpecificID = (UInt16)reader.ReadUint16();
            _offset = (UInt32)reader.ReadUint32();

            int oldPos = reader.Position;
            reader.Position = (int)(tableStartPos + _offset);

            _cmap = new CMapBase();
            int format = (UInt16)reader.ReadUint16();
            reader.Position -= 2;

            switch (format)
            {
                case 0:
                    _cmap = new CMapMapFormat0();
                    break;
                case 4:
                    _cmap = new CMapMapFormat4();
                    break;
                case 6:
                    _cmap = new CMapMapFormat6();
                    break;
            }

            if(_cmap!=null)
                _cmap.Read(reader);

            reader.Position = oldPos;
        }

        public ushort GetGlyphIndex(ushort ch)
        {
            if (_cmap == null)
                return 0;

            return _cmap.GetGlyphIndex((ushort)ch);
        }
    }
}
