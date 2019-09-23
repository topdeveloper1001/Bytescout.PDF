using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class JBIG2MMRDecoder
    {
        public JBIG2MMRDecoder()
        {
            m_str = null;
            Reset();
        }

        public void Reset()
        {
            m_buf = 0;
            m_bufLen = 0;
            m_nBytesRead = 0;
        }

        public void SetStream(Stream strA) { m_str = strA; }

        public int Get2DCode()
        {
            uint p;

            if (m_bufLen == 0)
            {
                m_buf = (uint)m_str.ReadByte() & 0xff;
                m_bufLen = 8;
                ++m_nBytesRead;
                p = CCITTCodeTables.TwoDimTab1[(m_buf >> 1) & 0x7f];
            }
            else if (m_bufLen == 8)
            {
                p = CCITTCodeTables.TwoDimTab1[(m_buf >> 1) & 0x7f];
            }
            else
            {
                p = CCITTCodeTables.TwoDimTab1[(m_buf << (int)(7 - m_bufLen)) & 0x7f];
                if ((short)p < 0 || (short)p > (int)m_bufLen)
                {
                    m_buf = (m_buf << 8) | ((uint)m_str.ReadByte() & 0xff);
                    m_bufLen += 8;
                    ++m_nBytesRead;
                    p = CCITTCodeTables.TwoDimTab1[(m_buf >> (int)(m_bufLen - 7)) & 0x7f];
                }
            }
            if ((short)p < 0)
            {
                return 0;
            }
            m_bufLen -= (uint)((short)p);
            return (short)(p >> 16);
        }

        public int GetWhiteCode()
        {
            uint p;
            int code;

            if (m_bufLen == 0)
            {
                m_buf = (uint)m_str.ReadByte() & 0xff;
                m_bufLen = 8;
                ++m_nBytesRead;
            }
            while (true)
            {
                if (m_bufLen >= 7 && ((m_buf >> (int)(m_bufLen - 7)) & 0x7f) == 0)
                {
                    if (m_bufLen <= 12)
                    {
                        code = (int)m_buf << (int)(12 - m_bufLen);
                    }
                    else
                    {
                        code = (int)m_buf >> (int)(m_bufLen - 12);
                    }
                    p = CCITTCodeTables.WhiteTab1[code & 0x1f];
                }
                else
                {
                    if (m_bufLen <= 9)
                    {
                        code = (int)m_buf << (int)(9 - m_bufLen);
                    }
                    else
                    {
                        code = (int)m_buf >> (int)(m_bufLen - 9);
                    }
                    p = CCITTCodeTables.WhiteTab2[code & 0x1ff];
                }
                if ((short)p > 0 && (short)p <= (int)m_bufLen)
                {
                    m_bufLen -= (uint)((short)p);
                    return (short)(p >> 16);
                }
                if (m_bufLen >= 12)
                {
                    break;
                }
                m_buf = (m_buf << 8) | ((uint)m_str.ReadByte() & 0xff);
                m_bufLen += 8;
                ++m_nBytesRead;
            }
            // eat a bit and return a positive number so that the caller doesn't
            // go into an infinite loop
            --m_bufLen;
            return 1;
        }

        public int GetBlackCode()
        {
            uint p;
            int code;

            if (m_bufLen == 0)
            {
                m_buf = (uint)m_str.ReadByte() & 0xff;
                m_bufLen = 8;
                ++m_nBytesRead;
            }
            while (true)
            {
                if (m_bufLen >= 6 && ((m_buf >> (int)(m_bufLen - 6)) & 0x3f) == 0)
                {
                    if (m_bufLen <= 13)
                    {
                        code = (int)m_buf << (int)(13 - m_bufLen);
                    }
                    else
                    {
                        code = (int)m_buf >> (int)(m_bufLen - 13);
                    }
                    p = CCITTCodeTables.BlackTab1[code & 0x7f];
                }
                else if (m_bufLen >= 4 && ((m_buf >> (int)(m_bufLen - 4)) & 0x0f) == 0)
                {
                    if (m_bufLen <= 12)
                    {
                        code = (int)m_buf << (int)(12 - m_bufLen);
                    }
                    else
                    {
                        code = (int)m_buf >> (int)(m_bufLen - 12);
                    }
                    p = CCITTCodeTables.BlackTab2[(code & 0xff) - 64];
                }
                else
                {
                    if (m_bufLen <= 6)
                    {
                        code = (int)m_buf << (int)(6 - m_bufLen);
                    }
                    else
                    {
                        code = (int)m_buf >> (int)(m_bufLen - 6);
                    }
                    p = CCITTCodeTables.BlackTab3[code & 0x3f];
                }
                if ((short)p > 0 && (short)p <= (int)m_bufLen)
                {
                    m_bufLen -= (uint)((short)p);
                    return (short)(p >> 16);
                }
                if (m_bufLen >= 13)
                {
                    break;
                }
                m_buf = (m_buf << 8) | ((uint)m_str.ReadByte() & 0xff);
                m_bufLen += 8;
                ++m_nBytesRead;
            }
            // eat a bit and return a positive number so that the caller doesn't
            // go into an infinite loop
            --m_bufLen;
            return 1;
        }

        public uint Get24Bits()
        {
            while (m_bufLen < 24)
            {
                m_buf = (m_buf << 8) | ((uint)m_str.ReadByte() & 0xff);
                m_bufLen += 8;
                ++m_nBytesRead;
            }
            return (m_buf >> (int)(m_bufLen - 24)) & 0xffffff;
        }

        public void skipTo(int length)
        {
            while (m_nBytesRead < length)
            {
                m_str.ReadByte();
                ++m_nBytesRead;
            }
        }

        private Stream m_str;
        private uint m_buf;
        private uint m_bufLen;
        private uint m_nBytesRead;
    }
}
