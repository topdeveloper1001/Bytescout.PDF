using System.Text;

namespace Bytescout.PDF
{
    internal class ParsedFont: FontBase
    {
        internal ParsedFont(PDFDictionary dict)
            : base(dict)
        {
            GetDictionary().Tag = this;
        }

        internal override string Name
        {
            get 
            {
                PDFName name = GetDictionary()["BaseFont"] as PDFName;
                if (name == null)
                    return "";
                return name.GetValue();
            }
        }

        internal override bool RealBold { get { return false; } }

        internal override bool RealItalic { get { return false; } }

        internal override System.Drawing.Rectangle FontBBox
        {
            get { return getFontBBox(); }
        }

        internal override void AddStringToEncoding(string str)
        {
        }

        internal override PDFString ConvertStringToFontEncoding(string str)
        {
            if (string.IsNullOrEmpty(str))
                return new PDFString(new byte[0], false);
            
            byte[] result = new byte[str.Length];
            for (int i = 0; i < str.Length; ++i)
                result[i] = getGlyf(str[i]);

            return new PDFString(result, false);
        }

        internal override bool Contains(char c)
        {
            return getGlyf(c) != 0;
        }
        
        internal override string ConvertFromFontEncoding(PDFString str)
        {
            if (str == null)
                return "";

            loadCmappers();
            byte[] data = str.GetBytes();
            StringBuilder result = new StringBuilder(data.Length);

            char tmp;
            for (int i = 0; i < data.Length; ++i)
            {
                tmp = m_toUnicode.GetChar(data[i]);
                if (tmp == 0)
                {
                    string s = m_toUnicode.GetLigature(data[i]);
                    if (s == null)
                    {
                        tmp = m_encoding.GetChar(data[i]);
                        if (tmp != 0)
                            result.Append(tmp);
                    }
                    else
                        result.Append(s);
                }
                else
                    result.Append(tmp);
            }

            return result.ToString();
        }

        internal override void CreateFontDictionary()
        {
        }

        internal override float GetTextHeight(float fontSize)
        {
            float bboxHeight = FontBBox.Height;
            float textHeight = bboxHeight * fontSize / 1000.0f;
            return textHeight;
        }

        internal override float GetTextWidth(string text, float fontSize)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            float result = 0;
            for (int i = 0; i < text.Length; ++i)
                result += GetCharWidth(text[i], fontSize);

            return result;
        }

        internal override float GetTextWidth(PDFString str, float fontSize)
        {
            PDFArray widths = GetDictionary()["Widths"] as PDFArray;
            byte[] data = str.GetBytes();
            float result = 0;

            if (widths != null)
            {
                PDFNumber fc = GetDictionary()["FirstChar"] as PDFNumber;
                int firstChar = 0;
                if (fc != null)
                    firstChar = (int)fc.GetValue();
                
                for (int i = 0; i < data.Length; ++i)
                {
                    PDFNumber w = widths[data[i] - firstChar] as PDFNumber;
                    if (w != null)
                        result += (float)w.GetValue();
                }

                return result * fontSize / 1000.0f;
            }

            StandardFonts font;
            if (isStandardFont(out font))
            {
                IGlyfMetrics metrics = getStandardFontMetrics(font);
                string tmp = ConvertFromFontEncoding(str);
                for (int i = 0; i < tmp.Length; ++i)
                    result += metrics.GetCharWidth(tmp[i]);
            }

            return result * fontSize / 1000.0f;
        }

        internal override float GetCharWidth(char c, float fontSize)
        {
            PDFArray widths = GetDictionary()["Widths"] as PDFArray;
            if (widths != null)
            {
                PDFNumber fc = GetDictionary()["FirstChar"] as PDFNumber;
                int firstChar = 0;
                if (fc != null)
                    firstChar = (int)fc.GetValue();

                int glyf = getGlyf(c);
                PDFNumber w = widths[glyf - firstChar] as PDFNumber;
                if (w != null)
                    return (float)(w.GetValue() * fontSize / 1000.0f);
            }
            else
            {
                StandardFonts font;
                if (isStandardFont(out font))
                {
                    IGlyfMetrics metrics = getStandardFontMetrics(font);
                    return metrics.GetCharWidth(c) * fontSize / 1000.0f;
                }
            }
            return 0;
        }

