using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Bytescout.PDF
{
    internal class Type1Font: FontBase
    {
	    private readonly Dictionary<char, byte> _charset = new Dictionary<char, byte>();
	    private FontEncoding _encoding;

	    internal override string Name
	    {
		    get
		    {
			    PDFName baseFont = GetDictionary()["BaseFont"] as PDFName;
			    return baseFont.GetValue();
		    }
	    }

	    internal override bool RealBold
	    {
		    get { return false; }
	    }

	    internal override bool RealItalic
	    {
		    get
		    {
			    PDFNumber italic = (GetDictionary()["FontDescriptor"] as PDFDictionary)["ItalicAngle"] as PDFNumber;
			    return italic.GetValue() != 0;
		    }
	    }

	    internal override System.Drawing.Rectangle FontBBox
	    {
		    get
		    {
			    PDFArray arr = (GetDictionary()["FontDescriptor"] as PDFDictionary)["FontBBox"] as PDFArray;
			    int l, t, w, h;
			    l = (int)(arr[0] as PDFNumber).GetValue();
			    t = (int)(arr[1] as PDFNumber).GetValue();
			    w = (int)(arr[2] as PDFNumber).GetValue();
			    h = (int)(arr[3] as PDFNumber).GetValue();

			    return new System.Drawing.Rectangle(l, t, w, h);
		    }
	    }

	    internal Type1Font(byte[] buffer)
        {
            Type1Parser parser = new Type1Parser(new Reader(buffer));
            initialize(parser);
            initFontEncoding(parser);

            PDFDictionaryStream fontFile = new PDFDictionaryStream();
            fontFile.Dictionary.AddItem("Length1", new PDFNumber(buffer.Length));
            fontFile.Dictionary.AddItem("Length2", new PDFNumber(0));
            fontFile.Dictionary.AddItem("Length3", new PDFNumber(0));
            Stream stream = fontFile.GetStream();
            stream.Position = 0;
            stream.Write(buffer, 0, buffer.Length);

            ((PDFDictionary) GetDictionary()["FontDescriptor"]).AddItem("FontFile", fontFile);

            GetDictionary().Tag = this;
        }

        internal override void AddStringToEncoding(string str)
        {
        }

	    internal override bool Contains(char c)
        {
            return _charset.ContainsKey(c);
        }

        internal override float GetCharWidth(char c, float fontSize)
        {
            byte glyf = 0;
            if (!_charset.TryGetValue(c, out glyf))
                return 0;

            PDFNumber firstChar = GetDictionary()["FirstChar"] as PDFNumber;
            int f = (int)firstChar.GetValue();

            PDFArray widths = GetDictionary()["Widths"] as PDFArray;
            PDFNumber num = widths[glyf - f] as PDFNumber;
            if (num != null)
                return (float)(num.GetValue() * fontSize / 1000.0f);

            return 0;
        }

        internal override float GetTextHeight(float fontSize)
        {
            float bboxHeight = FontBBox.Height;
            float textHeight = bboxHeight * fontSize / 1000.0f;
            return textHeight;
        }

        internal override float GetTextWidth(string text, float fontSize)
        {
            float result = 0;
            for (int i = 0; i < text.Length; ++i)
                result += GetCharWidth(text[i], fontSize);
            return result;
        }

        internal override float GetTextWidth(PDFString str, float fontSize)
        {
            PDFArray widths = GetDictionary()["Widths"] as PDFArray;
            PDFNumber fc = GetDictionary()["FirstChar"] as PDFNumber;
            int firstChar = 0;
            if (fc != null)
                firstChar = (int)fc.GetValue();

            byte[] data = str.GetBytes();
            float result = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                PDFNumber w = widths[data[i] - firstChar] as PDFNumber;
                if (w != null)
                    return (float)(w.GetValue() * fontSize / 1000.0f);
            }

            return result;
        }

        internal override PDFString ConvertStringToFontEncoding(string str)
        {
            if (string.IsNullOrEmpty(str))
                return new PDFString(new byte[0], false);

            byte[] data = new byte[str.Length];
            for (int i = 0; i < str.Length; ++i)
            {
                byte glyf;
                if (_charset.TryGetValue(str[i], out glyf))
                    data[i] = glyf;
            }

            return new PDFString(data, false);
        }

        internal override string ConvertFromFontEncoding(PDFString str)
        {
            if (str == null)
                return "";

            if (_encoding == null)
                _encoding = new FontEncoding(GetDictionary()["Encoding"]);

            if (str == null)
                return "";

            StringBuilder result = new StringBuilder();
            byte[] data = str.GetBytes();
            for (int i = 0; i < data.Length; ++i)
                result.Append(_encoding.GetChar(data[i]));
            
            return result.ToString();
        }

        internal override void CreateFontDictionary()
        {
        }

        private void initialize(Type1Parser parser)
        {
            PDFDictionary dict = GetDictionary();
            dict.AddItem("Type", new PDFName("Font"));
            dict.AddItem("Subtype", new PDFName("Type1"));
            dict.AddItem("BaseFont", new PDFName(parser.FontName));

            addEncoding(parser);
            addWidths(parser);
            addFontDescriptor(parser);
        }

        private void addFontDescriptor(Type1Parser parser)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Ascent", new PDFNumber(parser.FontBBox.Height));
            dict.AddItem("Descent", new PDFNumber(parser.FontBBox.Y));
            dict.AddItem("CapHeight", new PDFNumber(0));
            dict.AddItem("Flags", new PDFNumber(32));
            dict.AddItem("FontName", new PDFName(parser.FontName));
            dict.AddItem("ItalicAngle", new PDFNumber(parser.ItalicAngle));
            dict.AddItem("StemV", new PDFNumber(0));
            dict.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            System.Drawing.Rectangle rect = parser.FontBBox;
            bbox.AddItem(new PDFNumber(rect.X));
            bbox.AddItem(new PDFNumber(rect.Y));
            bbox.AddItem(new PDFNumber(rect.Width));
            bbox.AddItem(new PDFNumber(rect.Height));
            dict.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", dict);
        }

        private void addWidths(Type1Parser parser)
        {
            List<KeyValuePair<ushort, string>> charset = parser.GetCharset();
            Dictionary<string, int> widths = parser.GetWidths();
            PDFArray w = new PDFArray();

            if (charset != null)
            {
                int i = 0;
                if (charset[0].Key == 0)
                    ++i;

                int tmp = 0;
                for (; i < charset.Count; ++i)
                {
                    if (!widths.TryGetValue(charset[i].Value, out tmp))
                        w.AddItem(new PDFNumber(0));
                    else
                        w.AddItem(new PDFNumber(tmp));
                }
            }
            else
            {
                int result = 0;
                for (byte i = 32; i < 255; ++i)
                {
                    char c = Encoding.GetChar(i);
                    string glyfName = GlyfNames.GetName(c);
                    if (!widths.TryGetValue(glyfName, out result))
                        w.AddItem(new PDFNumber(0));
                    else
                        w.AddItem(new PDFNumber(result));
                }
            }

            GetDictionary().AddItem("Widths", w);
        }

        private void addEncoding(Type1Parser parser)
        {
            List<KeyValuePair<ushort, string>> charset = parser.GetCharset();
            if (charset == null)
                GetDictionary().AddItem("Encoding", new PDFName(parser.Encoding));
            else
            {
                PDFDictionary encoding = new PDFDictionary();
                encoding.AddItem("Type", new PDFName("Encoding"));
                PDFArray differences = new PDFArray();

                for (int i = 0; i < charset.Count; ++i)
                {
                    differences.AddItem(new PDFNumber(charset[i].Key));
                    differences.AddItem(new PDFName(charset[i].Value));
                }
                encoding.AddItem("Differences", differences);
                GetDictionary().AddItem("Encoding", encoding);
            }

            if (charset == null)
            {
                GetDictionary().AddItem("FirstChar", new PDFNumber(32));
                GetDictionary().AddItem("LastChar", new PDFNumber(255));
            }
            else
            {
                int firstChar = charset[0].Key;
                if (firstChar == 0)
                    firstChar = charset[1].Key;
                GetDictionary().AddItem("FirstChar", new PDFNumber(firstChar));
                GetDictionary().AddItem("LastChar", new PDFNumber(charset[charset.Count - 1].Key));
            }
        }

        private void initFontEncoding(Type1Parser parser)
        {
            List<KeyValuePair<ushort, string>> charset = parser.GetCharset();
            if (charset != null)
            {
                for (int i = 0; i < charset.Count; ++i)
                {
                    char c = GlyfNames.GetChar(charset[i].Value);
                    if (!_charset.ContainsKey(c))
                        _charset.Add(c, (byte)charset[i].Key);
                }
            }
            else
            {
                for (byte i = 32; i < 255; ++i)
                {
                    char ch = Encoding.GetChar(i);
                    if (!_charset.ContainsKey(ch))
                        _charset.Add(ch, i);
                }
            }
        }
    }
}
