using System.Text;

namespace Bytescout.PDF
{
    internal class Type0Font : FontBase
    {
	    private ToUnicode _toUnicode;
	    private CIDToGIDMap _cidToGIDMap;

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

	    internal Type0Font(PDFDictionary dict)
		    : base(dict)
	    {
		    GetDictionary().Tag = this;
	    }

	    internal override float GetCharWidth(char c, float fontSize)
        {
            ushort glyf = getGlyf(c);
            return getGlyfWidth(glyf) * fontSize / 1000.0f;
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
            byte[] data = str.GetBytes();
            float result = 0;
            for (int i = 0; i < data.Length - 1; i += 2)
            {
                ushort glyf = (ushort)(data[i] * 256 + data[i + 1]);
                result += getGlyfWidth(glyf);
            }

            return result * fontSize / 1000.0f;
        }

        internal override void AddStringToEncoding(string str)
        {
        }

        internal override PDFString ConvertStringToFontEncoding(string str)
        {
            if (string.IsNullOrEmpty(str))
                return new PDFString(new byte[0], false);
            
            ushort glyf;
            byte[] result = new byte[str.Length * 2];
            for (int i = 0; i < str.Length; ++i)
            {
                glyf = getGlyf(str[i]);
                result[i * 2] = (byte)(glyf / 256);
                result[i * 2 + 1] = (byte)(glyf - result[i * 2] * 256);
            }

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

            loadCmapper();
            byte[] data = str.GetBytes();
            StringBuilder result = new StringBuilder(data.Length / 2 + 1);
            ushort glyf;
            char tmp;

            for (int i = 0; i < data.Length - 1; i += 2)
            {
                glyf = (ushort)(data[i] * 256 + data[i + 1]);
                tmp = _toUnicode.GetChar(glyf);
                if (tmp == 0)
                {
                    string s = _toUnicode.GetLigature(glyf);
                    if (s == null)
                    {
                        tmp = _cidToGIDMap.GetChar(glyf);
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

        private System.Drawing.Rectangle getFontBBox()
        {
            try
            {
                PDFArray descendantFonts = GetDictionary()["DescendantFonts"] as PDFArray;
                PDFDictionary font = descendantFonts[0] as PDFDictionary;
                PDFDictionary fontDescriptor = font["FontDescriptor"] as PDFDictionary;
                PDFArray fontBBox = fontDescriptor["FontBBox"] as PDFArray;
                System.Drawing.Rectangle bbox = new System.Drawing.Rectangle();
                bbox.X = (int)(fontBBox[0] as PDFNumber).GetValue();
                bbox.Y = (int)(fontBBox[1] as PDFNumber).GetValue();
                bbox.Width = (int)(fontBBox[2] as PDFNumber).GetValue();
                bbox.Height = (int)(fontBBox[3] as PDFNumber).GetValue();
                return bbox;
            }
            catch
            {
            }
            return new System.Drawing.Rectangle();
        }

        private PDFArray getWidthsArray()
        {
            PDFArray descendantFonts = GetDictionary()["DescendantFonts"] as PDFArray;
            if (descendantFonts != null)
            {
                PDFDictionary font = descendantFonts[0] as PDFDictionary;
                if (font != null)
                    return font["W"] as PDFArray;
                
            }
            return null;
        }

        private int getDefaultCharWidth()
        {
            PDFNumber dw = GetDictionary()["DW"] as PDFNumber;
            if (dw != null)
                return (int)dw.GetValue();

            return 1000;
        }

        private int getGlyfWidth(ushort glyf)
        {
            PDFArray w = getWidthsArray();
            int dw = getDefaultCharWidth();
            if (w == null)
                return dw;

            for (int i = 0; i < w.Count; ++i)
            {
                PDFNumber first = w[i] as PDFNumber;
                if (first != null)
                {
                    if (first.GetValue() > glyf)
                        return dw;

                    if (first != null)
                    {
                        IPDFObject second = w[i + 1];
                        if (second is PDFNumber)
                        {
                            if (first.GetValue() <= glyf && glyf <= (second as PDFNumber).GetValue())
                            {
                                PDFNumber value = w[i + 2] as PDFNumber;
                                if (value != null)
                                    return (int)value.GetValue();
                                return dw;
                            }
                            else
                                i += 2;
                        }
                        else if (second is PDFArray)
                        {
                            int count = (second as PDFArray).Count;
                            if (first.GetValue() <= glyf && glyf <= first.GetValue() + count)
                            {
                                int index = glyf - (int)first.GetValue();
                                PDFNumber width = (second as PDFArray)[index] as PDFNumber;
                                if (width != null)
                                    return (int)width.GetValue();
                                return dw;
                            }
                            else
                                ++i;
                        }
                    }
                }
            }

            return dw;
        }

        private void loadCmapper()
        {
            if (_toUnicode == null)
            {
                _toUnicode = new ToUnicode(GetDictionary()["ToUnicode"] as PDFDictionaryStream);

                PDFArray descendantFonts = GetDictionary()["DescendantFonts"] as PDFArray;
                if (descendantFonts != null)
                {
                    PDFDictionary font = descendantFonts[0] as PDFDictionary;
                    if (font != null)
                        _cidToGIDMap = new CIDToGIDMap(font["CIDToGIDMap"]);
                }

                if (_cidToGIDMap == null)
                    _cidToGIDMap = new CIDToGIDMap(null);
            }
        }

        private ushort getGlyf(char c)
        {
            loadCmapper();
            ushort glyf = _toUnicode.GetGlyf(c);
            if (glyf == 0)
                glyf = _cidToGIDMap.GetGlyf(c);
            return glyf;
        }
    }
}
