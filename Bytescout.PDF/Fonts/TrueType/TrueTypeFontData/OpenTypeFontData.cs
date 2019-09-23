using System.Collections.Generic;

namespace Bytescout.PDF
{
    internal class OpenTypeFontData: TrueTypeFontData
    {
        public OpenTypeFontData(byte[] buffer)
            : base(buffer, 0)
        {
        }

        public override ushort[] GetUsedGlyphs(char ch)
        {
            ushort index = GetGlyphIndex(ch);
            if (index != 0)
                return new ushort[] { index };
            return null;
        }

        public override void Write(System.IO.Stream stream, ushort[] glyfIndexes, FontMap map)
        {
            string[] tags = { "CFF ", "head", "hhea", "hmtx", "maxp", "name", "post" };
            int count = tags.Length;
            writeHeader(stream, 1330926671);

            byte[][] bytes = new byte[count][];
            long offset = 12 + 16 * count;
            int curTable = 0;

            for (int i = 0; i < tags.Length; ++i)
            {
                if (Tables.ContainsKey(tags[i]))
                {
                    OffsetTable table = Tables[tags[i]];
                    byte[] buffer;
                    if (table.Table is CMapTable)
                        buffer = (table.Table as CMapTable).GetData(map);
                    else
                        buffer = table.Table.GetData(glyfIndexes);

                    bytes[curTable] = buffer;
                    table.Length = buffer.Length;
                    table.Checksum = calculateCheckSum(buffer);
                    table.Offset = offset;

                    table.Write(stream);

                    offset += buffer.Length;
                    ++curTable;
                }
            }

            for (int i = 0; i < bytes.Length; ++i)
            {
                byte[] buf = bytes[i];
                stream.Write(buf, 0, buf.Length);
            }
        }

        protected override void loadRequiredTables(List<OffsetTable> offsetsTables, Reader reader, long parallax)
        {
            string[] tags = {"CFF ", "head", "hhea", "maxp", "cmap", "hmtx", "name", "post" };
            for (int i = 0; i < tags.Length; ++i)
            {
                int index = findTable(offsetsTables, tags[i]);
                if (index == -1)
                    throw new PDFWrongFontFileException();

                OffsetTable offsetTable = offsetsTables[index];
                switch (tags[i])
                {
                    case "CFF ":
                        offsetTable.Table = new CFFTable();
                        break;
                    case "head":
                        offsetTable.Table = new HeadTable();
                        break;
                    case "hhea":
                        offsetTable.Table = new HHeaTable();
                        break;
                    case "maxp":
                        offsetTable.Table = new MaxpTable();
                        break;
                    case "cmap":
                        offsetTable.Table = new CMapTable();
                        break;
                    case "hmtx":
                        ushort numGlyph = (Tables["maxp"].Table as MaxpTable).NumGlyphs;
                        ushort numOfLongHorMetrics = (Tables["hhea"].Table as HHeaTable).NumOfLongHorMetrics;
                        offsetTable.Table = new HMtxTable(numOfLongHorMetrics, numGlyph);
                        break;
                    case "name":
                        offsetTable.Table = new NameTable();
                        break;
                    case "post":
                        offsetTable.Table = new PostTable();
                        break;
                }

                offsetTable.Table.Read(reader, (int)(offsetTable.Offset - parallax), (int)offsetTable.Length);
                Tables.Add(tags[i], offsetTable);
            }
        }
    }
}
