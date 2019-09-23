using System.Collections.Generic;

namespace Bytescout.PDF
{
    internal class HMtxTable : TTFTable
    {
	    private struct LongHorMetric
	    {
		    public ushort AdvanceWidth;
		    public short Lsb;
	    }

	    private List<LongHorMetric> _hMetrics = new List<LongHorMetric>();
	    private List<short> _leftSideBearing = new List<short>();
	    private int _numberOfHMetrics;
	    private int _numGlyphs;

	    public HMtxTable(int numberOfHMetrics, int numGlyphs)
        {
            _numberOfHMetrics = numberOfHMetrics;
            _numGlyphs = numGlyphs;
        }

        public int GetGlyphAdvanceWidth(int glyphIndex)
        {
            if (_hMetrics.Count == 0)
                return 0;

            if (glyphIndex >= _numGlyphs)
                return _hMetrics[0].AdvanceWidth;

            if (_hMetrics.Count > glyphIndex)
                return _hMetrics[glyphIndex].AdvanceWidth;

            return _hMetrics[_hMetrics.Count - 1].AdvanceWidth;
        }

        public override void Read(Reader reader, int offset, int length)
        {
            if (length < 0 || offset <= 0 || offset >= reader.Length)
                throw new PDFWrongFontFileException();

            reader.Position = offset;

            for (int i = 0; i < _numberOfHMetrics; ++i)
            {
                LongHorMetric tmp = new LongHorMetric();
                tmp.AdvanceWidth = (ushort)reader.ReadUint16();
                tmp.Lsb = (short)reader.ReadUint16();
                _hMetrics.Add(tmp);
            }

            int count = _numGlyphs - _numberOfHMetrics;
            for (int i = 0; i < count; ++i)
                _leftSideBearing.Add((short)reader.ReadUint16());
        }

        public override byte[] GetData(ushort[] glyfIndexes)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            
            int hMetricsCount = calculateNumOfLongHorMetrics(glyfIndexes);
            for (int i = 0; i < hMetricsCount; ++i)
            {
                if (i >= _hMetrics.Count)
                    break;

                LongHorMetric tmp = _hMetrics[i];
                ms.Write(BinaryUtility.UInt16ToBytes((ushort)tmp.AdvanceWidth), 0, 2);
                ms.Write(BinaryUtility.UInt16ToBytes((ushort)tmp.Lsb), 0, 2);
            }

            int glyfsCount = glyfIndexes[glyfIndexes.Length - 1] + 1;
            int leftSideBearingCount = glyfsCount - hMetricsCount;
            if (leftSideBearingCount > 0 && _leftSideBearing.Count > 0)
            {
                int start = calculateStartLeftSideBearing(glyfIndexes);
                for (int i = 0; i < leftSideBearingCount; ++i)
                {
                    if (i >= _leftSideBearing.Count + start)
                        break;
                    ms.Write(BinaryUtility.UInt16ToBytes((ushort)_leftSideBearing[start + i]), 0, 2);
                }
            }

            byte[] tempFontData = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(tempFontData, 0, tempFontData.Length);
            return tempFontData;
        }

        private ushort calculateNumOfLongHorMetrics(ushort[] glyfIndexes)
        {
            for (int i = glyfIndexes.Length - 1; i >= 0; --i)
            {
                if (glyfIndexes[i] < _numberOfHMetrics)
                    return (ushort)(glyfIndexes[i] + 1);
            }

            return 0;
        }

        private int calculateStartLeftSideBearing(ushort[] glyfIndexes)
        {
            for (int i = 0; i < glyfIndexes.Length; ++i)
            {
                if (glyfIndexes[i] >= _numberOfHMetrics)
                    return glyfIndexes[i] - _numberOfHMetrics;
            }

            return 0;
        }
    }
}
