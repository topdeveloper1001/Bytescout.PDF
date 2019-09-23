using System;

namespace Bytescout.PDF
{
    internal static class BinaryUtility
    {
        public static byte[] UInt16ToBytes(UInt16 value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value / 256);
            bytes[1] = (byte)(value - 256 * bytes[0]);
            return bytes;
        }

        public static byte[] UInt32ToBytes(UInt32 value)
        {
            byte[] bytes = new byte[4];
            int m = 256 * 256 * 256;

            for (int i = 0; i < 4; ++i)
            {
                bytes[i] = (byte)(value / m);
                value = (UInt32)(value - bytes[i] * m);
                m = m / 256;
            }

            return bytes;
        }

        public static byte[] UInt64ToBytes(UInt64 value)
        {
            byte[] bytes = new byte[8];
            ulong m = 256 * 256 * 256;
            m = m * m * 256;

            for (int i = 0; i < 8; ++i)
            {
                bytes[i] = (byte)(value / m);
                value = (UInt64)(value - bytes[i] * m);
                m = m / 256;
            }

            return bytes;
        }

        public static double ConvertFromIeeeExtended(byte[] bytes)
        {
            double f;
            int expon;
            ulong hiMant, loMant;

            expon = ((bytes[0] & 0x7F) << 8) | (bytes[1] & 0xFF);
            hiMant = ((ulong)(bytes[2] & 0xFF) << 24)
                    | ((ulong)(bytes[3] & 0xFF) << 16)
                    | ((ulong)(bytes[4] & 0xFF) << 8)
                    | ((ulong)bytes[5] & 0xFF);
            loMant = ((ulong)(bytes[6] & 0xFF) << 24)
                    | ((ulong)(bytes[7] & 0xFF) << 16)
                    | ((ulong)(bytes[8] & 0xFF) << 8)
                    | ((ulong)bytes[9] & 0xFF);

            if (expon == 0 && hiMant == 0 && loMant == 0)
            {
                f = 0;
            }
            else
            {
                if (expon == 0x7FFF)
                {
                    f = Double.MaxValue;
                }
                else
                {
                    expon -= 16383;
                    f = ((float)hiMant * Math.Pow(2, expon -= 31));
                    f += ((float)loMant * Math.Pow(2, expon -= 32));
                }
            }

            if ((bytes[0] & 0x80) != 0)
                return -f;
            else
                return f;
        }
    }
}
