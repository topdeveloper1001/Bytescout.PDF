using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class FreeTextAnnotation: MarkupAnnotation
#else
	/// <summary>
    /// Represents a free text annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class FreeTextAnnotation: MarkupAnnotation
#endif
	{
	    private Font _font;
	    private DeviceColor _fontColor;
	    private AnnotationBorderStyle _borderStyle;
	    private AnnotationBorderEffect _borderEffect;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.FreeText; } }

	    /// <summary>
	    /// Gets or sets the x-coordinate of the left edge of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Left
	    {
		    get { return base.Left; }
		    set { base.Left = value; }
	    }

	    /// <summary>
	    /// Gets or sets the y-coordinate of the top edge of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Top
	    {
		    get { return base.Top; }
		    set { base.Top = value; }
	    }

	    /// <summary>
	    /// Gets or sets the width of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Width
	    {
		    get { return base.Width; }
		    set
		    {
			    base.Width = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the height of this annotation.
	    /// </summary>
	    /// <value cref="float" href="http://msdn.microsoft.com/en-us/library/system.single.aspx"></value>
	    public new float Height
	    {
		    get { return base.Height; }
		    set
		    {
			    base.Height = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the font used to display text in the annotation.
	    /// </summary>
	    /// <value cref="PDF.Font"></value>
	    public Font Font
	    {
		    get { return _font; }
		    set
		    {
			    if (value == null)
				    throw new NullReferenceException();

			    if (_font != null)
				    _font.ChangedFontSize -= changedFontSize;

			    _font = value;
			    setTextAttributes();
			    _font.BaseFont.AddStringToEncoding(Contents);
			    _font.ChangedFontSize += changedFontSize;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the color of the font used to display text in the annotation.
	    /// </summary>
	    /// <value cref="DeviceColor"></value>
	    public DeviceColor FontColor
	    {
		    get { return _fontColor; }
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
	    /// Gets or sets a value indicating how the text should be aligned within the annotation.
	    /// </summary>
	    /// <value cref="PDF.TextAlign"></value>
	    public TextAlign TextAlign
	    {
		    get
		    {
			    PDFNumber number = Dictionary["Q"] as PDFNumber;
			    if (number == null)
				    return TextAlign.Left;
			    return TypeConverter.PDFNumberToPDFTextAlign(number);
		    }
		    set
		    {
			    Dictionary.AddItem("Q", new PDFNumber((int)value));
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets the border style options for the annotation.
	    /// </summary>
	    /// <value cref="AnnotationBorderStyle"></value>
	    public AnnotationBorderStyle BorderStyle
	    {
		    get
		    {
			    if (_borderStyle == null)
			    {
				    PDFDictionary dict = Dictionary["BS"] as PDFDictionary;
				    if (dict == null)
				    {
					    _borderStyle = new AnnotationBorderStyle();
					    Dictionary.AddItem("BS", _borderStyle.GetDictionary());
				    }
				    else
				    {
					    _borderStyle = new AnnotationBorderStyle(dict);
				    }
				    _borderStyle.ChangedBorderStyle += changedBorderStyle;
			    }
			    return _borderStyle;
		    }
	    }

	    /// <summary>
	    /// Gets the border effect options for the annotation.
	    /// </summary>
	    /// <value cref="AnnotationBorderEffect"></value>
	    public AnnotationBorderEffect BorderEffect
	    {
		    get
		    {
			    if (_borderEffect == null)
			    {
				    PDFDictionary dict = Dictionary["BE"] as PDFDictionary;
				    if (dict == null)
				    {
					    _borderEffect = new AnnotationBorderEffect();
					    Dictionary.AddItem("BE", _borderEffect.GetDictionary());
				    }
				    else
				    {
					    _borderEffect = new AnnotationBorderEffect(dict);
				    }
				    _borderEffect.ChangedBorderEffect += changedBorderEffect;
			    }
			    return _borderEffect;
		    }
	    }

	    /// <summary>
	    /// Gets or sets the annotation's inner rectangle.
	    /// </summary>
	    /// <value cref="RectangleF" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx"></value>
		[ComVisible(false)]
		public RectangleF InnerRectangle
	    {
		    get
		    {
			    PDFArray rect = Dictionary["RD"] as PDFArray;
			    if (rect != null)
			    {
				    try
				    {
					    float x1 = (float)((PDFNumber) rect[0]).GetValue();
					    float y1 = (float)((PDFNumber) rect[1]).GetValue();
					    float x2 = (float)((PDFNumber) rect[2]).GetValue();
					    float y2 = (float)((PDFNumber) rect[3]).GetValue();

					    return new RectangleF(x1, Height - y1, Width - x2 - x1, y2 - (Height - y1));
				    }
				    catch
				    {
					    // ignored
				    }
			    }
			    return new RectangleF(0, 0, Width, Height);
		    }
		    set
		    {
			    PDFArray rect = new PDFArray();
			    rect.AddItem(new PDFNumber(value.Left));
			    rect.AddItem(new PDFNumber(Height - value.Top));
			    rect.AddItem(new PDFNumber(Width - (value.Left + value.Width)));
			    rect.AddItem(new PDFNumber(value.Bottom));
			    Dictionary.AddItem("RD", rect);
			    CreateApperance();
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.FreeTextAnnotation class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the annotation.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the annotation.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the annotation.</param>
        public FreeTextAnnotation(float left, float top, float width, float height)
            : base(left, top, width, height, null)
        {
            Dictionary.AddItem("Subtype", new PDFName("FreeText"));
            _font = new Font(StandardFonts.Helvetica, 12);
            _font.ChangedFontSize += changedFontSize;
            _fontColor = new ColorRGB(0, 0, 0);
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.FreeTextAnnotation class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the annotation.</param>
        public FreeTextAnnotation(RectangleF boundingBox)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height) { }

	    internal FreeTextAnnotation(PDFDictionary dict, IDocumentEssential owner)
		    : this(dict, owner, true) { }

	    internal FreeTextAnnotation(PDFDictionary dict, IDocumentEssential owner, bool parseFont)
            : base(dict, owner)
        {
            if (parseFont)
                parseFontAndColor();
        }

	    // TODO:
        //internal PointsArray CoordinatesLines
        //{ 
        //    get
        //    {
        //        if (m_coordinatesLines == null)
        //        {
        //            PDFArray array = Dictionary["CL"] as PDFArray;
        //            if (array == null)
        //            {
        //                m_coordinatesLines = new PointsArray();
        //                m_coordinatesLines.AddPoint(new PointF(Left, Top + Height / 2));
        //                m_coordinatesLines.AddPoint(new PointF(Left, Top + Height / 2));
        //                Dictionary.AddItem("CL", m_coordinatesLines.Array);
        //            }
        //            else
        //                m_coordinatesLines = new PointsArray(array, Page);
        //        }

        //        return m_coordinatesLines;
        //    }
        //}

        //internal PDFLineEndingStyle EndLineStyle
        //{
        //    get { return TypeConverter.PDFNameToPDFLineEndingStyle(Dictionary["LE"] as PDFName); }
        //    set { Dictionary.AddItem("LE", TypeConverter.PDFLineEndingStyleToPDFName(value)); }
        //}

        //TODO IT

        internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                ApplyOwner(owner);
                SetPage(page, true);
                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);
            MarkupAnnotationBase.CopyTo(Dictionary, res);

            string[] keys = { "DA", "Q", "RC", "DS", "IT", "BE", "RD", "LE" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = Dictionary[keys[i]];
                if (obj != null)
                    res.AddItem(keys[i], obj.Clone());
            }

            PDFArray cl = Dictionary["CL"] as PDFArray;
            if (cl != null)
            {
                RectangleF oldRect;
                if (Page == null)
                    oldRect = new RectangleF();
                else
                    oldRect = Page.PageRect;

                res.AddItem("CL", CloneUtility.CopyArrayCoordinates(cl, oldRect, page.PageRect, Page == null));
            }

            PDFDictionary bs = Dictionary["BS"] as PDFDictionary;
            if (bs != null)
                res.AddItem("BS", AnnotationBorderStyle.Copy(bs));

            FreeTextAnnotation annot = new FreeTextAnnotation(res, owner, false);
            annot.SetPage(Page, false);
            annot.SetPage(page, true);

            annot._font = _font;
            annot._font.ChangedFontSize += annot.changedFontSize;
            annot._fontColor = _fontColor;
            annot.setTextAttributes();

            return annot;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            Owner = owner;
            setTextAttributes();
        }

        internal override void CreateApperance()
        {
            Dictionary.RemoveItem("AP");
        }

        private void setTextAttributes()
        {
            if (Owner != null)
            {
                if (_font != null && _fontColor != null)
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
        }

        private void changedFontSize(object sender)
        {
            setTextAttributes();
            CreateApperance();
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
                    _font = fontDict != null 
						? new Font(FontBase.Instance(fontDict)) 
						: new Font(StandardFonts.Helvetica, 12);
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

        private void changedBorderStyle(object sender)
        {
            CreateApperance();
        }

        private void changedBorderEffect(object sender)
        {
            CreateApperance();
        }

        //private PointsArray m_coordinatesLines;
    }
}
