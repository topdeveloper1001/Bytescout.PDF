namespace Bytescout.PDF
{
    internal static class TrueTypeFontFile
    {
        internal static bool IsTrueTypeFont(byte[] data)
        {
            uint sfntTag = 65536;
            uint ttcTag = 1953784678;

            Reader reader = new Reader(data);
            uint tag = (uint)reader.ReadUint32();

            if (tag == sfntTag || tag == ttcTag)
                return true;

            return false;
        }

        internal static bool IsOpenTypeFont(byte[] data)
        {
            uint cffTag = 1330926671;
            Reader reader = new Reader(data);
            uint tag = (uint)reader.ReadUint32();

            if (tag == cffTag)
                return true;

            return false;
        }

        internal static CMapTable GetCMAP(Reader reader)
        {
            reader.Position = 4;
            ushort numTables = (ushort)reader.ReadUint16();
            ushort searchRange = (ushort)reader.ReadUint16();
            ushort entrySelector = (ushort)reader.ReadUint16();
            ushort rangeShift = (ushort)reader.ReadUint16();

            int offset = -1;
            int length = -1;
            for (int i = 0; i < numTables; ++i)
            {
                OffsetTable table = new OffsetTable();
                table.Read(reader);
                if (table.Tag == "cmap")
                {
                    offset = (int)table.Offset;
                    length = (int)table.Length;
                    break;
                }
            }

            CMapTable cmap = new CMapTable();
            cmap.Read(reader, offset, length);
            return cmap;
        }
    }
}
