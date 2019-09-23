using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class ChoiceField: Field
#else
	/// <summary>
    /// Represents an abstract class for choice fields.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class ChoiceField: Field
#endif
	{
	    private Font _font;
	    private DeviceColor _fontColor;
	    private ChoiceItems _items;

	    /// <summary>
        /// Gets or sets a value indicating whether the field’s option items should be sorted alphabetically.
        /// This flag is intended for use by form authoring tools, not by PDF viewer applications.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool Sort
        {
            get { return getFlagAttribute(20); }
            set { setFlagAttribute(20, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to check spelling.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool SpellCheck
        {
            get { return !getFlagAttribute(23); }
            set { setFlagAttribute(23, !value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the new value is committed as soon as a selection is made with the pointing device.
        /// This option enables applications to perform an action once a selection is made, without requiring the user to exit the field.
        /// If clear, the new value is not committed until the user exits the field.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool CommitOnSelChange
        {
            get { return getFlagAttribute(27); }
            set { setFlagAttribute(27, value); }
        }

        /// <summary>
        /// Gets or sets the font used to display text in the control.
        /// </summary>
        /// <value cref="PDF.Font"></value>
        public Font Font
        {
            get
            {
                return _font;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException();

                if (_font != null)
                    _font.ChangedFontSize -= changedFontSize;

                _font = value;
                setTextAttributes();
                _font.ChangedFontSize += changedFontSize;

                for (int i = 0; i < Items.Count; ++i)
                    _font.BaseFont.AddStringToEncoding(Items[i]);

                if (this is ComboBox)
                    _font.BaseFont.AddStringToEncoding(((ComboBox) this).Text);
                CreateApperance();
            }
        }

        /// <summary>
        /// Gets or sets the color of the font used to display text in the control.
        /// </summary>
        /// <value cref="DeviceColor"></value>
        public DeviceColor FontColor
        {
            get
            {
                return _fontColor;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException();
                _fontColor = value;
                setTextAttributes();
                CreateApperance();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how the text should be aligned.
        /// </summary>
        /// <value cref="PDF.TextAlign"></value>
        public TextAlign TextAlign
        {
            get { return TypeConverter.PDFNumberToPDFTextAlign(Dictionary["Q"] as PDFNumber); }
            set
            {
                Dictionary.AddItem("Q", TypeConverter.PDFTextAlignToPDFNumber(value));
                CreateApperance();
            }
        }

        /// <summary>
        /// Gets the items of this PDFMoasic.ChoiceField.
        /// </summary>
        /// <value cref="ChoiceItems"></value>
        public ChoiceItems Items
        {
            get
            {
                if (_items == null)
                    loadItems();

                return _items;
            }
        }

	    internal bool MultiSelect
        {
            get { return getFlagAttribute(22); }
            set { setFlagAttribute(22, value); }
        }

        internal bool Combo
        {
            get { return getFlagAttribute(18); }
            set { setFlagAttribute(18, value); }
        }

        internal bool Editable
        {
            get { return getFlagAttribute(19); }
            set { setFlagAttribute(19, value); }
        }

	    internal ChoiceField(float left, float top, float width, float height, string name, IDocumentEssential owner)
		    : base(left, top, width, height, name, owner)
	    {
		    Dictionary.AddItem("FT", new PDFName("Ch"));
		    _font = new Font(StandardFonts.Helvetica, 12);
		    _font.ChangedFontSize += changedFontSize;
		    _fontColor = new ColorRGB(0, 0, 0);
	    }

	    internal ChoiceField(PDFDictionary dict, IDocumentEssential owner)
		    : base(dict, owner)
	    {
		    parseFontAndColor();
	    }

	    internal static void Copy(PDFDictionary sourceDict, PDFDictionary destinationDict)
	    {
		    string[] keys = { "Opt", "TI", "I", "Q", "RV" };
		    for (int i = 0; i < keys.Length; ++i)
		    {
			    IPDFObject obj = sourceDict[keys[i]];
			    if (obj != null)
				    destinationDict.AddItem(keys[i], obj.Clone());
		    }
	    }

	    internal void SetTextProperties(Font font, DeviceColor color)
        {
            if (_font != null)
                _font.ChangedFontSize -= changedFontSize;

            _font = font;
            _fontColor = color;
            setTextAttributes();
            _font.ChangedFontSize += changedFontSize;
        }

        private void setTextAttributes()
        {
            if (Owner != null && _fontColor != null && _font != null)
            {
                string fnt = Owner.AddFontAcroForm(_font);
                string tmp = '/' + fnt + ' ' + StringUtility.GetString(_font.Size) + " Tf " + _fontColor;
                switch (_fontColor.Colorspace.N)
                {
                    case 1:
                        tmp += " g";
                        break;
                    case 3:
                        tmp += " rg";
                        break;
                    case 4:
                        tmp += " k";
                        break;
                }

                Dictionary.AddItem("DA", new PDFString(System.Text.Encoding.ASCII.GetBytes(tmp), false));
            }
        }

        private bool getFlagAttribute(byte bytePosition)
        {
            return (Ff >> bytePosition - 1) % 2 != 0;
        }

        private void setFlagAttribute(byte bytePosition, bool value)
        {
            if (value)
                Ff = Ff | (uint)(1 << (bytePosition - 1));
            else
                Ff = Ff & (0xFFFFFFFF ^ (uint)(1 << (bytePosition - 1)));
        }

        private void loadItems()
        {
            PDFArray opt = Dictionary["Opt"] as PDFArray;
            if (opt == null)
            {
                PDFArray arr = new PDFArray();
                Dictionary.AddItem("Opt", arr);
                _items = new ChoiceItems(arr);
            }
            else
            {
                _items = new ChoiceItems(opt);
            }
            _items.AddedChoiceItem += addedChoiceItem;
            _items.ChangedChoiceItems += changedChoiceItems;
        }

        private void changedChoiceItems(object sender)
        {
            CreateApperance();
        }

        private void addedChoiceItem(object sender, AddedChoiceItemEvent e)
        {
            _font.BaseFont.AddStringToEncoding(e.Item);
        }

        private void parseFontAndColor()
        {
            if (Owner != null)
            {
                byte[] data = null;
                PDFString da = Dictionary["DA"] as PDFString;
                if (da != null)
                    data = da.GetBytes();
                else
                {
                    PDFString ada = Owner.GetAcroFormDefaultAttribute();
                    if (ada != null)
                        data = ada.GetBytes();
                }

                if (data == null)
                {
                    _fontColor = new ColorRGB(0, 0, 0);
                    _font = new Font(StandardFonts.Helvetica, 12);
                    _font.ChangedFontSize += changedFontSize;
                    return;
                }

                PageOperationParser parser = new PageOperationParser(new MemoryStream(data));
                List<IPDFPageOperation> operations = new List<IPDFPageOperation>();
                IPDFPageOperation operation;
                while ((operation = parser.Next()) != null)
                    operations.Add(operation);
                
                parseFont(operations);
                parseFontColor(operations);
            }
        }

        private void parseFont(List<IPDFPageOperation> operations)
        {
            for (int i = operations.Count - 1; i >= 0; --i)
            {
                if (operations[i] is TextFont)
                {
                    PDFDictionary fontDict = Owner.GetAcroFormFont(((TextFont)operations[i]).FontName.Substring(1));
                    float fontSize = ((TextFont)operations[i]).FontSize;
                    if (fontDict != null)
                        _font = new Font(FontBase.Instance(fontDict));
                    else
                        _font = new Font(StandardFonts.Helvetica, 8);

                    _font.Size = fontSize;
                    break;
                }
            }

            if (_font == null)
                _font = new Font(StandardFonts.Helvetica, 12);
            _font.ChangedFontSize += changedFontSize;
        }

        private void parseFontColor(List<IPDFPageOperation> operations)
        {
            for (int i = operations.Count - 1; i >= 0; --i)
            {
                if (operations[i] is RGBColorSpaceForNonStroking || operations[i] is GrayColorSpaceForNonStroking
                    || operations[i] is CMYKColorSpaceForNonStroking)
                {
                    if (operations[i] is RGBColorSpaceForNonStroking)
                    {
                        RGBColorSpaceForNonStroking rgb = (RGBColorSpaceForNonStroking)operations[i];
                        _fontColor = new ColorRGB((byte)(rgb.R * 255), (byte)(rgb.G * 255), (byte)(rgb.B * 255));
                    }
                    else if (operations[i] is GrayColorSpaceForNonStroking)
                    {
                        GrayColorSpaceForNonStroking gray = (GrayColorSpaceForNonStroking)operations[i];
                        _fontColor = new ColorGray((byte)(gray.Gray * 255));
                    }
					else // CMYKColorSpaceForNonStroking
                    {
                        CMYKColorSpaceForNonStroking cmyk = (CMYKColorSpaceForNonStroking)operations[i];
                        _fontColor = new ColorCMYK((byte)(cmyk.C * 255), (byte)(cmyk.M * 255), (byte)(cmyk.Y * 255), (byte)(cmyk.K * 255));
                    }

                    return;
                }
            }

            _fontColor = new ColorRGB(0, 0, 0);
        }

        private void changedFontSize(object sender)
        {
            CreateApperance();
            setTextAttributes();
        }

        //TI, I, DS, RV
    }
}
