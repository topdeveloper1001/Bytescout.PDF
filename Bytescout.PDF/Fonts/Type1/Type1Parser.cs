using System.Collections.Generic;
using System.Text;

namespace Bytescout.PDF
{
    internal class Type1Parser
    {
	    private struct Subr
	    {
		    public Subr(int offset, int length)
		    {
			    Offset = offset;
			    Length = length;
		    }

		    public int Offset;
		    public int Length;
	    }

	    private int _asciiLength;
	    private int _encryptedLength;

	    private string _fontName = "";
	    private int _italicAngle = 0;
	    private System.Drawing.Rectangle _fontBBox = System.Drawing.Rectangle.Empty;
	    private List<KeyValuePair<ushort, string>> _charSet = null;
	    private string _encoding;
	    private Dictionary<string, int> _widths = null;
	    private Subr[] _subrs;

	    internal string FontName { get { return _fontName; } }

        internal System.Drawing.Rectangle FontBBox { get { return _fontBBox; } }

        internal int ItalicAngle { get { return _italicAngle; } }

        internal string Encoding { get { return _encoding; } }

	    internal Type1Parser(Reader reader)
	    {
		    initialize(reader);
	    }

	    internal List<KeyValuePair<ushort, string>> GetCharset()
        {
            return _charSet;
        }

        internal Dictionary<string, int> GetWidths()
        {
            return _widths;
        }

        internal static bool IsType1Font(byte[] buffer)
        {
            if (buffer[0] == '%' || buffer[0] == 0x80)
                return true;
            return false;
        }

        private void initialize(Reader reader)
        {
            loadLengths(reader);
            parseASCII(reader);
            parseEncryptedBlock(reader);
        }

        private void loadLengths(Reader reader)
        {
            reader.Position = reader.Length;
            _encryptedLength = reader.FindPreviewSubstring("cleartomark");
            _asciiLength = reader.FindPreviewSubstring("eexec");

            if (_encryptedLength <= 0 || _asciiLength <= 0)
                throw new PDFWrongFontFileException();
            
            _asciiLength += 7;
            _encryptedLength = _encryptedLength - _asciiLength - 519;
        }

        private void parseASCII(Reader reader)
        {
            parseFontName(reader);
            parseItalicAngle(reader);
            parseBBOX(reader);
            parseCharSet(reader);
        }

        private void parseFontName(Reader reader)
        {
            int pos = reader.FindSubstring("/FullName", 0, _asciiLength);
            if (pos <= 0)
            {
                _fontName = "tmp";
                return;
            }

            reader.Position = pos + 9;
            reader.SkipEOL();

            if (reader.Peek() == '(')
            {
                StringBuilder tmp = new StringBuilder();
                reader.Position++;
                while (reader.Peek() != ')' && reader.Peek() != -1 && reader.Peek() != 0)
                    tmp.Append((char)reader.ReadByte());
                _fontName = tmp.ToString();
            }
            else
            {
                _fontName = "tmp";
            }
        }

        private void parseItalicAngle(Reader reader)
        {
            int pos = reader.FindSubstring("/ItalicAngle", 0, _asciiLength);
            if (pos <= 0)
                return;

            reader.Position = pos + 12;
            reader.SkipEOL();
            reader.ReadLexeme();

            bool succes;
            float tmp = (float)reader.Lexeme.ToDouble(out succes);
            _italicAngle = (int)tmp;
        }

        private void parseBBOX(Reader reader)
        {
            int pos = reader.FindSubstring("/FontBBox", 0, _asciiLength);
            if (pos <= 0)
                return;

            reader.Position = pos + 9;
            reader.SkipEOL();
            if (reader.Peek() == '{' || reader.Peek() == '[')
            {
                reader.Position++;
                int xMin, yMin, xMax, yMax;

                bool succes;

                reader.ReadLexeme();
                xMin = (int)reader.Lexeme.ToDouble(out succes);

                reader.ReadLexeme();
                yMin = (int)reader.Lexeme.ToDouble(out succes);

                reader.ReadLexeme();
                xMax = (int)reader.Lexeme.ToDouble(out succes);

                reader.ReadLexeme();
                yMax = (int)reader.Lexeme.ToDouble(out succes);

                _fontBBox = new System.Drawing.Rectangle(xMin, yMin, xMax, yMax);
            }
        }

