using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JBIG2SymbolDict : JBIG2Segment
    {
        public JBIG2SymbolDict(uint segNumA, int sizeA) :
            base(segNumA)
        {
            m_size = sizeA;
            m_bitmaps = new JBIG2Bitmap[m_size];
            m_genericRegionStats = null;
            m_refinementRegionStats = null;
        }

        public override JBIG2SegmentType Type { get { return JBIG2SegmentType.jbig2SegSymbolDict; } }

        public int Size { get { return m_size; } }

        public void SetBitmap(uint idx, JBIG2Bitmap bitmap) { m_bitmaps[idx] = bitmap; }

        public JBIG2Bitmap GetBitmap(uint idx) { return m_bitmaps[idx]; }

        public void SetGenericRegionStats(JArithmeticDecoderStats stats) { m_genericRegionStats = stats; }

        public void SetRefinementRegionStats(JArithmeticDecoderStats stats) { m_refinementRegionStats = stats; }

        public JArithmeticDecoderStats GetGenericRegionStats() { return m_genericRegionStats; }

        public JArithmeticDecoderStats GetRefinementRegionStats() { return m_refinementRegionStats; }

        private int m_size;
        private JBIG2Bitmap[] m_bitmaps;
        private JArithmeticDecoderStats m_genericRegionStats;
        private JArithmeticDecoderStats m_refinementRegionStats;
    }
}
