using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class JArithmeticDecoderStats
    {
        public JArithmeticDecoderStats(int contextSizeA)
        {
            m_contextSize = contextSizeA;
            m_cxTab = new byte[m_contextSize];
            Reset();
        }

        public JArithmeticDecoderStats Copy()
        {
            JArithmeticDecoderStats stats;

            stats = new JArithmeticDecoderStats(m_contextSize);
            Array.Copy(m_cxTab, stats.m_cxTab, m_contextSize);
            return stats;
        }

        public void Reset()
        {
            Array.Clear(m_cxTab, 0, m_contextSize);
        }

        public int ContextSize { get { return m_contextSize; } }

        public void CopyFrom(JArithmeticDecoderStats stats)
        {
            Array.Copy(stats.m_cxTab, m_cxTab, m_contextSize);
        }

        public void SetEntry(int cx, int i, int mps)
        {
            m_cxTab[cx] = (byte)((i << 1) + mps);
        }

        public byte this[uint i]
        {
            get
            {
                return m_cxTab[i];
            }
            set
            {
                m_cxTab[i] = value;
            }
        }

        private byte[] m_cxTab;		// cxTab[cx] = (i[cx] << 1) + mps[cx]
        private int m_contextSize;
    }
}
