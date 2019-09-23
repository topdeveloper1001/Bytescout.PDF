using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class JPXBitBuffer
    {
        internal static bool GetBits(Stream stream, int nBits, ref int bits)
        {
            int c;

            while (m_bitBufLength < nBits)
            {
                if (m_byteCount == 0 || (c = stream.ReadByte()) == -1)
                    return false;
                --m_byteCount;
                if (m_bitBufSkip)
                {
                    m_bitBuffer = (m_bitBuffer << 7) | (byte)(c & 0x7f);
                    m_bitBufLength += 7;
                }
                else
                {
                    m_bitBuffer = (m_bitBuffer << 8) | (byte)(c & 0xff);
                    m_bitBufLength += 8;
                }

                m_bitBufSkip = c == 0xff;
            }

            bits = (int)((m_bitBuffer >> (m_bitBufLength - nBits)) & (int)((1 << nBits) - 1));
            m_bitBufLength -= nBits;
            return true;
        }

        internal static void StartBitBuffer(int byteCount)
        {
            m_bitBufLength = 0;
            m_bitBufSkip = false;
            m_byteCount = byteCount;
        }

        internal static int FinishBitBuffer(Stream stream)
        {
            if (m_bitBufSkip)
            {
                stream.ReadByte();
                --m_byteCount;
            }
            return m_byteCount;
        }

        private static uint m_bitBuffer;
        private static int m_bitBufLength;
        private static bool m_bitBufSkip;
        private static int m_byteCount;
    }
}