        private void parseCharSet(Reader reader)
        {
            int pos = reader.FindSubstring("/Encoding", 0, _asciiLength);
            if (pos <= 0)
                throw new PDFWrongFontFileException();

            reader.Position = pos + 9;
            reader.SkipEOL();
            reader.ReadLexeme();
            Lexeme lex = reader.Lexeme;

            bool succes;
            int tmp = lex.ToInt(out succes);

            if (succes)
            {
                reader.ReadLexeme();
                pos = reader.Position;

                reader.ReadLexeme();
                if (lex.AreEqual("dup"))
                {
                    reader.Position = pos;
                }
                else
                {
                    for (; ; )
                    {
                        reader.ReadLexeme();
                        if (lex.AreEqual("for"))
                            break;
                        else if (lex.AreEqual(""))
                            throw new PDFWrongFontFileException();
                    }
                    reader.SkipEOL();
                }

                _charSet = new List<KeyValuePair<ushort, string>>();
                for (; ; )
                {
                    reader.ReadLexeme();
                    if (lex.AreEqual("def"))
                        break;

                    if (lex.AreEqual("dup"))
                    {
                        reader.SkipEOL();
                        reader.ReadLexeme();
                        int glyfIndex = lex.ToInt(out succes);
                        if(!succes)
                            throw new PDFWrongFontFileException();
                        reader.SkipEOL();

                        if (reader.Peek() == '/')
                        {
                            reader.Position++;
                            reader.ReadLexeme();
                            string glyfName = lex.ToString();
                            reader.SkipEOL();
                            reader.ReadLexeme();

                            if (!lex.AreEqual("put"))
                                throw new PDFWrongFontFileException();
                            reader.SkipEOL();

                            _charSet.Add(new KeyValuePair<ushort, string>((ushort)glyfIndex, glyfName));
                        }
                        else
                            throw new PDFWrongFontFileException();
                    }
                }

            }
            else
            {
                _encoding = lex.ToString();
            }
        }

        private void parseEncryptedBlock(Reader reader)
        {
            reader.Position = _asciiLength;
            byte[] binaryData = null;
            ushort R = 55665;

            if (reader.Peek() != 0x80)//hex
            {
                binaryData = new byte[_encryptedLength - 8];
                for (int i = 0; i < 4; ++i)
                    decrypt(readHex(reader), ref R);

                int current = 0;
                while (reader.Position <= _asciiLength + _encryptedLength)
                {
                    byte b = readHex(reader);
                    binaryData[current] = decrypt(b, ref R);
                    current++;
                }
            }
            else
            {
                reader.Position += 6;
                binaryData = new byte[_encryptedLength - 6];
                for (int i = 0; i < binaryData.Length; ++i)
                    binaryData[i] = decrypt(reader.ReadByte(), ref R);
            }

            if (!parseWidths(binaryData))
                throw new PDFWrongFontFileException();
        }

        private bool parseWidths(byte[] binaryData)
        {
            Reader reader = new Reader(binaryData);
            if (!findCharStrings(reader))
                return false;

            _widths = new Dictionary<string, int>();
            for (; ; )
            {
                if (reader.Peek() != '/')
                {
                    reader.ReadLexeme();
                    if (reader.Lexeme.AreEqual("end"))
                        break;
                    return false;
                }

                reader.Position++;

                reader.ReadLexeme();
                string glyfName = reader.Lexeme.ToString();

                reader.ReadLexeme();
                bool succes;
                int encryptedLength = reader.Lexeme.ToInt(out succes);
                if (!succes)
                    return false;

                reader.ReadLexeme();
                if (!reader.Lexeme.AreEqual("RD") && !reader.Lexeme.AreEqual("-|"))
                    return false;

                reader.Position++;
                int width = 0;
                findWidth(reader, encryptedLength, ref width);

                _widths.Add(glyfName, width);
                reader.SkipEOL();
                reader.ReadLexeme();
                if (!reader.Lexeme.AreEqual("ND") && !reader.Lexeme.AreEqual("|-"))
                    return false;
                reader.SkipEOL();
            }

            return true;
        }

