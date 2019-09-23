using System;

namespace Bytescout.PDF
{
    internal class CMapBase
    {
	    private UInt16 _format;
	    private UInt16 _length;
	    private UInt16 _language;

	    public virtual void Read(Reader reader)
        {
            _format = (UInt16)reader.ReadUint16();
            _length = (UInt16)reader.ReadUint16();
            _language = (UInt16)reader.ReadUint16();
        }

        public virtual ushort GetGlyphIndex(ushort characterCode)
        {
            return 0;
        }
    }
}
