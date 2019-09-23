using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class PushButton: ButtonField
#else
	/// <summary>
    /// Represents push button field in the PDF form.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class PushButton: ButtonField
#endif
	{
	    private Font _font;
	    private DeviceColor _fontColor;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.PushButton; } }

	    /// <summary>
	    /// Gets or sets the highlighting mode.
	    /// </summary>
	    /// <value cref="PushButtonHighlightingMode"></value>
	    public PushButtonHighlightingMode HighlightingMode
	    {
		    get { return TypeConverter.PDFNameToPDFPushButtonHighlightingMode(Dictionary["H"] as PDFName); }
		    set { Dictionary.AddItem("H", TypeConverter.PDFPushButtonHighlightingModeToPDFName(value)); }
	    }

	    /// <summary>
	    /// Gets or sets the caption.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string Caption
	    {
		    get { return Apperance.NormalCaption; }
		    set
		    {
			    Apperance.NormalCaption = value;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the font used to display text in the control.
	    /// </summary>
	    /// <value cref="PDF.Font"></value>
	    public Font Font
	    {
		    get { return _font; }
		    set
		    {
			    if (value == null)
				    throw new NullReferenceException();

			    _font.ChangedFontSize -= changedFontSize;
			    _font = value;
			    _font.ChangedFontSize += changedFontSize;
			    CreateApperance();
		    }
	    }

	    /// <summary>
	    /// Gets or sets the color of the font used to display text in the control.
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
			    CreateApperance();
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.PushButton class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the button.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the button.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the button.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the button.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the button control.</param>
        public PushButton(float left, float top, float width, float height, string name)
            : base(left, top, width, height, name, null)
        {
            Pushbutton = true;
            Apperance.BackgroundColor = new ColorGray(153);
            Apperance.BorderColor = new ColorRGB(0, 0, 0);
            _font = new Font(StandardFonts.Helvetica, 12);
            _font.ChangedFontSize += changedFontSize;
            _fontColor = new ColorRGB(0, 0, 0);
            CreateApperance();
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.PushButton class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the button.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the button control.</param>
        public PushButton(RectangleF boundingBox, string name)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height, name) { }

	    internal PushButton(PDFDictionary dict, IDocumentEssential owner)
		    : this(dict, owner, true) { }

	    internal PushButton(PDFDictionary dict, IDocumentEssential owner, bool createFont)
            : base(dict, owner)
        {
            if (createFont)
            {
                _font = new Font(StandardFonts.Helvetica, 12);
                _font.ChangedFontSize += changedFontSize;
                _fontColor = new ColorRGB(0, 0, 0);
            }
        }

	    internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                ApplyOwner(owner);
                SetPage(page, true);
                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);
            Field.CopyTo(Dictionary, res);

            PushButton annot = new PushButton(res, owner, false);
            annot.SetPage(Page, false);
            annot.SetPage(page, true);

            annot._fontColor = _fontColor;
            annot._font = _font;
            annot._font.ChangedFontSize += annot.changedFontSize;

            Field.SetActions(this, annot);

            return annot;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            Owner = owner;
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
            GraphicsTemplate n = new GraphicsTemplate(Width, Height, new float[] { 1, 0, 0, 1, 0, 0 });
                        
            DrawBackground(n);
            DrawBorder(n);
            drawText(n);
            
            ap.AddItem("N", n.GetDictionaryStream());
            ap.AddItem("D", n.GetDictionaryStream());
            Dictionary.AddItem("AP", ap);
        }

        private void drawText(GraphicsTemplate xObj)
        {
            StringFormat format = new StringFormat();
            format.VerticalAlign = VerticalAlign.Center;
            format.HorizontalAlign = HorizontalAlign.Center;
            xObj.DrawString(Caption, _font, new SolidBrush(_fontColor), new RectangleF(0, 0, Width, Height), format);
        }

        private void changedFontSize(object sender)
        {
            CreateApperance();
        }
    }
}