        private System.Drawing.Rectangle getFontBBox()
        {
            PDFDictionary fontDescriptor = GetDictionary()["FontDescriptor"] as PDFDictionary;
            if (fontDescriptor != null)
            {
                PDFArray fontBBox = fontDescriptor["FontBBox"] as PDFArray;
                if (fontBBox != null)
                {
                    System.Drawing.Rectangle bbox = new System.Drawing.Rectangle();
                    try
                    {
                        bbox.X = (int)(fontBBox[0] as PDFNumber).GetValue();
                        bbox.Y = (int)(fontBBox[1] as PDFNumber).GetValue();
                        bbox.Width = (int)(fontBBox[2] as PDFNumber).GetValue();
                        bbox.Height = (int)(fontBBox[3] as PDFNumber).GetValue();
                    }
                    catch
                    {
                    }
                    return bbox;
                }
            }
            
            StandardFonts bf;
            if (isStandardFont(out bf))
                return Base14Font.GetFontBBox(bf);
            return new System.Drawing.Rectangle();
        }

        private bool isStandardFont(out StandardFonts fontType)
        {
            string name = Name.ToLower();

            if (name.Contains("symbol"))
            {
                fontType = StandardFonts.Symbol;
                return true;
            }
            else if (name.Contains("zapfdingbats"))
            {
                fontType = StandardFonts.ZapfDingbats;
                return true;
            }

            bool bold = name.Contains("bold");
            bool italic = false;
            if (name.Contains("oblique") || name.Contains("italic"))
                italic = true;

            if (name.Contains("courier"))
            {
                if (bold && italic)
                {
                    fontType = StandardFonts.CourierBoldOblique;
                    return true;
                }
                if (bold)
                {
                    fontType = StandardFonts.CourierBold;
                    return true;
                }
                if (italic)
                {
                    fontType = StandardFonts.CourierOblique;
                    return true;
                }
                fontType = StandardFonts.Courier;
                return true;
            }
            else if (name.Contains("helvetica"))
            {
                if (bold && italic)
                {
                    fontType = StandardFonts.HelveticaBoldOblique;
                    return true;
                }
                if (bold)
                {
                    fontType = StandardFonts.HelveticaBold;
                    return true;
                }
                if (italic)
                {
                    fontType = StandardFonts.HelveticaOblique;
                    return true;
                }
                fontType = StandardFonts.Helvetica;
                return true;
            }
            else if (name.Contains("times"))
            {
                if (bold && italic)
                {
                    fontType = StandardFonts.TimesBoldItalic;
                    return true;
                }
                if (bold)
                {
                    fontType = StandardFonts.TimesBold;
                    return true;
                }
                if (italic)
                {
                    fontType = StandardFonts.TimesItalic;
                    return true;
                }
                fontType = StandardFonts.Times;
                return true;
            }
            fontType = StandardFonts.Helvetica;
            return false;
        }

        private IGlyfMetrics getStandardFontMetrics(StandardFonts font)
        {
            switch (font)
            {
                case StandardFonts.Courier:
                    return StandardFontGlyfData.Courier;
                case StandardFonts.CourierBold:
                    return StandardFontGlyfData.CourierBold;
                case StandardFonts.CourierBoldOblique:
                    return StandardFontGlyfData.CourierBoldOblique;
                case StandardFonts.CourierOblique:
                    return StandardFontGlyfData.CourierOblique;
                case StandardFonts.Helvetica:
                    return StandardFontGlyfData.Helvetica;
                case StandardFonts.HelveticaBold:
                    return StandardFontGlyfData.HelveticaBold;
                case StandardFonts.HelveticaBoldOblique:
                    return StandardFontGlyfData.HelveticaBoldOblique;
                case StandardFonts.HelveticaOblique:
                    return StandardFontGlyfData.HelveticaOblique;
                case StandardFonts.Symbol:
                    return StandardFontGlyfData.Symbol;
                case StandardFonts.Times:
                    return StandardFontGlyfData.Times;
                case StandardFonts.TimesBold:
                    return StandardFontGlyfData.TimesBold;
                case StandardFonts.TimesBoldItalic:
                    return StandardFontGlyfData.TimesBoldItalic;
                case StandardFonts.TimesItalic:
                    return StandardFontGlyfData.TimesItalic;
            }
            return StandardFontGlyfData.Helvetica;
        }

        private void loadCmappers()
        {
            if (m_encoding == null)
            {
                m_encoding = new FontEncoding(GetDictionary()["Encoding"]);
                m_toUnicode = new ToUnicode(GetDictionary()["ToUnicode"] as PDFDictionaryStream);
            }
        }

        private byte getGlyf(char c)
        {
            loadCmappers();
            ushort tmp = m_toUnicode.GetGlyf(c);
            if (tmp == 0)
                tmp = m_encoding.GetGlyf(c);
            return (byte)tmp;
        }

        private FontEncoding m_encoding;
        private ToUnicode m_toUnicode;
    }
}
