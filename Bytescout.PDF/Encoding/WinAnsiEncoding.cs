using System;
using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal static class WinAnsiEncoding
    {
        public static string GetString(byte[] bytes)
        {
            char[] result = new char[bytes.Length];
            for (int i = 0; i < bytes.Length; ++i)
                result[i] = (char)bytes[i];

            return new string(result);
        }

        public static char GetChar(byte b)
        {
            return (char)b;
        }
    }
}
