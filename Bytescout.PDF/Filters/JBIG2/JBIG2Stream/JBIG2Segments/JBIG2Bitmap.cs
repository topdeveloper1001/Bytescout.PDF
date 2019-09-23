using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    struct JBIG2BitmapPtr
    {
        public byte[] p;
        public int shift;
        public int x;
        public int index;
    }

    internal class JBIG2Bitmap : JBIG2Segment
    {
        public JBIG2Bitmap(uint segNumA, int wA, int hA)
            : base(segNumA)
        {
            m_w = wA;
            m_h = hA;
            m_line = (wA + 7) >> 3;
            if (m_w <= 0 || m_h <= 0 || m_line <= 0 || m_h >= (Int32.MaxValue - 1) / m_line)
            {
                m_data = null;
                return;
            }
            // need to allocate one extra guard byte for use in combine()
            m_data = new byte[m_h * m_line + 1];
            m_data[m_h * m_line] = 0;
        }

        public override JBIG2SegmentType Type { get { return JBIG2SegmentType.jbig2SegBitmap; } }

        public JBIG2Bitmap Copy() { return new JBIG2Bitmap(0, this); }

        public JBIG2Bitmap GetSlice(uint x, uint y, uint wA, uint hA)
        {
            JBIG2Bitmap slice;
            uint xx, yy;

            slice = new JBIG2Bitmap(0, (int)wA, (int)hA);
            slice.ClearToZero();
            for (yy = 0; yy < hA; ++yy)
            {
                for (xx = 0; xx < wA; ++xx)
                {
                    if (GetPixel((int)(x + xx), (int)(y + yy)) != 0)
                    {
                        slice.SetPixel((int)xx, (int)yy);
                    }
                }
            }
            return slice;
        }

        public void Expand(int newH, uint pixel)
        {
            if (newH <= m_h || m_line <= 0 || newH >= (Int32.MaxValue - 1) / m_line)
            {
                return;
            }
            // need to allocate one extra guard byte for use in combine()
            byte[] buffer = new byte[newH * m_line + 1];
            Array.Copy(m_data, buffer, m_h * m_line);

            m_data = buffer;
            if (pixel != 0)
            {
                setValue(m_data, m_h * m_line, (newH - m_h) * m_line, 0xff);
            }
            else
            {
                setValue(m_data, m_h * m_line, (newH - m_h) * m_line, 0x00);
            }
            m_h = newH;
            m_data[m_h * m_line] = 0;
        }

        public void ClearToZero()
        {
            setValue(m_data, 0, m_h * m_line, 0);
        }

        public void ClearToOne()
        {
            setValue(m_data, 0, m_h * m_line, 0xff);
        }

        public int Width { get { return m_w; } }
        public int Height { get { return m_h; } }

        public int GetPixel(int x, int y)
        {
            return (x < 0 || x >= m_w || y < 0 || y >= m_h) ? 0 :
                   (m_data[y * m_line + (x >> 3)] >> (7 - (x & 7))) & 1;
        }

        public void SetPixel(int x, int y)
        {
            m_data[y * m_line + (x >> 3)] |= (byte)(1 << (7 - (x & 7)));
        }

        public void ClearPixel(int x, int y)
        {
            m_data[y * m_line + (x >> 3)] &= (byte)(0x7f7f >> (x & 7));
        }

        public void GetPixelPtr(int x, int y, ref JBIG2BitmapPtr ptr)
        {
            if (y < 0 || y >= m_h || x >= m_w)
            {
                ptr.p = null;
            }
            else if (x < 0)
            {
                ptr.p = m_data;
                ptr.index = y * m_line;
                ptr.shift = 7;
                ptr.x = x;
            }
            else
            {
                ptr.p = m_data;
                ptr.index = y * m_line + (x >> 3);
                ptr.shift = 7 - (x & 7);
                ptr.x = x;
            }
        }

        public int NextPixel(ref JBIG2BitmapPtr ptr)
        {
            int pix;

            if (ptr.p == null)
            {
                pix = 0;
            }
            else if (ptr.x < 0)
            {
                ++ptr.x;
                pix = 0;
            }
            else
            {
                pix = (ptr.p[ptr.index] >> ptr.shift) & 1;
                if (++ptr.x == m_w)
                {
                    ptr.p = null;
                }
                else if (ptr.shift == 0)
                {
                    ++ptr.index;
                    ptr.shift = 7;
                }
                else
                {
                    --ptr.shift;
                }
            }
            return pix;
        }

        public void DuplicateRow(int yDest, int ySrc) 
        {
            Array.Copy(m_data, ySrc * m_line, m_data, yDest * m_line, m_line);
        }

        public void Combine(JBIG2Bitmap bitmap, int x, int y,
              uint combOp)
        {
            int x0, x1, y0, y1, xx, yy;
            byte[] srcPtr, destPtr;

            int srcPtr_i = 0, destPtr_i = 0;
            uint src0, src1, src, dest, s1, s2, m1, m2, m3;
            bool oneByte;

            if (y < 0)
            {
                y0 = -y;
            }
            else
            {
                y0 = 0;
            }
            if (y + bitmap.m_h > m_h)
            {
                y1 = m_h - y;
            }
            else
            {
                y1 = bitmap.m_h;
            }
            if (y0 >= y1)
            {
                return;
            }

            if (x >= 0)
            {
                x0 = x & ~7;
            }
            else
            {
                x0 = 0;
            }
            x1 = x + bitmap.m_w;
            if (x1 > m_w)
            {
                x1 = m_w;
            }
            if (x0 >= x1)
            {
                return;
            }

            s1 = (uint) x & 7;
            s2 = 8 - s1;
            m1 = (uint)(0xff >> (x1 & 7));
            m2 = (uint) (0xff << (((x1 & 7) == 0) ? 0 : 8 - (x1 & 7)));
            m3 = (uint)((0xff >> (int)s1) & m2);

            oneByte = x0 == ((x1 - 1) & ~7);

            for (yy = y0; yy < y1; ++yy)
            {

                // one byte per line -- need to mask both left and right side
                if (oneByte)
                {
                    if (x >= 0)
                    {
                        destPtr = m_data;
                        destPtr_i = (y + yy) * m_line + (x >> 3);

                        srcPtr = bitmap.m_data;
                        srcPtr_i = yy * bitmap.m_line;
                        dest = destPtr[destPtr_i];
                        src1 = srcPtr[srcPtr_i];
                        switch (combOp)
                        {
                            case 0: // or
                                dest |= (src1 >> (int)s1) & m2;
                                break;
                            case 1: // and
                                dest &= ((0xff00 | src1) >> (int)s1) | m1;
                                break;
                            case 2: // xor
                                dest ^= (src1 >> (int)s1) & m2;
                                break;
                            case 3: // xnor
                                dest ^= ((src1 ^ 0xff) >> (int)s1) & m2;
                                break;
                            case 4: // replace
                                dest = (dest & ~m3) | ((src1 >> (int)s1) & m3);
                                break;
                        }
                        destPtr[destPtr_i] = (byte)dest;
                    }
                    else
                    {
                        destPtr = m_data; destPtr_i = (y + yy) * m_line;
                        srcPtr = bitmap.m_data; srcPtr_i = yy * bitmap.m_line + (-x >> 3);
                        dest = destPtr[destPtr_i];
                        src1 = srcPtr[srcPtr_i];
                        switch (combOp)
                        {
                            case 0: // or
                                dest |= src1 & m2;
                                break;
                            case 1: // and
                                dest &= src1 | m1;
                                break;
                            case 2: // xor
                                dest ^= src1 & m2;
                                break;
                            case 3: // xnor
                                dest ^= (src1 ^ 0xff) & m2;
                                break;
                            case 4: // replace
                                dest = (src1 & m2) | (dest & m1);
                                break;
                        }
                        destPtr[destPtr_i] = (byte)dest;
                    }

                    // multiple bytes per line -- need to mask left side of left-most
                    // byte and right side of right-most byte
                }
                else
                {

                    // left-most byte
                    if (x >= 0)
                    {
                        destPtr = m_data; destPtr_i = (y + yy) * m_line + (x >> 3);
                        srcPtr = bitmap.m_data; srcPtr_i = yy * bitmap.m_line;
                        src1 = srcPtr[srcPtr_i++];
                        dest = destPtr[destPtr_i];
                        switch (combOp)
                        {
                            case 0: // or
                                dest |= src1 >> (int)s1;
                                break;
                            case 1: // and
                                dest &= (0xff00 | src1) >> (int)s1;
                                break;
                            case 2: // xor
                                dest ^= src1 >> (int)s1;
                                break;
                            case 3: // xnor
                                dest ^= (src1 ^ 0xff) >> (int)s1;
                                break;
                            case 4: // replace
                                dest = (uint)(dest & (0xff << (int)s2)) | (src1 >> (int)s1);
                                break;
                        }
                        destPtr[destPtr_i++] = (byte)dest;
                        xx = x0 + 8;
                    }
                    else
                    {
                        destPtr = m_data; destPtr_i = (y + yy) * m_line;
                        srcPtr = bitmap.m_data; srcPtr_i = yy * bitmap.m_line + (-x >> 3);
                        src1 = srcPtr[srcPtr_i++];
                        xx = x0;
                    }

                    // middle bytes
                    for (; xx < x1 - 8; xx += 8)
                    {
                        dest = destPtr[destPtr_i];
                        src0 = src1;
                        src1 = srcPtr[srcPtr_i++];
                        src = (((src0 << 8) | src1) >> (int)s1) & 0xff;
                        switch (combOp)
                        {
                            case 0: // or
                                dest |= src;
                                break;
                            case 1: // and
                                dest &= src;
                                break;
                            case 2: // xor
                                dest ^= src;
                                break;
                            case 3: // xnor
                                dest ^= src ^ 0xff;
                                break;
                            case 4: // replace
                                dest = src;
                                break;
                        }
                        destPtr[destPtr_i++] = (byte)dest;
                    }

                    // right-most byte
                    // note: this last byte (src1) may not actually be used, depending
                    // on the values of s1, m1, and m2 - and in fact, it may be off
                    // the edge of the source bitmap, which means we need to allocate
                    // one extra guard byte at the end of each bitmap
                    dest = destPtr[destPtr_i];
                    src0 = src1;
                    src1 = srcPtr[srcPtr_i++];
                    src = (((src0 << 8) | src1) >> (int)s1) & 0xff;
                    switch (combOp)
                    {
                        case 0: // or
                            dest |= src & m2;
                            break;
                        case 1: // and
                            dest &= src | m1;
                            break;
                        case 2: // xor
                            dest ^= src & m2;
                            break;
                        case 3: // xnor
                            dest ^= (src ^ 0xff) & m2;
                            break;
                        case 4: // replace
                            dest = (src & m2) | (dest & m1);
                            break;
                    }
                    destPtr[destPtr_i] = (byte)dest;
                }
            }
        }

        public byte[] GetDataPtr() { return m_data; }
        public int GetDataSize() { return m_h * m_line; }

        private JBIG2Bitmap(uint segNumA, JBIG2Bitmap bitmap)
            : base(segNumA)
        {
            m_w = bitmap.m_w;
            m_h = bitmap.m_h;
            m_line = bitmap.m_line;
            if (m_w <= 0 || m_h <= 0 || m_line <= 0 || m_h >= (Int32.MaxValue - 1) / m_line)
            {
                m_data = null;
                return;
            }
            // need to allocate one extra guard byte for use in combine()
            m_data = new byte[m_h * m_line + 1];
            Array.Copy(bitmap.m_data, m_data, m_h * m_line);
            m_data[m_h * m_line] = 0;
        }

        private static void setValue(byte[] data, int offset, int len, byte value)
        {
            for (int i = offset; i < len; ++i)
                data[i] = value;
        }

        private int m_w, m_h, m_line;
        private byte[] m_data;
    }
}
