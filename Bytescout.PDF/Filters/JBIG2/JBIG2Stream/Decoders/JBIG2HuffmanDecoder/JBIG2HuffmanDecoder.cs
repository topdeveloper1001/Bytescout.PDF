using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class JBIG2HuffmanDecoder
    {
        public JBIG2HuffmanDecoder()
        {
            m_str = null;
            Reset();
        }

        public void SetStream(Stream strA) { m_str = strA; }

        public void Reset()
        {
            m_buf = 0;
            m_bufLen = 0;
        }

        // Returns false for OOB, otherwise sets *<x> and returns true.
        public bool DecodeInt(ref int x, JBIG2HuffmanTable[] table)
        {
            int i, len, prefix;

            i = 0;
            len = 0;
            prefix = 0;
            while (table[i].rangeLen != JBIG2HuffmanTables.jbig2HuffmanEOT)
            {
                while (len < table[i].prefixLen)
                {
                    prefix = (prefix << 1) | (int)ReadBit();
                    ++len;
                }
                if (prefix == table[i].prefix)
                {
                    if (table[i].rangeLen == JBIG2HuffmanTables.jbig2HuffmanOOB)
                    {
                        return false;
                    }
                    if (table[i].rangeLen == JBIG2HuffmanTables.jbig2HuffmanLOW)
                    {
                        x = (int)(table[i].val - ReadBits(32));
                    }
                    else if (table[i].rangeLen > 0)
                    {
                        x = (int)(table[i].val + ReadBits(table[i].rangeLen));
                    }
                    else
                    {
                        x = table[i].val;
                    }
                    return true;
                }
                ++i;
            }
            return false;
        }

        public uint ReadBits(uint n)
        {

            uint x, mask, nLeft;

            mask = (n == 32) ? 0xffffffff : (uint)((1 << (int) n) - 1);
            if (m_bufLen >= n)
            {
                x = (m_buf >> (int) (m_bufLen - n)) & mask;
                m_bufLen -= n;
            }
            else
            {
                x = (uint)(m_buf & ((1 << (int)m_bufLen) - 1));
                nLeft = n - m_bufLen;
                m_bufLen = 0;
                while (nLeft >= 8)
                {
                    x = (x << 8) | ((uint)m_str.ReadByte() & 0xff);
                    nLeft -= 8;
                }
                if (nLeft > 0)
                {
                    m_buf = (uint)m_str.ReadByte();
                    m_bufLen = 8 - nLeft;
                    x = (uint)((x << (int)nLeft) | ((m_buf >> (int)m_bufLen) & ((1 << (int)nLeft) - 1)));
                }
            }
            return x;

        }
        public uint ReadBit()
        {
            if (m_bufLen == 0)
            {
                m_buf = (uint)m_str.ReadByte();
                m_bufLen = 8;
            }
            --m_bufLen;
            return (m_buf >> (int)m_bufLen) & 1;
        }

        // Sort the table by prefix length and assign prefix values.
        public void BuildTable(JBIG2HuffmanTable[] table, uint len)
        {
            uint i, j, k, prefix;
            JBIG2HuffmanTable tab;

            // stable selection sort:
            // - entries with prefixLen > 0, in ascending prefixLen order
            // - entry with prefixLen = 0, rangeLen = EOT
            // - all other entries with prefixLen = 0
            // (on entry, table[len] has prefixLen = 0, rangeLen = EOT)
            for (i = 0; i < len; ++i)
            {
                for (j = i; j < len && table[j].prefixLen == 0; ++j) ;
                if (j == len)
                {
                    break;
                }
                for (k = j + 1; k < len; ++k)
                {
                    if (table[k].prefixLen > 0 &&
                    table[k].prefixLen < table[j].prefixLen)
                    {
                        j = k;
                    }
                }
                if (j != i)
                {
                    tab = table[j];
                    for (k = j; k > i; --k)
                    {
                        table[k] = table[k - 1];
                    }
                    table[i] = tab;
                }
            }
            table[i] = table[len];

            // assign prefixes
            i = 0;
            prefix = 0;
            table[i++].prefix = prefix++;
            for (; table[i].rangeLen != JBIG2HuffmanTables.jbig2HuffmanEOT; ++i)
            {
                prefix <<= (int)(table[i].prefixLen - table[i - 1].prefixLen);
                table[i].prefix = prefix++;
            }
        }

        private Stream m_str;
        private uint m_buf;
        private uint m_bufLen;
    }
}

