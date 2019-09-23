using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JBIG2CodeTable : JBIG2Segment
    {
        public JBIG2CodeTable(uint segNumA, JBIG2HuffmanTable[] tableA)
            : base(segNumA)
        {
            m_table = tableA;
        }

        public override JBIG2SegmentType Type { get { return JBIG2SegmentType.jbig2SegCodeTable; } }

        public JBIG2HuffmanTable[] GetHuffTable() { return m_table; }

        private JBIG2HuffmanTable[] m_table;
    }
}
