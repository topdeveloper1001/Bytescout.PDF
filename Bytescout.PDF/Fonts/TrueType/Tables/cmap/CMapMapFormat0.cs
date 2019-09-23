namespace Bytescout.PDF
{
    internal class CMapMapFormat0 : CMapBase
    {
	    private readonly byte[] _glyphIndexArray = new byte[256];

	    public override void Read(Reader reader)
        {
            base.Read(reader);

            for (int i = 0; i < _glyphIndexArray.Length; ++i)
                _glyphIndexArray[i] = (byte)reader.ReadByte();
        }

        public override ushort GetGlyphIndex(ushort characterCode)
        {
            if (!isValidCharacterCode(characterCode))
                return 0;
            
            return _glyphIndexArray[characterCode];
        }

        private bool isValidCharacterCode(ushort characterCode)
        {
            return (characterCode < _glyphIndexArray.Length);
        }
    }
}
