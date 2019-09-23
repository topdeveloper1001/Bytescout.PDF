namespace Bytescout.PDF
{
    internal class AppearanceCharacteristics
    {
	    private readonly PDFDictionary _dictionary;

	    public RotationAngle RotationAngle
        {
            get { return TypeConverter.PDFNumberToRotationAngle(_dictionary["R"] as PDFNumber); }
            set { _dictionary.AddItem("R", TypeConverter.RotationAngleToPDFNumber(value)); }
        }

        public DeviceColor BorderColor
        {
            get
            {
                PDFArray  bc = _dictionary["BC"] as PDFArray;
                if (bc == null)
                    return null;
                return TypeConverter.PDFArrayToPDFColor(bc);
            }
            set
            {
                if (value == null)
                    _dictionary.RemoveItem("BC");
                else
                    _dictionary.AddItem("BC", TypeConverter.PDFColorToPDFArray(value));
            }
        }

        public DeviceColor BackgroundColor
        {
            get
            {
                PDFArray bg = _dictionary["BG"] as PDFArray;
                if (bg == null)
                    return null;
                return TypeConverter.PDFArrayToPDFColor(bg);
            }
            set
            {
                if (value == null)
                    _dictionary.RemoveItem("BG");
                else
                    _dictionary.AddItem("BG", TypeConverter.PDFColorToPDFArray(value));
            }
        }

        public string NormalCaption
        {
            get
            {
                PDFString str = _dictionary["CA"] as PDFString;
                if (str != null)
                    return str.GetValue();
                return "";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _dictionary.RemoveItem("CA");
                else
                    _dictionary.AddItem("CA", new PDFString(value));
            }
        }

        public string RolloverCaption
        {
            get
            {
                PDFString str = _dictionary["RC"] as PDFString;
                if (str != null)
                    return str.GetValue();
                return "";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _dictionary.RemoveItem("RC");
                else
                    _dictionary.AddItem("RC", new PDFString(value));
            }
        }

        public string AlternateCaption
        {
            get
            {
                PDFString str = _dictionary["AC"] as PDFString;
                if (str != null)
                    return str.GetValue();
                return "";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _dictionary.RemoveItem("AC");
                else
                    _dictionary.AddItem("AC", new PDFString(value));
            }
        }

	    internal AppearanceCharacteristics()
	    {
		    _dictionary = new PDFDictionary();
	    }

	    internal AppearanceCharacteristics(PDFDictionary dict)
	    {
		    _dictionary = dict;
	    }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        internal static PDFDictionary Copy(PDFDictionary dict)
        {
            PDFDictionary result = new PDFDictionary();
            string[] keys = { "R", "BC", "BG", "CA", "RC", "AC", "TP", "IF" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = dict[keys[i]];
                if (obj != null)
                    result.AddItem(keys[i], obj.Clone());
            }

            string[] xObjects = { "I", "RI", "IX" };
            for (int i = 0; i < xObjects.Length; ++i)
            {
                PDFDictionaryStream stream = dict[xObjects[i]] as PDFDictionaryStream;
                if (stream != null)
                    result.AddItem(xObjects[i], GraphicsTemplate.Copy(stream));
            }

            return result;
        }

        //I, RI, IX, IF, TP
    }
}
