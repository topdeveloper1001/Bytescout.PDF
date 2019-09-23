
using System;


namespace Bytescout.PDF
{
    internal class SupportClass
	{
		public static long Identity(long literal)
		{
			return literal;
		}
		
		public static ulong Identity(ulong literal)
		{
			return literal;
		}
		
		public static float Identity(float literal)
		{
			return literal;
		}
		
		public static double Identity(double literal)
		{
			return literal;
		}
		
		public static int URShift(int number, int bits)
		{
			if ( number >= 0)
				return number >> bits;
			else
				return (number >> bits) + (2 << ~bits);
		}
		
		public static int URShift(int number, long bits)
		{
			return URShift(number, (int)bits);
		}
		
		public static long URShift(long number, int bits)
		{
			if ( number >= 0)
				return number >> bits;
			else
				return (number >> bits) + (2L << ~bits);
		}
		
		public static long URShift(long number, long bits)
		{
			return URShift(number, (int)bits);
		}
		
		public static System.Int32 ReadInput(System.IO.Stream sourceStream, byte[] target, int start, int count)
		{
			// Returns 0 bytes if not enough space in target
			if (target.Length == 0)
				return 0;

			byte[] receiver = new byte[target.Length];
			int bytesRead   = sourceStream.Read(receiver, start, count);

			// Returns -1 if EOF
			if (bytesRead == 0)	
				return -1;
                
			for(int i = start; i < start + bytesRead; i++)
				target[i] = (byte)receiver[i];
                
			return bytesRead;
		}
		
		public static System.Int32 ReadInput(System.IO.TextReader sourceTextReader, byte[] target, int start, int count)
		{
			// Returns 0 bytes if not enough space in target
			if (target.Length == 0) return 0;

			char[] charArray = new char[target.Length];
			int bytesRead = sourceTextReader.Read(charArray, start, count);

			// Returns -1 if EOF
			if (bytesRead == 0) return -1;

			for(int index=start; index<start+bytesRead; index++)
				target[index] = (byte)charArray[index];

			return bytesRead;
		}
		
		public static byte[] ToByteArray(System.String sourceString)
		{
			return System.Text.UTF8Encoding.UTF8.GetBytes(sourceString);
		}
		
		public static char[] ToCharArray(byte[] byteArray) 
		{
			return System.Text.UTF8Encoding.UTF8.GetChars(byteArray);
		}


	}
}