        private bool findWidth(Reader reader, int encryptedLength, ref int width)
        {
            ushort R = 4330;
            for (int i = 0; i < 4; ++i)
                decrypt(reader.ReadByte(), ref R);
            
            int currentNumber = 0;
            for (int i = 4; i < encryptedLength; ++i)
            {
                byte b = decrypt(reader.ReadByte(), ref R);
                if (b >= 32)
                {
                    int tmp = readNumber(b, ref R, reader, ref i);
                    currentNumber = tmp;
                }
                else
                {
                    if (b == 10)
                    {
                        int oldPos = reader.Position;
                        if (_subrs == null)
                            parseSubrs(reader);

                        if (currentNumber >= _subrs.Length)
                            throw new PDFWrongFontFileException();

                        Subr subr = _subrs[currentNumber];
                        reader.Position = subr.Offset;
                        if (findWidth(reader, subr.Length, ref currentNumber))
                        {
                            width = currentNumber;
                            reader.Position = oldPos;
                            reader.Position += encryptedLength - i;
                            return true;
                        }

                        reader.Position = oldPos;
                    }
                    else if (b == 13)//hsbw
                    {
                        reader.Position += encryptedLength - i;
                        width = currentNumber;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool findCharStrings(Reader reader)
        {
            reader.Position = 0;
            int pos = reader.FindNextSubstring("/CharStrings");
            if (pos <= 0)
                return false;

            reader.Position = pos + 12;
            for (; ; )
            {
                reader.ReadLexeme();
                if (reader.Lexeme.AreEqual("begin"))
                    break;
                else if (reader.Lexeme.AreEqual(""))
                    return false;
            }
            reader.SkipEOL();
            return true;
        }

        private int readNumber(byte firstByte, ref ushort R, Reader reader, ref int numRead)
        {
            int result;
            if (firstByte >= 32 && firstByte <= 246)
                result = firstByte - 139;
            else if (firstByte >= 247 && firstByte <= 250)
            {
                result = (firstByte - 247) * 256 + 108 + decrypt(reader.ReadByte(), ref R);
                numRead++;
            }
            else if (firstByte >= 251 && firstByte <= 254)
            {
                result = -(firstByte - 251) * 256 - 108 - decrypt(reader.ReadByte(), ref R);
                numRead++;
            }
            else
            {
                result = (decrypt(reader.ReadByte(), ref R) & 0xff) << 24;
                result |= (decrypt(reader.ReadByte(), ref R) & 0xff) << 16;
                result |= (decrypt(reader.ReadByte(), ref R) & 0xff) << 8;
                result |= (decrypt(reader.ReadByte(), ref R) & 0xff) << 0;
                numRead += 4;
            }
            
            return result;
        }

        private byte readHex(Reader reader)
        {
            byte b1, b2;
            while (Reader.IsEOL(reader.Peek()))
                reader.Position++;
            b1 = (byte)reader.ReadByte();
            if (!isHex(b1))
                throw new PDFWrongFontFileException();

            while (Reader.IsEOL(reader.Peek()))
                reader.Position++;
            b2 = (byte)reader.ReadByte();
            if (!isHex(b2))
                throw new PDFWrongFontFileException();
            b1 = toDecimal(b1);
            b2 = toDecimal(b2);

            return (byte)(b1 * 16 + b2);
        }

        private byte decrypt(int cipherChar, ref ushort R)
        {
            ushort plainChar = (ushort)(cipherChar ^ (R >> 8));
            R = (ushort)((cipherChar + R) * 52845 + 22719);
            return (byte)plainChar;
        }

        private byte toDecimal(byte b)
        {
            if (b >= '0' && b <= '9')
                return (byte)(b - '0');
            if (b >= 'a' && b <= 'f')
                return (byte)(b - 'a' + 10);
            if (b >= 'A' && b <= 'F')
                return (byte)(b - 'A' + 10);
            return 0;
        }

        private bool isHex(byte b)
        {
            if ((b >= '0' && b <= '9') || (b >= 'a' && b <= 'f') || (b >= 'A' && b <= 'F'))
                return true;
            return false;
        }

        private void parseSubrs(Reader reader)
        {
            reader.Position = 0;
            int subrsPos = reader.FindNextSubstring("/Subrs");
            reader.Position = subrsPos + 6; ;
            reader.SkipEOL();

            Lexeme lex = reader.Lexeme;
            reader.ReadLexeme();
            bool succes;
            int count = lex.ToInt(out succes);
            if (!succes)
                throw new PDFWrongFontFileException();

            reader.ReadLexeme();
            if (!lex.AreEqual("array"))
                throw new PDFWrongFontFileException();
            reader.SkipEOL();

            _subrs = new Subr[count];
            for (int i = 0; i < count; ++i)
            {
                reader.ReadLexeme();
                if (!lex.AreEqual("dup"))
                    throw new PDFWrongFontFileException();

                reader.ReadLexeme();
                int tmp = lex.ToInt(out succes);
                if (!succes || tmp != i)
                    throw new PDFWrongFontFileException();
                reader.SkipEOL();
                
                reader.ReadLexeme();
                int length = lex.ToInt(out succes);
                if (!succes)
                    throw new PDFWrongFontFileException();

                reader.SkipEOL();
                reader.ReadLexeme();
                reader.SkipEOL();

                _subrs[i] = new Subr(reader.Position, length);

                reader.Position += length;
                reader.ReadLexeme();
                reader.SkipEOL();
            }
        }
    }
}
