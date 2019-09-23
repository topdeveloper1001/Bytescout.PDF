namespace Bytescout.PDF
{
    internal static class StandardFontGlyfData
    {
	    private static readonly IGlyfMetrics _courier = new CourierGlyfs();
	    private static readonly IGlyfMetrics _helvetica = new HelveticaGlyfs();
	    private static readonly IGlyfMetrics _helveticaBold = new HelveticaBoldGlyfs();
	    private static readonly IGlyfMetrics _helveticaItalic = new HelveticaObliqueGlyfs();
	    private static readonly IGlyfMetrics _helveticaBoldItalic = new HelveticaBoldObliqueGlyfs();
	    private static readonly IGlyfMetrics _times = new TimesGlyfs();
	    private static readonly IGlyfMetrics _timesBold = new TimesBoldGlyfs();
	    private static readonly IGlyfMetrics _timesItalic = new TimesItalicGlyfs();
	    private static readonly IGlyfMetrics _timesBoldItalic = new TimesBoldItalicGlyfs();
	    private static readonly IGlyfMetrics _symbol = new SymbolGlyfs();
		
	    internal static IGlyfMetrics Courier { get { return _courier; } }
		internal static IGlyfMetrics CourierBold { get { return _courier; } }
        internal static IGlyfMetrics CourierBoldOblique { get { return _courier; } }
	    internal static IGlyfMetrics CourierOblique { get { return _courier; } }
        internal static IGlyfMetrics Helvetica { get { return _helvetica; } }
        internal static IGlyfMetrics HelveticaBold { get { return _helveticaBold; } }
        internal static IGlyfMetrics HelveticaBoldOblique { get { return _helveticaBoldItalic; } }
        internal static IGlyfMetrics HelveticaOblique { get { return _helveticaItalic; } }
        internal static IGlyfMetrics Symbol { get { return _symbol; } }
        internal static IGlyfMetrics Times { get { return _times; } }
        internal static IGlyfMetrics TimesBold { get { return _timesBold; } }
        internal static IGlyfMetrics TimesBoldItalic { get { return _timesBoldItalic; } }
        internal static IGlyfMetrics TimesItalic { get { return _timesItalic; } }
    }
}
