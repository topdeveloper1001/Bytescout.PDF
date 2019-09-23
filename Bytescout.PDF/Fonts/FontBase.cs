namespace Bytescout.PDF
{
    internal abstract class FontBase
    {
	    private readonly PDFDictionary _fontDictionary;

	    internal abstract string Name { get; }

	    internal abstract bool RealBold { get; }

	    internal abstract bool RealItalic { get; }

	    internal abstract System.Drawing.Rectangle FontBBox { get; }

	    internal FontBase()
        {
            _fontDictionary = new PDFDictionary();
        }

        internal FontBase(PDFDictionary dict)
        {
            _fontDictionary = dict;
        }

        internal static FontBase Instance(PDFDictionary dict)
        {
            if (dict.Tag is FontBase)
                return dict.Tag as FontBase;

            PDFName type = dict["Subtype"] as PDFName;
            if (type != null && type.GetValue() == "Type0")
                return new Type0Font(dict);
            else
                return new ParsedFont(dict);
        }

	    internal abstract float GetTextHeight(float fontSize);

        internal abstract float GetTextWidth(string text, float fontSize);

        internal abstract float GetTextWidth(PDFString str, float fontSize);

        internal abstract float GetCharWidth(char c, float fontSize);

        internal abstract bool Contains(char c);

        internal PDFDictionary GetDictionary()
        {
            return _fontDictionary;
        }

        internal abstract void CreateFontDictionary();

        internal abstract void AddStringToEncoding(string str);

        internal abstract PDFString ConvertStringToFontEncoding(string str);

        internal abstract string ConvertFromFontEncoding(PDFString str);
    }
}
