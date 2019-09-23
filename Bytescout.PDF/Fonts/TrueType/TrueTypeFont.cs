using System.Text;

namespace Bytescout.PDF
{
    internal class TrueTypeFont : FontBase
    {
	    private readonly TrueTypeFontData _trueTypeData;
	    private readonly FontMap _fontMap;

	    internal override string Name
        {
            get
            {
                return _trueTypeData.FontName;
            }
        }

        internal override System.Drawing.Rectangle FontBBox
        {
            get { return _trueTypeData.FontBBox; }
        }

        internal override bool RealBold { get { return _trueTypeData.Bold; } }

        internal override bool RealItalic { get { return _trueTypeData.ItalicAngle != 0; } }

	    public TrueTypeFont(byte[] buffer, bool bold, bool italic, uint ttcSize)
	    {
		    if (TrueTypeFontFile.IsTrueTypeFont(buffer))
			    _trueTypeData = new TrueTypeFontData(buffer, ttcSize);
		    else if (TrueTypeFontFile.IsOpenTypeFont(buffer))
			    _trueTypeData = new OpenTypeFontData(buffer);
            
		    _fontMap = new FontMap();
		    GetDictionary().Tag = this;
	    }

	    private IPDFObject createToUnicode()
	    {
		    StringBuilder result = new StringBuilder();
		    result.Append("/CIDInit /ProcSet findresource begin\r");
		    result.Append("12 dict begin\r");
		    result.Append("begincmap\r");
		    result.Append("/CIDSystemInfo\r");
		    result.Append("<</Registry (Adobe)\r");
		    result.Append("/Ordering (UCS)\r");
		    result.Append("/Supplement 0\r>> def\r\r");
		    result.Append("/CMapName /Adobe-Identity-UCS def\r");
		    result.Append("/CMapType 2 def\r\r");
		    result.Append("1 begincodespacerange\r");
		    result.Append("<0000><FFFF>\r");
		    result.Append("endcodespacerange\r\r");

		    result.Append(_fontMap.Characters.Count.ToString() + " beginbfchar\r");
		    for (int i = 0; i < _fontMap.Characters.Count; ++i)
		    {
			    result.Append("<");
			    result.Append(StringUtility.UShortToHexString(_fontMap.GetGlyfIndex(_fontMap.Characters[i])));
			    result.Append(">");
			    result.Append("<");
			    result.Append(StringUtility.UShortToHexString((ushort)_fontMap.Characters[i]));
			    result.Append(">");
			    result.Append("\r");
		    }

		    result.Append("endbfchar\r\r");
		    result.Append("endcmap\r");
		    result.Append("CMapName currentdict /CMap defineresource pop\r");
		    result.Append("end\rend");

		    System.IO.MemoryStream stream = new System.IO.MemoryStream();
		    byte[] buf = System.Text.Encoding.ASCII.GetBytes(result.ToString());
		    stream.Write(buf, 0, buf.Length);

		    PDFDictionary dict = new PDFDictionary();
		    return new PDFDictionaryStream(dict, stream); ;
	    }

	    internal override PDFString ConvertStringToFontEncoding(string str)
        {
            return _fontMap.Convert(str);
        }

        internal override string ConvertFromFontEncoding(PDFString str)
        {
            return _fontMap.ConvertFrom(str);
        }

        internal override float GetTextHeight(float fontSize)
        {
            float bboxHeight = _trueTypeData.Ascent;
            float textHeight = bboxHeight * fontSize / 1000.0f;
            return textHeight;
        }

        internal override float GetTextWidth(string text, float fontSize)
        {
            float result = 0.0f;
            for (int i = 0; i < text.Length; ++i)
                result += _trueTypeData.GetCharWidth(text[i]);
            return result * fontSize / 1000.0f;
        }

        internal override float GetTextWidth(PDFString str, float fontSize)
        {
            float result = 0.0f;
            byte[] data = str.GetBytes();
            for (int i = 0; i < data.Length; i += 2)
            {
                ushort glyf = (ushort)(data[i] * 256 + data[i + 1]);
                result += _trueTypeData.GetWidthOfGlyph(glyf);
            }

            return result * fontSize / 1000.0f;
        }

        internal override float GetCharWidth(char c, float fontSize)
        {
            float result = _trueTypeData.GetCharWidth(c);
            return result * fontSize / 1000.0f;
        }

