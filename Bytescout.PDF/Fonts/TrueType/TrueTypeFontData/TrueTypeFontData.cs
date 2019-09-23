using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Bytescout.PDF
{
    internal class TrueTypeFontData
    {
	    private readonly Dictionary<string, OffsetTable> _tables = new Dictionary<string, OffsetTable>();

	    public string FontName
        {
            get { return (_tables["name"].Table as NameTable).FontName; }
        }
        
        public bool Bold
        {
            get
            {
                return (_tables["head"].Table as HeadTable).Bold;
            }
        }
        
        public System.Drawing.Rectangle FontBBox
        {
            get { return mapSize((_tables["head"].Table as HeadTable).GetBBox()); }
        }

        public int ItalicAngle
        {
            get { return (short)((_tables["post"].Table as PostTable).ItalicAngle.X); }
        }

        public int Ascent
        {
            get { return mapSize((_tables["hhea"].Table as HHeaTable).Ascent); }
        }

        public int Descent
        {
            get { return mapSize((_tables["hhea"].Table as HHeaTable).Descent); }
        }

        public int MissingWidth
        {
            get { return mapSize((_tables["hmtx"].Table as HMtxTable).GetGlyphAdvanceWidth(0)); }
        }

        protected Dictionary<string, OffsetTable> Tables
        {
            get { return _tables; }
        }

	    public TrueTypeFontData(byte[] buffer, long parallax)
	    {
		    initialize(new Reader(buffer), parallax);
	    }

	    public ushort GetGlyphIndex(char ch)
        {
            return (_tables["cmap"].Table as CMapTable).GetGlyphIndex(ch);
        }

        public int GetCharWidth(char ch)
        {
            int index = GetGlyphIndex(ch);
            if (index == 0)
                return 0;

            return mapSize((_tables["hmtx"].Table as HMtxTable).GetGlyphAdvanceWidth(index));
        }

        public int GetWidthOfGlyph(int glyfIndex)
        {
            return mapSize((_tables["hmtx"].Table as HMtxTable).GetGlyphAdvanceWidth(glyfIndex));
        }

        public virtual ushort[] GetUsedGlyphs(char ch)
        {
            ushort index = GetGlyphIndex(ch);
            if (index == 0)
                return null;
            return (_tables["glyf"].Table as GlyphTable).GetUsedGlyfIndexes(index);
        }

        public virtual void Write(Stream stream, ushort[] glyfIndexes, FontMap map)
        {
            string[] tags = { "cmap", "glyf", "head", "hhea", "hmtx", "loca", "maxp", "name", "post" };
            int count = tags.Length;
            writeHeader(stream, 65536);

            byte[][] bytes = new byte[count][];
            long offset = 12 + 16 * count;
            int curTable = 0;

            for (int i = 0; i < tags.Length; ++i)
            {
                if (_tables.ContainsKey(tags[i]))
                {
                    OffsetTable table = _tables[tags[i]];
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

        protected uint calculateCheckSum(byte[] buf)
        {
            Reader reader = new Reader(buf);
            int count = buf.Length / 4;

            uint sum = 0;
            for (int i = 0; i < count; ++i)
                sum += (uint)reader.ReadUint32();

            return sum;
        }

        protected virtual void loadRequiredTables(List<OffsetTable> offsetsTables, Reader reader, long parallax)
        {
            string[] tags = {"head","hhea", "maxp", "loca", "cmap", "glyf", "hmtx", "name", "post" };
            for (int i = 0; i < tags.Length; ++i)
            {
                int index = findTable(offsetsTables, tags[i]);
                if (index == -1)
                    throw new PDFWrongFontFileException();

                OffsetTable offsetTable = offsetsTables[index];
                switch (tags[i])
                {
                    case "head":
                        offsetTable.Table = new HeadTable();
                        break;
                    case "hhea":
                        offsetTable.Table = new HHeaTable();
                        break;
                    case "maxp":
                        offsetTable.Table = new MaxpTable();
                        break;
                    case "loca":
                        short indexToLocFormat = (_tables["head"].Table as HeadTable).IndexToLocFormat;
                        ushort numGlyphs = (_tables["maxp"].Table as MaxpTable).NumGlyphs;
                        offsetTable.Table = new LocaTable(indexToLocFormat, numGlyphs);
                        break;
                    case "cmap":
                        offsetTable.Table = new CMapTable();
                        break;
                    case "glyf":
                        offsetTable.Table = new GlyphTable((_tables["loca"].Table as LocaTable).Offsets);
                        break;
                    case "hmtx":
                        ushort numGlyph = (_tables["maxp"].Table as MaxpTable).NumGlyphs;
                        ushort numOfLongHorMetrics = (_tables["hhea"].Table as HHeaTable).NumOfLongHorMetrics;
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
                _tables.Add(tags[i], offsetTable);
            }
        }

        protected int findTable(List<OffsetTable> offsetsTables, string tag)
        {
            for (int i = 0; i < offsetsTables.Count; ++i)
            {
                if (offsetsTables[i].Tag == tag)
                    return i;
            }
            return -1;
        }

        protected void writeHeader(Stream stream, uint tag)
        {
            stream.Write(BinaryUtility.UInt32ToBytes(tag), 0, 4);

            int numTables = _tables.Count;
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)numTables), 0, 2);

            uint maxPow = maxPowerOf2(numTables);
            uint searchRange = maxPow * 16;
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)searchRange), 0, 2);

            int entrySelector = (int)Math.Log(maxPow, 2);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)entrySelector), 0, 2);

            int rangeShift = (int)(numTables * 16 - searchRange);
            stream.Write(BinaryUtility.UInt16ToBytes((ushort)rangeShift), 0, 2);
        }

        private void initialize(Reader reader, long parallax)
        {
            reader.Position = 0;
            uint tag = (uint)reader.ReadUint32();

            if (tag == 1953784678) //ttc tag
            {
                Point version = new Point();
                version.X = reader.ReadUint16();
                version.Y = reader.ReadUint16();

                uint numFonts = (uint)reader.ReadUint32();
                uint offsetTable = (uint)reader.ReadUint32();

                reader.Position = (int)(offsetTable + 4);
            }

            ushort numTables = (ushort)reader.ReadUint16();
            ushort searchRange = (ushort)reader.ReadUint16();
            ushort entrySelector = (ushort)reader.ReadUint16();
            ushort rangeShift = (ushort)reader.ReadUint16();
            List<OffsetTable> offsetsTables = new List<OffsetTable>();

            for (int i = 0; i < numTables; ++i)
            {
                OffsetTable table = new OffsetTable();
                table.Read(reader);
                offsetsTables.Add(table);
            }

            loadRequiredTables(offsetsTables, reader, parallax);
        }

        private uint maxPowerOf2(int value)
        {
            uint pow = 2;
            while (pow * 2 > value)
                pow *= 2;
            return pow;
        }

        private int mapSize(int value)
        {
            return (value * 1000 / (_tables["head"].Table as HeadTable).UnitsPerEm);
        }

        private System.Drawing.Rectangle mapSize(System.Drawing.Rectangle rect)
        {
            rect.X = mapSize(rect.X);
            rect.Y = mapSize(rect.Y);
            rect.Width = mapSize(rect.Width);
            rect.Height = mapSize(rect.Height);
            return rect;
        }
    }
}
