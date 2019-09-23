using System.Collections.Generic;

namespace Bytescout.PDF
{
    internal class Base14Font: FontBase
    {
	    private readonly StandardFonts _builtFont;

	    internal override string Name
        {
            get
            {
                return ((PDFName) GetDictionary()["BaseFont"]).GetValue();
            }
        }

        internal override bool RealBold
        {
            get
            {
                switch (_builtFont)
                {
                    case StandardFonts.CourierBold:
                    case StandardFonts.CourierBoldOblique:
                    case StandardFonts.HelveticaBold:
                    case StandardFonts.HelveticaBoldOblique:
                    case StandardFonts.TimesBold:
                    case StandardFonts.TimesBoldItalic:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal override bool RealItalic
        {
            get
            {
                switch (_builtFont)
                {
                    case StandardFonts.CourierBoldOblique:
                    case StandardFonts.CourierOblique:
                    case StandardFonts.HelveticaBoldOblique:
                    case StandardFonts.HelveticaOblique:
                    case StandardFonts.TimesBoldItalic:
                    case StandardFonts.TimesItalic:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal StandardFonts BuiltInFontType { get { return _builtFont; } }

        internal override System.Drawing.Rectangle FontBBox
        {
            get 
            {
                PDFArray bbox = (GetDictionary()["FontDescriptor"] as PDFDictionary)["FontBBox"] as PDFArray;
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                
                rect.X = 0;
                rect.Y = 0;
                rect.Width = (int)(bbox[2] as PDFNumber).GetValue();
                rect.Height = (int)(bbox[3] as PDFNumber).GetValue();

                return rect;
            }
        }

	    public Base14Font(StandardFonts builtFont)
	    {
		    _builtFont = builtFont;

		    PDFDictionary dict = GetDictionary();
		    dict.AddItem("FirstChar", new PDFNumber(32));
		    dict.AddItem("LastChar", new PDFNumber(255));
		    dict.AddItem("Type", new PDFName("Font"));
		    dict.AddItem("Subtype", new PDFName("Type1"));

		    addFontName();
		    addWidths();
		    addFontDescriptor();
		    GetDictionary().Tag = this;
	    }

	    internal override bool Contains(char c)
        {
            return 32 >= c && c <= 255; 
        }

        internal override void CreateFontDictionary()
        {
        }

        internal override void AddStringToEncoding(string str)
        {
        }

        internal override PDFString ConvertStringToFontEncoding(string str)
        {
            byte[] tmp = System.Text.Encoding.Default.GetBytes(str);
            List<byte> result = new List<byte>();
            for (int i = 0; i < tmp.Length; ++i)
            {
                if (!(tmp[i] == '?' && str[i] != '?'))
                    result.Add(tmp[i]);
            }
            return new PDFString(result.ToArray(), false);
        }

        internal override string ConvertFromFontEncoding(PDFString str)
        {
            byte[] data = str.GetBytes();
            char[] result = new char[data.Length];

            for (int i = 0; i < data.Length; ++i)
                result[i] = (char)data[i];

            return new string(result);
        }

        internal override float GetTextHeight(float fontSize)
        {
            PDFArray bbox = (GetDictionary()["FontDescriptor"] as PDFDictionary)["FontBBox"] as PDFArray;
            float height = (float)(bbox[3] as PDFNumber).GetValue();
            return height * fontSize / 1000.0f;
        }

        internal override float GetTextWidth(string text, float fontSize)
        {
            PDFArray widths = GetDictionary()["Widths"] as PDFArray;
            float result = 0.0f;

            PDFString str = ConvertStringToFontEncoding(text);
            byte[] buf = str.GetBytes();
            for (int i = 0; i < buf.Length; ++i)
            {
                PDFNumber w = widths[buf[i] - 32] as PDFNumber;
                if (w != null)
                    result += (float)w.GetValue();
            }

            result = result * fontSize / 1000.0f;
            return result;
        }

        internal override float GetTextWidth(PDFString str, float fontSize)
        {
            PDFArray widths = GetDictionary()["Widths"] as PDFArray;
            float result = 0.0f;

            byte[] buf = str.GetBytes();
            for (int i = 0; i < buf.Length; ++i)
            {
                PDFNumber w = widths[buf[i] - 32] as PDFNumber;
                if (w != null)
                    result += (float)w.GetValue();
            }

            result = result * fontSize / 1000.0f;
            return result;
        }

        internal override float GetCharWidth(char c, float fontSize)
        {
            if (c < 32 || c > 255)
                return 0;

            PDFArray widths = GetDictionary()["Widths"] as PDFArray;
            PDFNumber w = widths[c - 32] as PDFNumber;
            return (float)w.GetValue() * fontSize / 1000.0f;
        }

        internal static System.Drawing.Rectangle GetFontBBox(StandardFonts font)
        {
            switch (font)
            {
                case StandardFonts.Courier:
                case StandardFonts.CourierOblique:
                    return new System.Drawing.Rectangle(-6, -249, 639, 803);
                case StandardFonts.CourierBold:
                case StandardFonts.CourierBoldOblique:
                    return new System.Drawing.Rectangle(-88, -249, 697, 811);
                case StandardFonts.Helvetica:
                case StandardFonts.HelveticaOblique:
                    return new System.Drawing.Rectangle(-166, -255, 1000, 931);
                case StandardFonts.HelveticaBold:
                case StandardFonts.HelveticaBoldOblique:
                    return new System.Drawing.Rectangle(-170, -228, 1003, 962);
                case StandardFonts.Symbol:
                    return new System.Drawing.Rectangle(-180, -293, 1090, 1010);
                case StandardFonts.Times:
                    return new System.Drawing.Rectangle(-168, -218, 1000, 898);
                case StandardFonts.TimesBold:
                    return new System.Drawing.Rectangle(-168, -218, 1000, 935);
                case StandardFonts.TimesBoldItalic:
                    return new System.Drawing.Rectangle(-200, -218, 996, 921);
                case StandardFonts.TimesItalic:
                    return new System.Drawing.Rectangle(-169, -217, 1010, 883);
                case StandardFonts.ZapfDingbats:
                    return new System.Drawing.Rectangle(-1, -143, 981, 820);
            }
            return new System.Drawing.Rectangle(0, 0, 0, 0);
        }

        private void addFontName()
        {
            switch (_builtFont)
            {
                case StandardFonts.Courier:
                    GetDictionary().AddItem("BaseFont", new PDFName("Courier"));
                    break;
                case StandardFonts.CourierBold:
                    GetDictionary().AddItem("BaseFont", new PDFName("Courier-Bold"));
                    break;
                case StandardFonts.CourierBoldOblique:
                    GetDictionary().AddItem("BaseFont", new PDFName("Courier-BoldOblique"));
                    break;
                case StandardFonts.CourierOblique:
                    GetDictionary().AddItem("BaseFont", new PDFName("Courier-Oblique"));
                    break;
                case StandardFonts.Helvetica:
                    GetDictionary().AddItem("BaseFont", new PDFName("Helvetica"));
                    break;
                case StandardFonts.HelveticaBold:
                    GetDictionary().AddItem("BaseFont", new PDFName("Helvetica-Bold"));
                    break;
                case StandardFonts.HelveticaBoldOblique:
                    GetDictionary().AddItem("BaseFont", new PDFName("Helvetica-BoldOblique"));
                    break;
                case StandardFonts.HelveticaOblique:
                    GetDictionary().AddItem("BaseFont", new PDFName("Helvetica-Oblique"));
                    break;
                case StandardFonts.Symbol:
                    GetDictionary().AddItem("BaseFont", new PDFName("Symbol"));
                    break;
                case StandardFonts.Times:
                    GetDictionary().AddItem("BaseFont", new PDFName("Times-Roman"));
                    break;
                case StandardFonts.TimesBold:
                    GetDictionary().AddItem("BaseFont", new PDFName("Times-Bold"));
                    break;
                case StandardFonts.TimesBoldItalic:
                    GetDictionary().AddItem("BaseFont", new PDFName("Times-BoldItalic"));
                    break;
                case StandardFonts.TimesItalic:
                    GetDictionary().AddItem("BaseFont", new PDFName("Times-Italic"));
                    break;
                case StandardFonts.ZapfDingbats:
                    GetDictionary().AddItem("BaseFont", new PDFName("ZapfDingbats"));
                    break;
            }
        }

        private void addFontDescriptor()
        {
            switch (_builtFont)
            {
                case StandardFonts.Courier:
                    addCourierDescriptor();
                    break;
                case StandardFonts.CourierBold:
                    addCourierBoldDescriptor();
                    break;
                case StandardFonts.CourierBoldOblique:
                    addCourierBoldObliqueDescriptor();
                    break;
                case StandardFonts.CourierOblique:
                    addCourierObliqueDescriptor();
                    break;
                case StandardFonts.Helvetica:
                    addHelveticaDescriptor();
                    break;
                case StandardFonts.HelveticaBold:
                    addHelveticaBoldDescriptor();
                    break;
                case StandardFonts.HelveticaBoldOblique:
                    addHelveticaBoldObliqueDescriptor();
                    break;
                case StandardFonts.HelveticaOblique:
                    addHelveticaObliqueDescriptor();
                    break;
                case StandardFonts.Symbol:
                    addSymbolDescriptor();
                    break;
                case StandardFonts.Times:
                    addTimesDescriptor();
                    break;
                case StandardFonts.TimesBold:
                    addTimesBoldDescriptor();
                    break;
                case StandardFonts.TimesBoldItalic:
                    addTimesBoldItalicDescriptor();
                    break;
                case StandardFonts.TimesItalic:
                    addTimesItalicDescriptor();
                    break;
                case StandardFonts.ZapfDingbats:
                    addZapfDingbatsDescriptor();
                    break;
            }
        }

        private void addCourierDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(629));
            descriptor.AddItem("CapHeight", new PDFNumber(562));
            descriptor.AddItem("Descent", new PDFNumber(-157));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Courier"));
            descriptor.AddItem("FontWeight", new PDFNumber(500));
            descriptor.AddItem("ItalicAngle", new PDFNumber(0));
            descriptor.AddItem("MissingWidth", new PDFNumber(600));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(426));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-6));
            bbox.AddItem(new PDFNumber(-249));
            bbox.AddItem(new PDFNumber(639));
            bbox.AddItem(new PDFNumber(803));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addCourierBoldDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(629));
            descriptor.AddItem("CapHeight", new PDFNumber(562));
            descriptor.AddItem("Descent", new PDFNumber(-157));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Courier-Bold"));
            descriptor.AddItem("FontWeight", new PDFNumber(700));
            descriptor.AddItem("ItalicAngle", new PDFNumber(0));
            descriptor.AddItem("MissingWidth", new PDFNumber(600));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(439));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-88));
            bbox.AddItem(new PDFNumber(-249));
            bbox.AddItem(new PDFNumber(697));
            bbox.AddItem(new PDFNumber(811));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addCourierBoldObliqueDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(629));
            descriptor.AddItem("CapHeight", new PDFNumber(562));
            descriptor.AddItem("Descent", new PDFNumber(-157));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Courier-BoldOblique"));
            descriptor.AddItem("FontWeight", new PDFNumber(700));
            descriptor.AddItem("ItalicAngle", new PDFNumber(-11));
            descriptor.AddItem("MissingWidth", new PDFNumber(600));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(439));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-88));
            bbox.AddItem(new PDFNumber(-249));
            bbox.AddItem(new PDFNumber(697));
            bbox.AddItem(new PDFNumber(811));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addCourierObliqueDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(629));
            descriptor.AddItem("CapHeight", new PDFNumber(562));
            descriptor.AddItem("Descent", new PDFNumber(-157));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Courier-Oblique"));
            descriptor.AddItem("FontWeight", new PDFNumber(500));
            descriptor.AddItem("ItalicAngle", new PDFNumber(-11));
            descriptor.AddItem("MissingWidth", new PDFNumber(600));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(426));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-6));
            bbox.AddItem(new PDFNumber(-249));
            bbox.AddItem(new PDFNumber(639));
            bbox.AddItem(new PDFNumber(803));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addHelveticaDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(718));
            descriptor.AddItem("CapHeight", new PDFNumber(718));
            descriptor.AddItem("Descent", new PDFNumber(-207));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Helvetica"));
            descriptor.AddItem("FontWeight", new PDFNumber(500));
            descriptor.AddItem("ItalicAngle", new PDFNumber(0));
            descriptor.AddItem("MissingWidth", new PDFNumber(278));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(523));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-166));
            bbox.AddItem(new PDFNumber(-225));
            bbox.AddItem(new PDFNumber(1000));
            bbox.AddItem(new PDFNumber(931));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addHelveticaBoldDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(718));
            descriptor.AddItem("CapHeight", new PDFNumber(718));
            descriptor.AddItem("Descent", new PDFNumber(-207));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Helvetica-Bold"));
            descriptor.AddItem("FontWeight", new PDFNumber(700));
            descriptor.AddItem("ItalicAngle", new PDFNumber(0));
            descriptor.AddItem("MissingWidth", new PDFNumber(278));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(532));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-170));
            bbox.AddItem(new PDFNumber(-228));
            bbox.AddItem(new PDFNumber(1003));
            bbox.AddItem(new PDFNumber(962));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addHelveticaBoldObliqueDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(718));
            descriptor.AddItem("CapHeight", new PDFNumber(718));
            descriptor.AddItem("Descent", new PDFNumber(-207));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Helvetica-BoldOblique"));
            descriptor.AddItem("FontWeight", new PDFNumber(700));
            descriptor.AddItem("ItalicAngle", new PDFNumber(-12));
            descriptor.AddItem("MissingWidth", new PDFNumber(278));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(532));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-170));
            bbox.AddItem(new PDFNumber(-228));
            bbox.AddItem(new PDFNumber(1003));
            bbox.AddItem(new PDFNumber(962));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addHelveticaObliqueDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(718));
            descriptor.AddItem("CapHeight", new PDFNumber(718));
            descriptor.AddItem("Descent", new PDFNumber(-207));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Helvetica-Oblique"));
            descriptor.AddItem("FontWeight", new PDFNumber(500));
            descriptor.AddItem("ItalicAngle", new PDFNumber(-12));
            descriptor.AddItem("MissingWidth", new PDFNumber(278));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(532));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-166));
            bbox.AddItem(new PDFNumber(-225));
            bbox.AddItem(new PDFNumber(1000));
            bbox.AddItem(new PDFNumber(931));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addSymbolDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(692));
            descriptor.AddItem("CapHeight", new PDFNumber(0));
            descriptor.AddItem("Descent", new PDFNumber(-14));
            descriptor.AddItem("Flags", new PDFNumber(4));
            descriptor.AddItem("FontName", new PDFName("Symbol"));
            descriptor.AddItem("FontWeight", new PDFNumber(500));
            descriptor.AddItem("ItalicAngle", new PDFNumber(0));
            descriptor.AddItem("MissingWidth", new PDFNumber(250));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(0));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-180));
            bbox.AddItem(new PDFNumber(-293));
            bbox.AddItem(new PDFNumber(1090));
            bbox.AddItem(new PDFNumber(1010));
            descriptor.AddItem("FontBBox", bbox);

            byte[] data = System.Text.Encoding.ASCII.GetBytes("/space/exclam/universal/numbersign/existential/percent/ampersand/suchthat/parenleft/parenright/asteriskmath/plus/comma/minus/period/slash/zero/one/two/three/four/five/six/seven/eight/nine/colon/semicolon/less/equal/greater/question/congruent/Alpha/Beta/Chi/Delta/Epsilon/Phi/Gamma/Eta/Iota/theta1/Kappa/Lambda/Mu/Nu/Omicron/Pi/Theta/Rho/Sigma/Tau/Upsilon/sigma1/Omega/Xi/Psi/Zeta/bracketleft/therefore/bracketright/perpendicular/underscore/radicalex/alpha/beta/chi/delta/epsilon/phi/gamma/eta/iota/phi1/kappa/lambda/mu/nu/omicron/pi/theta/rho/sigma/tau/upsilon/omega1/omega/xi/psi/zeta/braceleft/bar/braceright/similar/Euro/Upsilon1/minute/lessequal/fraction/infinity/florin/club/diamond/heart/spade/arrowboth/arrowleft/arrowup/arrowright/arrowdown/degree/plusminus/second/greaterequal/multiply/proportional/partialdiff/bullet/divide/notequal/equivalence/approxequal/ellipsis/arrowvertex/arrowhorizex/carriagereturn/aleph/Ifraktur/Rfraktur/weierstrass/circlemultiply/circleplus/emptyset/intersection/union/propersuperset/reflexsuperset/notsubset/propersubset/reflexsubset/element/notelement/angle/gradient/registerserif/copyrightserif/trademarkserif/product/radical/dotmath/logicalnot/logicaland/logicalor/arrowdblboth/arrowdblleft/arrowdblup/arrowdblright/arrowdbldown/lozenge/angleleft/registersans/copyrightsans/trademarksans/summation/parenlefttp/parenleftex/parenleftbt/bracketlefttp/bracketleftex/bracketleftbt/bracelefttp/braceleftmid/braceleftbt/braceex/angleright/integral/integraltp/integralex/integralbt/parenrighttp/parenrightex/parenrightbt/bracketrighttp/bracketrightex/bracketrightbt/bracerighttp/bracerightmid/bracerightbt");
            descriptor.AddItem("CharSet", new PDFString(data, false));

            GetDictionary().AddItem("FontDescriptor", descriptor);

            addSymbolEncodingDictionary();
        }

        private void addTimesDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(683));
            descriptor.AddItem("CapHeight", new PDFNumber(662));
            descriptor.AddItem("Descent", new PDFNumber(-217));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Times-Roman"));
            descriptor.AddItem("FontWeight", new PDFNumber(500));
            descriptor.AddItem("ItalicAngle", new PDFNumber(0));
            descriptor.AddItem("MissingWidth", new PDFNumber(250));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(450));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-168));
            bbox.AddItem(new PDFNumber(-218));
            bbox.AddItem(new PDFNumber(1000));
            bbox.AddItem(new PDFNumber(898));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addTimesBoldDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(683));
            descriptor.AddItem("CapHeight", new PDFNumber(676));
            descriptor.AddItem("Descent", new PDFNumber(-217));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Times-Bold"));
            descriptor.AddItem("FontWeight", new PDFNumber(700));
            descriptor.AddItem("ItalicAngle", new PDFNumber(0));
            descriptor.AddItem("MissingWidth", new PDFNumber(250));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(461));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-168));
            bbox.AddItem(new PDFNumber(-218));
            bbox.AddItem(new PDFNumber(1000));
            bbox.AddItem(new PDFNumber(935));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addTimesBoldItalicDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(683));
            descriptor.AddItem("CapHeight", new PDFNumber(669));
            descriptor.AddItem("Descent", new PDFNumber(-217));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Times-BoldItalic"));
            descriptor.AddItem("FontWeight", new PDFNumber(700));
            descriptor.AddItem("ItalicAngle", new PDFNumber(-15));
            descriptor.AddItem("MissingWidth", new PDFNumber(250));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(462));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-200));
            bbox.AddItem(new PDFNumber(-218));
            bbox.AddItem(new PDFNumber(996));
            bbox.AddItem(new PDFNumber(921));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addTimesItalicDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(683));
            descriptor.AddItem("CapHeight", new PDFNumber(653));
            descriptor.AddItem("Descent", new PDFNumber(-217));
            descriptor.AddItem("Flags", new PDFNumber(32));
            descriptor.AddItem("FontName", new PDFName("Times-Italic"));
            descriptor.AddItem("FontWeight", new PDFNumber(500));
            descriptor.AddItem("ItalicAngle", new PDFNumber(-15));
            descriptor.AddItem("MissingWidth", new PDFNumber(250));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(441));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-169));
            bbox.AddItem(new PDFNumber(-217));
            bbox.AddItem(new PDFNumber(1010));
            bbox.AddItem(new PDFNumber(883));
            descriptor.AddItem("FontBBox", bbox);

            GetDictionary().AddItem("FontDescriptor", descriptor);
            addEncoding();
        }

        private void addZapfDingbatsDescriptor()
        {
            PDFDictionary descriptor = new PDFDictionary();

            descriptor.AddItem("Ascent", new PDFNumber(692));
            descriptor.AddItem("CapHeight", new PDFNumber(0));
            descriptor.AddItem("Descent", new PDFNumber(-140));
            descriptor.AddItem("Flags", new PDFNumber(4));
            descriptor.AddItem("FontName", new PDFName("ZapfDingbats"));
            descriptor.AddItem("FontWeight", new PDFNumber(500));
            descriptor.AddItem("ItalicAngle", new PDFNumber(0));
            descriptor.AddItem("MissingWidth", new PDFNumber(278));
            descriptor.AddItem("StemV", new PDFNumber(0));
            descriptor.AddItem("XHeight", new PDFNumber(0));
            descriptor.AddItem("Type", new PDFName("FontDescriptor"));

            PDFArray bbox = new PDFArray();
            bbox.AddItem(new PDFNumber(-1));
            bbox.AddItem(new PDFNumber(-143));
            bbox.AddItem(new PDFNumber(981));
            bbox.AddItem(new PDFNumber(820));
            descriptor.AddItem("FontBBox", bbox);

            byte[] data = System.Text.Encoding.ASCII.GetBytes("/space/a1/a2/a202/a3/a4/a5/a119/a118/a117/a11/a12/a13/a14/a15/a16/a105/a17/a18/a19/a20/a21/a22/a23/a24/a25/a26/a27/a28/a6/a7/a8/a9/a10/a29/a30/a31/a32/a33/a34/a35/a36/a37/a38/a39/a40/a41/a42/a43/a44/a45/a46/a47/a48/a49/a50/a51/a52/a53/a54/a55/a56/a57/a58/a59/a60/a61/a62/a63/a64/a65/a66/a67/a68/a69/a70/a71/a72/a73/a74/a203/a75/a204/a76/a77/a78/a79/a81/a82/a83/a84/a97/a98/a99/a100/a89/a90/a93/a94/a91/a92/a205/a85/a206/a86/a87/a88/a95/a96/a101/a102/a103/a104/a106/a107/a108/a112/a111/a110/a109/a120/a121/a122/a123/a124/a125/a126/a127/a128/a129/a130/a131/a132/a133/a134/a135/a136/a137/a138/a139/a140/a141/a142/a143/a144/a145/a146/a147/a148/a149/a150/a151/a152/a153/a154/a155/a156/a157/a158/a159/a160/a161/a163/a164/a196/a165/a192/a166/a167/a168/a169/a170/a171/a172/a173/a162/a174/a175/a176/a177/a178/a179/a193/a180/a199/a181/a200/a182/a201/a183/a184/a197/a185/a194/a198/a186/a195/a187/a188/a189/a190/a191");
            descriptor.AddItem("CharSet", new PDFString(data, false));

            GetDictionary().AddItem("FontDescriptor", descriptor);

            addZapfDingbatsEncodingDictionary();
        }

        private void addEncoding()
        {
            GetDictionary().AddItem("Encoding", new PDFName("WinAnsiEncoding"));
        }

        private void addSymbolEncodingDictionary()
        {
            PDFDictionary encoding = new PDFDictionary();
            encoding.AddItem("Type", new PDFName("Encoding"));
            encoding.AddItem("BaseEncoding", new PDFName("WinAnsiEncoding"));

            PDFArray differences = new PDFArray();
            int[] indexes = {
                                0,   1,   2,   3,   4,   5,   6,   7,   8,   9,   10,  11,  12,  13,  14,  15, 
                                16,  17,  18,  19,  20,  21,  22,  23,  24,  25,  26,  27,  28,  29,  30,  31, 
                                34,  36,  39,  42,  45,  64,  65,  66,  67,  68,  69,  70,  71,  72,  73,  74, 
                                75,  76,  77,  78,  79,  80,  81,  82,  83,  84,  85,  86,  87,  88,  89,  90, 
                                92,  94,  96,  97,  98,  99,  100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 
                                110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 126, 128, 130, 
                                131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 142, 145, 146, 147, 148, 149, 
                                150, 151, 152, 153, 154, 155, 156, 158, 159, 160, 161, 162, 163, 164, 165, 166, 
                                167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 178, 179, 180, 181, 182, 183, 
                                184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 
                                200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 
                                216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 
                                232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 
                                248, 249, 250, 251, 252, 253, 254, 255
                            };

            string[] names = {
                                ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", 
                                ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", 
                                "universal", "existential", "suchthat", "asteriskmath", "minus", "congruent", "Alpha", "Beta", "Chi", "Delta", "Epsilon", "Phi", "Gamma", "Eta", "Iota", "theta1", 
                                "Kappa", "Lambda", "Mu", "Nu", "Omicron", "Pi", "Theta", "Rho", "Sigma", "Tau", "Upsilon", "sigma1", "Omega", "Xi", "Psi", "Zeta", 
                                "therefore", "perpendicular", "radicalex", "alpha", "beta", "chi", "delta", "epsilon", "phi", "gamma", "eta", "iota", "phi1", "kappa", "lambda", "mu", 
                                "nu", "omicron", "pi", "theta", "rho", "sigma", "tau", "upsilon", "omega1", "omega", "xi", "psi", "zeta", "similar", ".notdef", ".notdef", 
                                ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", 
                                ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", "Euro", "Upsilon1", "minute", "lessequal", "fraction", "infinity", "florin", 
                                "club", "diamond", "heart", "spade", "arrowboth", "arrowleft", "arrowup", "arrowright", "arrowdown", "degree", "second", "greaterequal", "multiply", "proportional", "partialdiff", "bullet", 
                                "divide", "notequal", "equivalence", "approxequal", "ellipsis", "arrowvertex", "arrowhorizex", "carriagereturn", "aleph", "Ifraktur", "Rfraktur", "weierstrass", "circlemultiply", "circleplus", "emptyset", "intersection", 
                                "union", "propersuperset", "reflexsuperset", "notsubset", "propersubset", "reflexsubset", "element", "notelement", "angle", "gradient", "registerserif", "copyrightserif", "trademarkserif", "product", "radical", "dotmath", 
                                "logicalnot", "logicaland", "logicalor", "arrowdblboth", "arrowdblleft", "arrowdblup", "arrowdblright", "arrowdbldown", "lozenge", "angleleft", "registersans", "copyrightsans", "trademarksans", "summation", "parenlefttp", "parenleftex", 
                                "parenleftbt", "bracketlefttp", "bracketleftex", "bracketleftbt", "bracelefttp", "braceleftmid", "braceleftbt", "braceex", ".notdef", "angleright", "integral", "integraltp", "integralex", "integralbt", "parenrighttp", "parenrightex", 
                                "parenrightbt", "bracketrighttp", "bracketrightex", "bracketrightbt", "bracerighttp", "bracerightmid", "bracerightbt", ".notdef"
                             };

            for (int i = 0; i < names.Length; ++i)
            {
                differences.AddItem(new PDFNumber(indexes[i]));
                differences.AddItem(new PDFName(names[i]));
            }

            encoding.AddItem("Differences", differences);
            GetDictionary().AddItem("Encoding", encoding);
        }

        private void addZapfDingbatsEncodingDictionary()
        {
            PDFDictionary encoding = new PDFDictionary();
            encoding.AddItem("Type", new PDFName("Encoding"));
            encoding.AddItem("BaseEncoding", new PDFName("WinAnsiEncoding"));

            PDFArray differences = new PDFArray();

            int[] indexes = {
                                0,   1,   2,   3,   4,   5,   6,   7,   8,   9,   10,  11,  12,  13,  14,  15, 
                                16,  17,  18,  19,  20,  21,  22,  23,  24,  25,  26,  27,  28,  29,  30,  31, 
                                33,  34,  35,  36,  37,  38,  39,  40,  41,  42,  43,  44,  45,  46,  47,  48, 
                                49,  50,  51,  52,  53,  54,  55,  56,  57,  58,  59,  60,  61,  62,  63,  64, 
                                65,  66,  67,  68,  69,  70,  71,  72,  73,  74,  75,  76,  77,  78,  79,  80, 
                                81,  82,  83,  84,  85,  86,  87,  88,  89,  90,  91,  92,  93,  94,  95,  96, 
                                97,  98,  99,  100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 
                                113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 128, 129, 
                                130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 145, 146, 147, 
                                148, 149, 150, 151, 152, 153, 154, 155, 156, 158, 159, 161, 162, 163, 164, 165, 
                                166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 
                                182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 
                                198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 
                                214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 
                                230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 
                                246, 247, 248, 249, 250, 251, 252, 253, 254, 255
                            };

            string[] names = {
                                ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", 
                                ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", 
                                "a1", "a2", "a202", "a3", "a4", "a5", "a119", "a118", "a117", "a11", "a12", "a13", "a14", "a15", "a16", "a105", 
                                "a17", "a18", "a19", "a20", "a21", "a22", "a23", "a24", "a25", "a26", "a27", "a28", "a6", "a7", "a8", "a9", 
                                "a10", "a29", "a30", "a31", "a32", "a33", "a34", "a35", "a36", "a37", "a38", "a39", "a40", "a41", "a42", "a43", 
                                "a44", "a45", "a46", "a47", "a48", "a49", "a50", "a51", "a52", "a53", "a54", "a55", "a56", "a57", "a58", "a59", 
                                "a60", "a61", "a62", "a63", "a64", "a65", "a66", "a67", "a68", "a69", "a70", "a71", "a72", "a73", "a74", "a203", 
                                "a75", "a204", "a76", "a77", "a78", "a79", "a81", "a82", "a83", "a84", "a97", "a98", "a99", "a100", "a89", "a90", 
                                "a93", "a94", "a91", "a92", "a205", "a85", "a206", "a86", "a87", "a88", "a95", "a96", ".notdef", ".notdef", ".notdef", ".notdef", 
                                ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", ".notdef", "a101", "a102", "a103", "a104", "a106", 
                                "a107", "a108", "a112", "a111", "a110", "a109", "a120", "a121", "a122", "a123", "a124", "a125", "a126", "a127", "a128", "a129", 
                                "a130", "a131", "a132", "a133", "a134", "a135", "a136", "a137", "a138", "a139", "a140", "a141", "a142", "a143", "a144", "a145", 
                                "a146", "a147", "a148", "a149", "a150", "a151", "a152", "a153", "a154", "a155", "a156", "a157", "a158", "a159", "a160", "a161", 
                                "a163", "a164", "a196", "a165", "a192", "a166", "a167", "a168", "a169", "a170", "a171", "a172", "a173", "a162", "a174", "a175", 
                                "a176", "a177", "a178", "a179", "a193", "a180", "a199", "a181", "a200", "a182", ".notdef", "a201", "a183", "a184", "a197", "a185", 
                                "a194", "a198", "a186", "a195", "a187", "a188", "a189", "a190", "a191", ".notdef"
                             };

            for (int i = 0; i < names.Length; ++i)
            {
                differences.AddItem(new PDFNumber(indexes[i]));
                differences.AddItem(new PDFName(names[i]));
            }

            encoding.AddItem("Differences", differences);
            GetDictionary().AddItem("Encoding", encoding);
        }

        private void addWidths()
        {
            int[] values = Base14GlyfWidth.GetWidthArray(_builtFont);
            PDFArray widths = new PDFArray();
            for (int i = 0; i < values.Length; ++i)
                widths.AddItem(new PDFNumber(values[i]));

            GetDictionary().AddItem("Widths", widths);
        }
    }
}
