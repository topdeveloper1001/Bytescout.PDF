using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class JArithmeticDecoder
    {
        public JArithmeticDecoder()
        {
            m_str = null;
            m_dataLen = 0;
            m_limitStream = false;
        }

        public void SetStream(Stream strA)
        { m_str = strA; m_dataLen = 0; m_limitStream = false; }

        public void SetStream(Stream strA, int dataLenA)
        { m_str = strA; m_dataLen = dataLenA; m_limitStream = true; }

        // Start decoding on m_a new stream.  This fills the byte buffers and
        // runs INITDEC.
        public void Start()
        {
            m_buf0 = readByte();
            m_buf1 = readByte();

            // INITDEC
            m_c = (m_buf0 ^ 0xff) << 16;
            byteIn();
            m_c <<= 7;
            m_ct -= 7;


            m_a = 0x80000000;
        }

        // Restart decoding on an interrupted stream.  This refills the
        // buffers if needed, but does not run INITDEC.  (This is used in
        // JPEG 2000 streams when codeblock data is split across multiple
        // packets/layers.)
        public void Restart(int dataLenA)
        {
            int oldDataLen;

            oldDataLen = m_dataLen;
            m_dataLen = dataLenA;
            if (oldDataLen == -1)
            {
                m_buf1 = readByte();
            }
            else if (oldDataLen <= -2)
            {
                m_buf0 = readByte();
                m_buf1 = readByte();
            }
        }

        // Read any leftover data in the stream.
        public void Cleanup()
        {
            if (m_limitStream)
            {
                while (m_dataLen > 0)
                {
                    m_buf0 = m_buf1;
                    m_buf1 = readByte();
                }
            }
        }

        // Decode one bit.
        public int DecodeBit(uint context, JArithmeticDecoderStats stats)
        {
            int bit;
            uint qe;
            int iCX, mpsCX;
            
            iCX = stats[context] >> 1;
            mpsCX = stats[context] & 1;
            qe = qeTab[iCX];
            m_a -= qe;
            if (m_c < m_a)
            {
                if ((m_a & 0x80000000) != 0)
                {
                    bit = mpsCX;
                }
                else
                {
                    // MPS_EXCHANGE
                    if (m_a < qe)
                    {
                        bit = 1 - mpsCX;
                        if (switchTab[iCX] != 0)
                        {
                            stats[context] = (byte)((nlpsTab[iCX] << 1) | (1 - mpsCX));
                        }
                        else
                        {
                            stats[context] = (byte)((nlpsTab[iCX] << 1) | mpsCX);
                        }
                    }
                    else
                    {
                        bit = mpsCX;
                        stats[context] = (byte)((nmpsTab[iCX] << 1) | mpsCX);
                    }
                    // RENORMD
                    do
                    {
                        if (m_ct == 0)
                        {
                            byteIn();
                        }
                        m_a <<= 1;
                        m_c <<= 1;
                        --m_ct;
                    } while ((m_a & 0x80000000) == 0);
                }
            }
            else
            {
                m_c -= m_a;
                // LPS_EXCHANGE
                if (m_a < qe)
                {
                    bit = mpsCX;
                    stats[context] = (byte)((nmpsTab[iCX] << 1) | mpsCX);
                }
                else
                {
                    bit = 1 - mpsCX;
                    if (switchTab[iCX] != 0)
                    {
                        stats[context] = (byte)((nlpsTab[iCX] << 1) | (1 - mpsCX));
                    }
                    else
                    {
                        stats[context] = (byte)((nlpsTab[iCX] << 1) | mpsCX);
                    }
                }
                m_a = qe;
                // RENORMD
                do
                {
                    if (m_ct == 0)
                    {
                        byteIn();
                    }
                    m_a <<= 1;
                    m_c <<= 1;
                    --m_ct;
                } while ((m_a & 0x80000000) == 0);
            }
            return bit;
        }

        // Decode eight bits.
        public int DecodeByte(uint context, JArithmeticDecoderStats stats)
        {
            int b;
            int i;

            b = 0;
            for (i = 0; i < 8; ++i)
            {
                b = (b << 1) | DecodeBit(context, stats);
            }
            return b;
        }

        // Returns false for OOB, otherwise sets *<x> and returns true.
        public bool DecodeInt(ref int x, JArithmeticDecoderStats stats)
        {
            int s;
            uint v;
            int i;

            m_prev = 1;
            s = decodeIntBit(stats);
            if (decodeIntBit(stats) != 0)
            {
                if (decodeIntBit(stats) != 0)
                {
                    if (decodeIntBit(stats) != 0)
                    {
                        if (decodeIntBit(stats) != 0)
                        {
                            if (decodeIntBit(stats) != 0)
                            {
                                v = 0;
                                for (i = 0; i < 32; ++i)
                                {
                                    v = (v << 1) | (uint)decodeIntBit(stats);
                                }
                                v += 4436;
                            }
                            else
                            {
                                v = 0;
                                for (i = 0; i < 12; ++i)
                                {
                                    v = (v << 1) | (uint)decodeIntBit(stats);
                                }
                                v += 340;
                            }
                        }
                        else
                        {
                            v = 0;
                            for (i = 0; i < 8; ++i)
                            {
                                v = (v << 1) | (uint)decodeIntBit(stats);
                            }
                            v += 84;
                        }
                    }
                    else
                    {
                        v = 0;
                        for (i = 0; i < 6; ++i)
                        {
                            v = (v << 1) | (uint)decodeIntBit(stats);
                        }
                        v += 20;
                    }
                }
                else
                {
                    v = (uint)decodeIntBit(stats);
                    v = (v << 1) | (uint)decodeIntBit(stats);
                    v = (v << 1) | (uint)decodeIntBit(stats);
                    v = (v << 1) | (uint)decodeIntBit(stats);
                    v += 4;
                }
            }
            else
            {
                v = (uint)decodeIntBit(stats);
                v = (v << 1) | (uint)decodeIntBit(stats);
            }

            if (s != 0)
            {
                if (v == 0)
                {
                    return false;
                }
                x = -(int)v;
            }
            else
            {
                x = (int)v;
            }
            return true;
        }

        public uint DecodeIAID(uint codeLen, JArithmeticDecoderStats stats)
        {
            uint i;
            int bit;

            m_prev = 1;
            for (i = 0; i < codeLen; ++i)
            {
                bit = DecodeBit(m_prev, stats);
                m_prev = (m_prev << 1) | (uint)bit;
            }
            return (uint)(m_prev - (1 << (int)codeLen));
        }

        private uint readByte()
        {
            if (m_limitStream)
            {
                --m_dataLen;
                if (m_dataLen < 0)
                {
                    return 0xff;
                }
            }
            return (uint)m_str.ReadByte() & 0xff;
        }

        private int decodeIntBit(JArithmeticDecoderStats stats)
        {
            int bit;

            bit = DecodeBit(m_prev, stats);
            
            if (m_prev < 0x100)
            {
                m_prev = (m_prev << 1) | (uint)bit;
            }
            else
            {
                m_prev = (((m_prev << 1) | (uint)bit) & 0x1ff) | 0x100;
            }

            return bit;
        }

        private void byteIn()
        {
            if (m_buf0 == 0xff)
            {
                if (m_buf1 > 0x8f)
                {
                    m_ct = 8;
                }
                else
                {
                    m_buf0 = m_buf1;
                    m_buf1 = readByte();
                    m_c = m_c + 0xfe00 - (m_buf0 << 9);
                    m_ct = 7;
                }
            }
            else
            {
                m_buf0 = m_buf1;
                m_buf1 = readByte();
                m_c = m_c + 0xff00 - (m_buf0 << 8);
                m_ct = 8;
            }
        }

        private static uint[] qeTab = {
          0x56010000, 0x34010000, 0x18010000, 0x0AC10000,
          0x05210000, 0x02210000, 0x56010000, 0x54010000,
          0x48010000, 0x38010000, 0x30010000, 0x24010000,
          0x1C010000, 0x16010000, 0x56010000, 0x54010000,
          0x51010000, 0x48010000, 0x38010000, 0x34010000,
          0x30010000, 0x28010000, 0x24010000, 0x22010000,
          0x1C010000, 0x18010000, 0x16010000, 0x14010000,
          0x12010000, 0x11010000, 0x0AC10000, 0x09C10000,
          0x08A10000, 0x05210000, 0x04410000, 0x02A10000,
          0x02210000, 0x01410000, 0x01110000, 0x00850000,
          0x00490000, 0x00250000, 0x00150000, 0x00090000,
          0x00050000, 0x00010000, 0x56010000
        };

        private static int[] nmpsTab = {
           1,  2,  3,  4,  5, 38,  7,  8,  9, 10, 11, 12, 13, 29, 15, 16,
          17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
          33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 45, 46
        };

        private static int[] nlpsTab = {
           1,  6,  9, 12, 29, 33,  6, 14, 14, 14, 17, 18, 20, 21, 14, 14,
          15, 16, 17, 18, 19, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
          30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 46
        };

        private static int[] switchTab = {
          1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0,
          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private uint m_buf0, m_buf1;
        private uint m_c, m_a;
        private int m_ct;

        private uint m_prev;			// for the integer decoder

        private Stream m_str;
        private int m_dataLen;
        private bool m_limitStream;
    }
}
