using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class EditBox: Field
#else
	/// <summary>
    /// Represents a text box field in the PDF form.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class EditBox: Field
#endif
	{
	    private Font _font;
	    private DeviceColor _fontColor;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.EditBox; } }

	    /// <summary>
	    /// Gets or sets a value indicating whether this Bytescout.PDF.EditBox is multiline.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Multiline
	    {
		    get { return getFlagAttribute(13); }
		    set
		    {
			    setFlagAttribute(13, value);
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether this Bytescout.PDF.EditBox is a password field.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Password
	    {
		    get { return getFlagAttribute(14); }
		    set
		    {
			    setFlagAttribute(14, value);
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether the text entered in the field represents the pathname
	    /// of a file whose contents are to be submitted as the value of the field.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool FileSelect
	    {
		    get { return getFlagAttribute(21); }
		    set { setFlagAttribute(21, value); }
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
	    /// Gets or sets a value indicating whether this Bytescout.PDF.EditBox is scrollable.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Scrollable
	    {
		    get { return !getFlagAttribute(24); }
		    set { setFlagAttribute(24, !value); }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether the field is automatically divided into as many equally
	    /// spaced positions, or combs, as the value of MaxLength, and the text is laid out into those combs.
	    /// <remarks>Meaningful only if the MaxLength property is set and the Multiline, Password
	    /// properties are false.</remarks>
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool InsertSpaces
	    {
		    get { return getFlagAttribute(25); }
		    set
		    {
			    if (MaxLength != -1 && !Password && !Multiline)
			    {
				    setFlagAttribute(25, value);
				    CreateApperance();
			    }
		    }
	    }

	    /// <summary>
	    /// Gets or sets a value indicating whether the value of this field should be represented as a rich text string.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool RichText
	    {
		    get { return getFlagAttribute(26); }
		    set { setFlagAttribute(26, value); }
	    }

	    /// <summary>
	    /// Gets or sets the maximum number of characters that can be entered in the text box.
	    /// </summary>
	    /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
	    public int MaxLength
	    {
		    get
		    {
			    PDFNumber maxLen = Dictionary["MaxLen"] as PDFNumber;
			    if (maxLen == null)
				    return -1;
			    return (int)maxLen.GetValue();
		    }
		    set
		    {
			    if (value < -1)
				    throw new ArgumentOutOfRangeException();

			    if (value == -1)
				    Dictionary.RemoveItem("MaxLen");
			    else
				    Dictionary.AddItem("MaxLen", new PDFNumber(value));
			    CreateApperance();
		    }
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
			    CreateApperance();
			    _font.ChangedFontSize += changedFontSize;
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
	    /// Gets or sets a value indicating how the text should be aligned within the edit box.
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
	    /// Gets or sets the text associated with the control.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string Text
	    {
		    get
		    {
			    PDFString text = Dictionary["V"] as PDFString;
			    if (text == null)
				    return "";
			    return text.GetValue();
		    }
		    set
		    {
			    if (value == null)
				    value = "";
                Dictionary.AddItem("V", new PDFString(value));
                Dictionary.AddItem("DV", new PDFString(value));
                CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the text rotation angle.
	    /// </summary>
	    /// <value cref="string"></value>
	    public RotationAngle TextRotationAngle
	    {
		    get { return Apperance.RotationAngle; }
		    set
		    {
			    Apperance.RotationAngle = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.EditBox class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the edit box.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the edit box.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the edit box.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the edit box.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the edit box control.</param>
        public EditBox(float left, float top, float width, float height, string name)
            : base(left, top, width, height, name, null)
        {
            Dictionary.AddItem("FT", new PDFName("Tx"));
            _font = new Font(StandardFonts.Helvetica, 12);
            _font.ChangedFontSize += changedFontSize;
            _fontColor = new ColorRGB(0, 0, 0);
            BorderColor = new ColorRGB(0, 0, 0);
            CreateApperance();
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.EditBox class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the edit box.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the edit box control.</param>
        public EditBox(RectangleF boundingBox, string name)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height, name) { }

	    internal EditBox(PDFDictionary dict, IDocumentEssential owner)
		    : this(dict, owner, true) { }

	    internal EditBox(PDFDictionary dict, IDocumentEssential owner, bool parseFont)
            : base(dict, owner)
        {
            if (parseFont)
                parseFontAndColor();
        }

	    internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                SetPage(page, true);
                ApplyOwner(owner);
                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);
            Field.CopyTo(Dictionary, res);

            string[] keys = { "MaxLen", "Q", "RV"};
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = Dictionary[keys[i]];
                if (obj != null)
                    res.AddItem(keys[i], obj.Clone());
            }

            EditBox annot = new EditBox(res, owner, false);
            annot.SetPage(Page, false);
            annot.SetPage(page, true);

            annot._font = _font;
            annot._font.ChangedFontSize += annot.changedFontSize;
            annot._fontColor = _fontColor;
            annot.setTextAttributes();

            Field.SetActions(this, annot);
            
            return annot;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            Owner = owner;
            setTextAttributes();

            if (OnMouseEnter != null)
                OnMouseEnter.ApplyOwner(owner);
            if (OnMouseExit != null)
                OnMouseExit.ApplyOwner(owner);
            if (OnMouseDown != null)
                OnMouseDown.ApplyOwner(owner);
            if (OnMouseUp != null)
                OnMouseUp.ApplyOwner(owner);
            if (OnReceiveFocus != null)
                OnReceiveFocus.ApplyOwner(owner);
            if (OnLoseFocus != null)
                OnLoseFocus.ApplyOwner(owner);
            if (OnPageOpen != null)
                OnPageOpen.ApplyOwner(owner);
            if (OnPageClose != null)
                OnPageClose.ApplyOwner(owner);
            if (OnPageVisible != null)
                OnPageVisible.ApplyOwner(owner);
            if (OnPageInvisible != null)
                OnPageInvisible.ApplyOwner(owner);
            if (OnKeyPressed != null)
                OnKeyPressed.ApplyOwner(owner);
            if (OnBeforeFormatting != null)
                OnBeforeFormatting.ApplyOwner(owner);
            if (OnChange != null)
                OnChange.ApplyOwner(owner);
            if (OnOtherFieldChanged != null)
                OnOtherFieldChanged.ApplyOwner(owner);
            if (OnActivated != null)
                OnActivated.ApplyOwner(owner);
        }

        internal override void CreateApperance()
        {
            PDFDictionary ap = new PDFDictionary();
            GraphicsTemplate n = new GraphicsTemplate(getRotateWidth(), getRotateHeight(), getRotateMatrix());
            drawBackground(n);
            drawBorder(n);
            drawText(n);
            ap.AddItem("N", n.GetDictionaryStream());
            Dictionary.AddItem("AP", ap);
        }

        private void drawBorder(GraphicsTemplate xObj)
        {
            if (BackgroundColor != null)
            {
                Brush br = new SolidBrush(BackgroundColor);
                xObj.DrawRectangle(br, new RectangleF(0, 0, getRotateWidth(), getRotateHeight()));
            }
        }

        private void drawBackground(GraphicsTemplate xObj)
        {
            if (BorderColor != null)
            {
                Pen pen = new SolidPen(BorderColor);
                pen.Width = Border.Width;
                if (Border.Style == BorderStyle.Underline)
                    xObj.DrawLine(pen, 0, getRotateHeight() - pen.Width / 2, getRotateWidth(), getRotateHeight() - pen.Width / 2);
                else
                {
                    RectangleF rect = new RectangleF(pen.Width / 2, pen.Width / 2, getRotateWidth() - pen.Width, getRotateHeight() - pen.Width);
                    if (Border.Style == BorderStyle.Dashed)
                        pen.DashPattern = Border.DashPattern;
                    xObj.DrawRectangle(pen, rect);

                    if (Border.Style == BorderStyle.Inset)
                    {
                        Pen insPen = new SolidPen(new ColorRGB(128, 128, 128));
                        insPen.Width = Border.Width;
                        xObj.DrawLine(insPen, insPen.Width, 1.5f * insPen.Width, Width - insPen.Width, 1.5f * insPen.Width);
                        xObj.DrawLine(insPen, 1.5f * insPen.Width, 1.5f * insPen.Width, 1.5f * insPen.Width, getRotateHeight() - insPen.Width);
                    }

                    if (Border.Style == BorderStyle.Inset || Border.Style == BorderStyle.Beveled)
                    {
                        float width = Border.Width;
                        Path path = new Path();
                        path.MoveTo(width, getRotateHeight() - width);
                        path.AddLineTo(2 * width, getRotateHeight() - 2 * width);
                        path.AddLineTo(Width - 2 * width, getRotateHeight() - 2 * width);
                        path.AddLineTo(getRotateWidth() - 2 * width, 2 * width);
                        path.AddLineTo(getRotateWidth() - width, width);
                        path.AddLineTo(getRotateWidth() - width, getRotateHeight() - width);
                        path.ClosePath();

                        Brush br = new SolidBrush(new ColorRGB(192, 192, 192));
                        xObj.DrawPath(br, path);
                    }
                }
            }
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

        private void drawText(GraphicsTemplate xObj)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                xObj.BeginStringEdit();
                xObj.Transform(1, 0, 0, 1, 0, getRotateHeight());

                string text = getDrawingText();

                if (Multiline)
                    drawMultilineText(text, xObj);
                else
                {
                    if (MaxLength != -1 && !Password && !Multiline && InsertSpaces)
                        drawInsertSpacesText(text, xObj);
                    else
                        drawOnelineText(text, xObj);
                }
                xObj.EndStringEdit();
            }
        }

        private string getDrawingText()
        {
            string text = Text;
            if (MaxLength != -1)
            {
                if (text.Length > MaxLength)
                    return text.Substring(0, MaxLength);
            }
            if (Password)
                return new string('*', text.Length);
            return text;
        }

        private void drawSpacesCells(GraphicsTemplate xObj)
        {
            if (BorderColor != null)
            {
                float w = getRotateWidth() / MaxLength;
                Pen pen = new SolidPen(BorderColor);
                if (BorderStyle == BorderStyle.Dashed)
                    pen.DashPattern = new DashPattern(new float[] { 3 });

                float curPos = 0;
                for (int i = 1; i < MaxLength; ++i)
                {
                    curPos += w;
                    xObj.DrawLine(pen, curPos, 0, curPos, Height);
                }
            }
        }

        private void drawInsertSpacesText(string text, GraphicsTemplate xObj)
        {
            if (BorderStyle == BorderStyle.Dashed || BorderStyle == BorderStyle.Solid)
                drawSpacesCells(xObj);

            float left = 0;
            
            float top = 0;
            float height = getRotateHeight();
            float width = getRotateWidth() / MaxLength;

            if (TextAlign == TextAlign.Right)
                left = (MaxLength - text.Length) * width;
            else if (TextAlign == TextAlign.Center)
                left = (MaxLength - text.Length) / 2 * width;

            StringFormat sf = new StringFormat();
            sf.HorizontalAlign = HorizontalAlign.Center;
            sf.VerticalAlign = VerticalAlign.Center;
            Brush br = new SolidBrush(FontColor);

            for (int i = 0; i < text.Length; ++i)
            {
                xObj.DrawString(new string(text[i], 1), Font, br, new RectangleF(left, top, width, height), sf);
                left += width;
            }
        }

        private void drawOnelineText(string text, GraphicsTemplate xObj)
        {
            float left = Border.Width * 2;
            if (Border.Style == BorderStyle.Beveled || Border.Style == BorderStyle.Inset)
                left *= 2;

            if (Font.Size == 0)
            {
                int w = (int)Width; // 72 pixels per inch
                Font tf = new Font(Font.Name, 12, Font.Bold, Font.Italic, Font.Underline, Font.Strikeout);
                float step = 0.5f;
                while (tf.GetTextWidth(text) > w && tf.Size > 4 + step)
                {
                    tf.Size -= step;
                }
                Font.Size = tf.Size;
                if (this.Dictionary["DA"] != null)
                    this.Dictionary.RemoveItem("DA");
            }

            StringFormat format = new StringFormat();
            format.VerticalAlign = VerticalAlign.Center;
            float y = getRotateHeight() / 2 - Font.GetTextHeight() / 2;

            if (TextAlign == TextAlign.Left)
            {
                format.HorizontalAlign = HorizontalAlign.Left;
                xObj.DrawString(text, Font, new SolidBrush(FontColor), left, y, format);
            }
            else if (TextAlign == TextAlign.Center)
            {
                if (Font.GetTextWidth(text) <= getRotateWidth() - 4 * left + 1.0f)
                {
                    format.HorizontalAlign = HorizontalAlign.Center;
                    xObj.DrawString(text, Font, new SolidBrush(FontColor), new RectangleF(0, 0, getRotateWidth(), getRotateHeight()), format);
                }
                else
                {
                    format.HorizontalAlign = HorizontalAlign.Left;
                    xObj.DrawString(text, Font, new SolidBrush(FontColor), left, y, format);
                }
            }
            else if (TextAlign == TextAlign.Right)
            {
                if (Font.GetTextWidth(text) <= getRotateWidth() - 4 * left + 1.0f)
                {
                    format.HorizontalAlign = HorizontalAlign.Right;
                    xObj.DrawString(text, Font, new SolidBrush(FontColor), new RectangleF(left, 0, getRotateWidth() - 2 * left, getRotateHeight()), format);
                }
                else
                {
                    format.HorizontalAlign = HorizontalAlign.Left;
                    xObj.DrawString(text, Font, new SolidBrush(FontColor), left, y, format);
                }
            }
        }

        private void drawMultilineText(string text, GraphicsTemplate xObj)
        {
            float left = Border.Width * 2;
            if (Border.Style == BorderStyle.Beveled || Border.Style == BorderStyle.Inset)
                left *= 2;

            StringFormat format = new StringFormat();
            format.VerticalAlign = VerticalAlign.Top;
            if (TextAlign == TextAlign.Center)
                format.HorizontalAlign = HorizontalAlign.Center;
            else if (TextAlign == TextAlign.Left)
                format.HorizontalAlign = HorizontalAlign.Left;
            else if (TextAlign == TextAlign.Right)
                format.HorizontalAlign = HorizontalAlign.Right;

            xObj.DrawString(text, Font, new SolidBrush(FontColor), new RectangleF(left, left + 2.5f, getRotateWidth() - 2 * left, getRotateHeight() - 2 * left), format);
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
                    //PDFDictionary fontDict = Owner.GetAcroFormFont(((TextFont)operations[i]).FontName.Substring(1));
                    PDFDictionary fontDict = Owner.GetAcroFormFont(((TextFont)operations[i]).FontName);
                    float fontSize = ((TextFont)operations[i]).FontSize;
                    if (fontDict != null)
                        _font = new Font(FontBase.Instance(fontDict));
                    else
                        _font = new Font(StandardFonts.Helvetica, 12);

                    _font.Size = fontSize;
                    break;
                }
            }

            if (_font == null)
                _font = new Font(StandardFonts.Helvetica, 12);
            // см. тикет #86
            //if (_font.Size == 0)
            //    _font.Size = 12;
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

        private float getRotateHeight()
        {
            if (TextRotationAngle == RotationAngle.Rotate90 || TextRotationAngle == RotationAngle.Rotate270)
                return Width;
            return Height;
        }

        private float getRotateWidth()
        {
            if (TextRotationAngle == RotationAngle.Rotate90 || TextRotationAngle == RotationAngle.Rotate270)
                return Height;
            return Width;
        }

        private float [] getRotateMatrix()
        {
            if (TextRotationAngle == RotationAngle.Rotate180)
                return new float[] { -1, 0, 0, -1, 0, 0 };
            if (TextRotationAngle == RotationAngle.Rotate90)
                return new float[] { 0, 1, -1, 0, Width, 0 };
            if (TextRotationAngle == RotationAngle.Rotate270)
                return new float[] { 0, -1, 1, 0, Width, 0 };
            return new float[] { 1, 0, 0, 1, 0, 0 };
        }

        private void changedFontSize(object sender)
        {
            CreateApperance();
            setTextAttributes();
        }

        //DS, RV
    }
}