        internal override bool Contains(char c)
        {
            return _fontMap.Contains(c);
        }

        internal override void CreateFontDictionary()
        {
            PDFDictionary dict = GetDictionary();
            dict.AddItem("BaseFont", new PDFName(_trueTypeData.FontName));
            dict.AddItem("Encoding", new PDFName("Identity-H"));
            dict.AddItem("Type", new PDFName("Font"));
            dict.AddItem("Subtype", new PDFName("Type0"));
            dict.AddItem("DescendantFonts", createDescendantFonts());
            dict.AddItem("ToUnicode", createToUnicode());
        }

        internal override void AddStringToEncoding(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (!_fontMap.Contains(str[i]))
                {
                    ushort[] indexes = _trueTypeData.GetUsedGlyphs(str[i]);
                    if (indexes != null)
                    {
                        _fontMap.Add(str[i], indexes[0]);
                        for (int j = 1; j < indexes.Length; ++j)
                            _fontMap.AddToAdditional(indexes[j]);
                    }
                }
            }
        }

        private IPDFObject createGlyfWidths()
        {
            PDFArray widths = new PDFArray();

            for (int i = 0; i < _fontMap.Characters.Count; ++i)
            {
                char ch = _fontMap.Characters[i];
                int index = _fontMap.GetGlyfIndex(ch);
                widths.AddItem(new PDFNumber(index));

                PDFArray arr = new PDFArray();
                arr.AddItem(new PDFNumber(_trueTypeData.GetWidthOfGlyph(index)));

                widths.AddItem(arr);
            }

            return widths;
        }

        private IPDFObject createDescendantFonts()
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("BaseFont", new PDFName(_trueTypeData.FontName));

            PDFDictionary CIDSystemInfo = new PDFDictionary();
            CIDSystemInfo.AddItem("Ordering", new PDFString(System.Text.Encoding.ASCII.GetBytes("Identity"), false));
            CIDSystemInfo.AddItem("Registry", new PDFString(System.Text.Encoding.ASCII.GetBytes("Adobe"), false));
            CIDSystemInfo.AddItem("Supplement", new PDFNumber(0));

            dict.AddItem("CIDSystemInfo", CIDSystemInfo);
            dict.AddItem("CIDToGIDMap", new PDFName("Identity"));
            dict.AddItem("DW", new PDFNumber(_trueTypeData.MissingWidth));
            dict.AddItem("FontDescriptor", createFontDescriptorDictionary());
            dict.AddItem("Type", new PDFName("Font"));
            dict.AddItem("Subtype", new PDFName("CIDFontType2"));

            dict.AddItem("W", createGlyfWidths());

            PDFArray arr = new PDFArray();
            arr.AddItem(dict);

            return arr;
        }

        private PDFDictionary createFontDescriptorDictionary()
        {
            PDFDictionary dict = new PDFDictionary();

            dict.AddItem("Ascent", new PDFNumber(_trueTypeData.Ascent));
            dict.AddItem("Descent", new PDFNumber(_trueTypeData.Descent));
            dict.AddItem("CapHeight", new PDFNumber(0));
            dict.AddItem("Flags", new PDFNumber(32));
            dict.AddItem("FontName", new PDFName(_trueTypeData.FontName));
            dict.AddItem("ItalicAngle", new PDFNumber(_trueTypeData.ItalicAngle));
            dict.AddItem("MissingWidth", new PDFNumber(_trueTypeData.MissingWidth));
            dict.AddItem("StemV", new PDFNumber(0));
            dict.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            System.Drawing.Rectangle rect = _trueTypeData.FontBBox;
            bbox.AddItem(new PDFNumber(rect.X));
            bbox.AddItem(new PDFNumber(rect.Y));
            bbox.AddItem(new PDFNumber(rect.Width));
            bbox.AddItem(new PDFNumber(rect.Height));
            dict.AddItem("FontBBox", bbox);

            PDFDictionary fontFileDict = new PDFDictionary();

            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            _trueTypeData.Write(stream, _fontMap.GetIndexes(), _fontMap);

            fontFileDict.AddItem("Length1", new PDFNumber(stream.Length));
            dict.AddItem("FontFile2", new PDFDictionaryStream(fontFileDict, stream));

            return dict;
        }
    }
}
