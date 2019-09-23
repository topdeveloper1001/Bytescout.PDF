using System.Collections.Generic;

namespace Bytescout.PDF
{
    internal class ToUnicode
    {
        internal ToUnicode(PDFDictionaryStream toUnicode)
        {
            if (toUnicode != null)
            {
                toUnicode.Decode();
                toUnicode.GetStream().Position = 0;
                m_chars = new List<BfChar>();
                m_ranges = new List<BfRange>();

                Lexer lexer = new Lexer(toUnicode.GetStream(), null, null, 256);
                parse(lexer);
            }
        }

        internal ushort GetGlyf(char c)
        {
            if (m_ranges == null)
                return 0;

            int count = m_chars.Count;
            for (int i = 0; i < count; ++i)
            {
                if (m_chars[i].Char == c)
                    return m_chars[i].Glyf;
            }

            count = m_ranges.Count;
            for (int i = 0; i < count; ++i)
            {
                BfRange range = m_ranges[i];
                if (range.FirstChar <= c && c <= range.EndChar)
                    return (ushort)(range.FirstGlyf + c - range.FirstChar);
            }

            return 0;
        }

        internal char GetChar(ushort glyf)
        {
            if (m_ranges == null)
                return '\0';

            int count = m_chars.Count;
            for (int i = 0; i < count; ++i)
            {
                if (m_chars[i].Glyf == glyf)
                    return m_chars[i].Char;
            }

            count = m_ranges.Count;
            for (int i = 0; i < count; ++i)
            {
                BfRange range = m_ranges[i];
                if (range.FirstGlyf <= glyf && glyf <= range.EndGlyf)
                    return (char)(range.FirstChar + glyf - range.FirstGlyf);
            }

            return '\0';
        }

        internal string GetLigature(ushort glyf)
        {
            if (m_ligatures == null)
                return null;
            string val;
            if (m_ligatures.TryGetValue(glyf, out val))
                return val;
            return null;
        }

        private void parse(Lexer lexer)
        {
            lexer.LastParsedByte = lexer.ReadByte();
            for (; ; )
            {
                if(Lexer.IsEOL(lexer.LastParsedByte))
                    lexer.SkipEOL();
                if (lexer.LastParsedByte == -1)
                    return;

                lexer.ReadLexemeWithLastParsedByte();
                if (lexer.CurrentLexemeEquals(Beginbfrange))
                    loadRange(lexer);
                else if (lexer.CurrentLexemeEquals(Beginbfchar))
                    loadChar(lexer);
            }
        }

        private int getChar(Lexer lexer)
        {
            if (lexer.LastParsedByte != '<')
                return -1;

            int count = lexer.ReadHexValue();
            if (Lexer.IsEOL(lexer.LastParsedByte))
                lexer.SkipEOL();

            if (count == 1)
                return lexer.GetLexemeHexByte();
            else if (count == 2)
                return lexer.GetLexemeHex2Bytes();
            return -1;
        }

        private int getLigature(Lexer lexer, out string value)
        {
            value = null;
            if (lexer.LastParsedByte != '<')
                return -1;

            int count = lexer.ReadHexValue();
            if (Lexer.IsEOL(lexer.LastParsedByte))
                lexer.SkipEOL();

            if (count == 1)
                return lexer.GetLexemeHexByte();
            else if (count == 2)
                return lexer.GetLexemeHex2Bytes();
            if (count % 2 != 0)
                return -1;
            value = lexer.GetLexemeHexLigature();
            return 0;
        }

        private void loadRange(Lexer lexer)
        {
            if (Lexer.IsEOL(lexer.LastParsedByte))
                lexer.SkipEOL();
            for (; ; )
            {
                if (lexer.LastParsedByte == 'e')
                    return;

                int first = getChar(lexer);
                if (first < 0)
                    return;

                int second = getChar(lexer);
                if (second < 0)
                    return;

                if (lexer.LastParsedByte == '[')
                {
                    string tmp;
                    lexer.SkipEOL();
                    for (; ; )
                    {
                        if (lexer.LastParsedByte == ']')
                        {
                            lexer.SkipEOL();
                            break;
                        }

                        int code = getLigature(lexer, out tmp);
                        if (code < 0)
                            return;
                        if (tmp != null)
                        {
                            initLigatures();
                            m_ligatures[(ushort)code] = tmp;
                        }
                        else
                        {
                            m_chars.Add(new BfChar((char)code, (ushort)first));
                        }
                        ++first;
                    }
                }
                else
                {
                    int value = getChar(lexer);
                    if (value < 0)
                        return;
                    m_ranges.Add(new BfRange((char)value, (char)(value + second - first), (ushort)first, (ushort)second));
                }
            }
        }

        private void loadChar(Lexer lexer)
        {
            if (Lexer.IsEOL(lexer.LastParsedByte))
                lexer.SkipEOL();

            string tmp;
            for (; ; )
            {
                if (lexer.LastParsedByte == 'e')
                    return;

                int code = getChar(lexer);
                if (code < 0)
                    return;

                int value = getLigature(lexer, out tmp);
                if (value < 0)
                    return;

                if (tmp != null)
                {
                    initLigatures();
                    m_ligatures[(ushort)code] = tmp;
                }
                else
                {
                    m_chars.Add(new BfChar((char)value, (ushort)code));
                }
            }
        }

        private void initLigatures()
        {
            if (m_ligatures == null)
                m_ligatures = new Dictionary<ushort, string>();
        }

        private List<BfChar> m_chars;
        private List<BfRange> m_ranges;
        private Dictionary<ushort, string> m_ligatures;

        private static string Beginbfrange = "beginbfrange";
        private static string Beginbfchar = "beginbfchar";

        private struct BfChar
        {
            public BfChar(char c, ushort glyf)
            {
                Char = c;
                Glyf = glyf;
            }

            public char Char;
            public ushort Glyf;
        }

        private struct BfRange
        {
            public BfRange(char firstChar, char endChar, ushort firstGlyf, ushort endGlyf)
            {
                FirstChar = firstChar;
                EndChar = endChar;
                FirstGlyf = firstGlyf;
                EndGlyf = endGlyf;
            }

            public char FirstChar;
            public char EndChar;
            public ushort FirstGlyf;
            public ushort EndGlyf;
        }
    }
}
