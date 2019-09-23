using System.Collections.Generic;
using System.IO;

namespace Bytescout.PDF
{
    internal static class FontsManager
    {
	    private static List<Base14Font> _standard = new List<Base14Font>();
	    private static List<FontBase> _fromFile = new List<FontBase>();
	    private static Dictionary<LOGFONT, FontBase> _fonts = new Dictionary<LOGFONT, FontBase>();

	    public static FontBase AddFont(string fontName, bool bold, bool italic)
        {
            LOGFONT lf = FontDataLoader.GetLOGFONT(fontName, bold, italic);
            if (_fonts.ContainsKey(lf))
                return _fonts[lf];

            uint ttc;
            byte[] data = FontDataLoader.LoadFont(lf, out ttc);
            FontBase font = loadFromBuffer(data, ttc);
            _fonts.Add(lf, font);

            return font;
        }

        public static FontBase AddStandardFont(StandardFonts builtFont, bool underline, bool strikeout)
        {
            for (int i = 0; i < _standard.Count; ++i)
            {
                if (_standard[i].BuiltInFontType == builtFont)
                    return _standard[i];
            }

            Base14Font pdfFont = new Base14Font(builtFont);
            _standard.Add(pdfFont);
            return pdfFont;
        }

        public static FontBase AddFontFromFile(string fileName)
        {
            byte[] buf = FontDataLoader.LoadFontFromFile(fileName);
            FontBase font = loadFromBuffer(buf, 0);
            _fromFile.Add(font);
            return font;
        }

        public static FontBase AddFontFromStream(Stream stream)
        {
            byte[] buf = new byte[stream.Length];
            stream.Position = 0;
            FontBase font = loadFromBuffer(buf, 0);
            _fromFile.Add(font);
            return font;
        }

        public static void Release()
        {
            for (int i = 0; i < _fromFile.Count; ++i)
                _fromFile[i].CreateFontDictionary();

            FontBase[] fonts = new FontBase[_fonts.Count];
            _fonts.Values.CopyTo(fonts, 0);
            for (int i = 0; i < fonts.Length; ++i)
                fonts[i].CreateFontDictionary();
        }

        private static FontBase loadFromBuffer(byte[] buf, uint ttcSize)
        {
            if (TrueTypeFontFile.IsTrueTypeFont(buf) || TrueTypeFontFile.IsOpenTypeFont(buf))
            {
                FontBase font = new TrueTypeFont(buf, false, false, ttcSize);
                return font;
            }
            if (Type1Parser.IsType1Font(buf))
            {
                FontBase font = new Type1Font(buf);
                return font;
            }

            throw new PDFUnsupportFontFormatException();
        }
    }
}
