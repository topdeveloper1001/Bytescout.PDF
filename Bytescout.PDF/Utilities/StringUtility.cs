using System;
using System.Globalization;
using System.IO;

namespace Bytescout.PDF
{
    internal static class StringUtility
    {
        public static void WriteToStream(double value, Stream stream)
        {
            int integer = (int)value;
            int fractional = (int)((value - integer) * 10000);
            integer = mod(integer);
            fractional = mod(fractional);

            if (integer == 0 && fractional == 0)
            {
                stream.WriteByte((byte)'0');
                return;
            }

            byte[] buf = new byte[16];
            int count = 0;

            if (value < 0)
                stream.WriteByte((byte)'-');

            if (integer == 0)
                stream.WriteByte((byte)'0');

            while (integer != 0)
            {
                buf[count] = (byte)(integer % 10 + '0');
                integer /= 10;
                ++count;
            }

            for (int i = 0; i < count; ++i)
                stream.WriteByte(buf[count - i - 1]);

            if (fractional == 0)
                return;

            const int numToken = 4;

            stream.WriteByte((byte)'.');
            for (int i = 0; i < numToken; ++i)
            {
                buf[i] = (byte)(fractional % 10 + '0');
                fractional /= 10;
            }

            int zero = 0;
            for (int i = 0; i < numToken; ++i)
            {
                if (buf[i] == '0')
                    ++zero;
                else
                    break;
            }

            for (int i = 0; i < numToken - zero; ++i)
                stream.WriteByte(buf[numToken - 1 - i]);
        }

        public static string GetString(double val)
        {
            NumberFormatInfo numInfo = new NumberFormatInfo();
            numInfo.NumberDecimalSeparator = ".";
            return val.ToString("0.####", numInfo);
        }

        public static string UShortToHexString(ushort val)
        {
            byte b1 = (byte)(val / 256);
            byte b2 = (byte)(val - b1 * 256);

            string res = "";
            string tmp = b1.ToString("X");

            if (tmp.Length == 1)
                res += '0' + tmp;
            else
                res += tmp;

            tmp = b2.ToString("X");
            if (tmp.Length == 1)
                res += '0' + tmp;
            else
                res += tmp;

            return res;
        }

        public static DateTime ParseDateTime(string str)
        {
            if ((str.Substring(0, 2) != "D:"))
                str = "D:" + str;

            int YY = 1;
            int MM = 1;
            int DD = 1;
            int HH = 0;
            int mm = 0;
            int SS = 0;

            try
            {
                YY = int.Parse(str.Substring(2, 4));
                MM = int.Parse(str.Substring(6, 2));
                DD = int.Parse(str.Substring(8, 2));
                HH = int.Parse(str.Substring(10, 2));
                mm = int.Parse(str.Substring(12, 2));
                SS = int.Parse(str.Substring(14, 2));
            }
            catch
            {
                return DateTime.Today;
            }

            return new DateTime(YY, MM, DD, HH, mm, SS);
        }

        public static string CreatePDFDateTime(DateTime time)
        {
            string result = "D:";
            result += time.Year.ToString();

            if (time.Month < 10)
                result += '0';
            result += time.Month.ToString();

            if (time.Day < 10)
                result += '0';
            result += time.Day.ToString();

            if (time.Hour < 10)
                result += '0';
            result += time.Hour.ToString();

            if (time.Minute < 10)
                result += '0';
            result += time.Minute.ToString();

            if (time.Second < 10)
                result += '0';
            result += time.Second.ToString();

            return result;
        }

        private static int mod(int val)
        {
            if (val < 0)
                return -val;
            return val;
        }
    }
}
