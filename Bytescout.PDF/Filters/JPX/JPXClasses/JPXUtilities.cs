using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class JPXUtilities
    {
        internal static bool GetInt8(Stream stream, ref int result)
        {
            result = 0;
            int c;

            if ((c = stream.ReadByte()) == -1)
                return false;

            result = c;

            return true;
        }

        internal static bool GetInt16(Stream stream, ref int result)
        {
            result = 0;
            int c1, c2;

            if ((c1 = stream.ReadByte()) == -1 || (c2 = stream.ReadByte()) == -1)
                return false;

            result = (int)(((byte)c1 << 8) | (byte)c2);

            return true;
        }

        internal static bool GetInt32(Stream stream, ref int result)
        {
            result = 0;
            int c1, c2, c3, c4;

            if ((c1 = stream.ReadByte()) == -1 || (c2 = stream.ReadByte()) == -1
                || (c3 = stream.ReadByte()) == -1 || (c4 = stream.ReadByte()) == -1)
                return false;

            result = (int)(((byte)c1 << 24) | ((byte)c2 << 16) | ((byte)c3 << 8) | ((byte)c4));

            return true;
        }

        internal static bool GetInt64(Stream stream, ref int result)
        {
            result = 0;
            int c1 = 0, c2 = 0;

            if (!GetInt32(stream, ref c1) || !GetInt32(stream, ref c2))
                return false;

            result = ((c1 << 32) | c2);

            return true;
        }

        internal static int CeilDiv(int a, int b)
        {
            return (int)Math.Ceiling((double)(a / b));
        }

        internal static int CeilDivPow2(int a, int b)
        {
            //return (uint)((a + (Math.Pow(2, b) - 1)) / Math.Pow(2, b));
            int res = 1;
            for (int i = 0; i < b; ++i)
                res <<= 1;
            res += a - 1;
            for (int i = 0; i < b; ++i)
                res >>= 1;
            return res;
        }

        internal static int FloorDivPow2(int a, int b)
        {
            return (int)(a / Math.Pow(2, b));
        }
    }
}
