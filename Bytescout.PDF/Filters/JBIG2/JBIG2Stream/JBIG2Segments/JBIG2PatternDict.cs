using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JBIG2PatternDict : JBIG2Segment
    {
        public JBIG2PatternDict(uint segNumA, int sizeA) :
            base(segNumA)
        {
            m_size = sizeA;
            m_bitmaps = new JBIG2Bitmap[m_size];
        }

        public override JBIG2SegmentType Type { get { return JBIG2SegmentType.jbig2SegPatternDict; } }

        public int Size { get { return m_size; } }

        public void SetBitmap(int idx, JBIG2Bitmap bitmap) { m_bitmaps[idx] = bitmap; }

        public JBIG2Bitmap GetBitmap(int idx) { return m_bitmaps[idx]; }

        private int m_size;
        private JBIG2Bitmap[] m_bitmaps;
    }
}
