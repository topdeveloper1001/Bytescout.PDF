using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    enum JBIG2SegmentType
    {
        jbig2SegBitmap,
        jbig2SegSymbolDict,
        jbig2SegPatternDict,
        jbig2SegCodeTable,
        jbig2Seg
    }

    internal class JBIG2Segment
    {
        public JBIG2Segment(uint segNumA) { m_segNum = segNumA; }

        public uint SegNum
        {
            get
            {
                return m_segNum;
            }
            set
            {
                m_segNum = value;
            }
        }

        public virtual JBIG2SegmentType Type { get { return JBIG2SegmentType.jbig2Seg; } }

        private uint m_segNum;
    }
}
