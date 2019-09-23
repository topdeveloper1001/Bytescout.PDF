namespace Bytescout.PDF
{
    internal class CFFTable : TTFTable
    {
	    private byte[] _data;

	    public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            reader.Position = offset;
            _data = new byte[length];
            reader.Read(_data);
        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            return _data;
        }
    }
}
