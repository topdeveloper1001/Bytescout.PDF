namespace Bytescout.PDF
{
    internal abstract class TTFTable
    {
        public abstract void Read(Reader reader, int offset, int length);

        public abstract byte[] GetData(ushort[] glyfIndexes);
    }
}
