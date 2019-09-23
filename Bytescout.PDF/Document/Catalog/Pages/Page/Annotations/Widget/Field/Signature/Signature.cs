using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class Signature: Field
#else
    /// <summary>
    /// Represents a signature field in the PDF form.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    internal class Signature : Field
#endif
    {
        private Font _font;
        private DeviceColor _fontColor;
        private bool _visible = false;
        private string _name;
        private string _reason;
        private string _contact;
        private string _location;

        /// <summary>
        /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
        /// </summary>
        /// <value cref="AnnotationType"></value>
        public override AnnotationType Type { get { return AnnotationType.Signature; } }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Signature class.
        /// </summary>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the signature.</param>
        public Signature(string name, PDFDictionary sig)
            : base(0, 0, 0, 0, name, null)
        {
            Dictionary.AddItem("FT", new PDFName("Sig"));
            Flag = 132; // Invisible signature

            CreateApperance();
            Dictionary.AddItem("V", sig);
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.Signature class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the signature box.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the signature box.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the signature box.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the signature box.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the signature.</param>
        public Signature(float left, float top, float width, float height, string name, PDFDictionary sig)
            : base(left, top, width, height, name, null)
        {
            Dictionary.AddItem("FT", new PDFName("Sig"));
            Flag = 132; // ???

            _visible = true;
            _font = new Font(StandardFonts.Helvetica, 12);
            //_font.ChangedFontSize += changedFontSize;
            _fontColor = new ColorRGB(0, 0, 0);
            BorderColor = new ColorRGB(0, 0, 0);
            CreateApperance();
            Dictionary.AddItem("V", sig);
        }

        public Signature(float left, float top, float width, float height, string name, PDFDictionary sig, string reason, string contact, string location)
            : base(left, top, width, height, name, null)
        {
            Dictionary.AddItem("FT", new PDFName("Sig"));
            Flag = 132; // ???

            PDFString sname = (PDFString )sig["Name"];
            if (sname != null)
                _name = sname.ToString();
            _reason = reason;
            _contact = contact;
            _location = location;
            _visible = true;
            _font = new Font(StandardFonts.Helvetica, 12);
            //_font.ChangedFontSize += changedFontSize;
            _fontColor = new ColorRGB(0, 0, 0);
            BorderColor = new ColorRGB(0, 0, 0);
            CreateApperance();
            Dictionary.AddItem("V", sig);
        }

        internal Signature(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

        internal override Annotation Clone(IDocumentEssential owner, Page page)
        {
            if (Page == null)
            {
                //SetPage(page, true);
                SetPage(page, false);
                ApplyOwner(owner);
                return this;
            }

            PDFDictionary res = AnnotationBase.Copy(Dictionary);
            Field.CopyTo(Dictionary, res);

            //string[] keys = { "MaxLen", "Q", "RV" };
            //for (int i = 0; i < keys.Length; ++i)
            //{
            //    IPDFObject obj = Dictionary[keys[i]];
            //    if (obj != null)
            //        res.AddItem(keys[i], obj.Clone());
            //}

            Signature annot = new Signature(res, owner/*, false*/);
            annot.SetPage(Page, false);
            annot.SetPage(page, true);

            annot._font = _font;
            //annot._font.ChangedFontSize += annot.changedFontSize;
            //annot._fontColor = _fontColor;
            //annot.setTextAttributes();

            Field.SetActions(this, annot);

            return annot;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            Owner = owner;
            //setTextAttributes();

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
            GraphicsTemplate n = new GraphicsTemplate(Width, Height, new float[] {1, 0, 0, 1, 0, 0});
            if (_visible)
            {
                drawBackground(n);
                drawBorder(n);
                drawText(n);
            }
            ap.AddItem("N", n.GetDictionaryStream());
            Dictionary.AddItem("AP", ap);
        }

        private void drawBorder(GraphicsTemplate xObj)
        {
            if (BackgroundColor != null)
            {
                Brush br = new SolidBrush(BackgroundColor);
                xObj.DrawRectangle(br, new RectangleF(0, 0, Width, Height));
            }
        }

        private void drawBackground(GraphicsTemplate xObj)
        {
            if (BorderColor != null)
            {
                Pen pen = new SolidPen(BorderColor);
                pen.Width = Border.Width;
                if (Border.Style == BorderStyle.Underline)
                    xObj.DrawLine(pen, 0, Height - pen.Width / 2, Width, Height - pen.Width / 2);
                else
                {
                    RectangleF rect = new RectangleF(pen.Width / 2, pen.Width / 2, Width - pen.Width, Height - pen.Width);
                    if (Border.Style == BorderStyle.Dashed)
                        pen.DashPattern = Border.DashPattern;
                    xObj.DrawRectangle(pen, rect);

                    if (Border.Style == BorderStyle.Inset)
                    {
                        Pen insPen = new SolidPen(new ColorRGB(128, 128, 128));
                        insPen.Width = Border.Width;
                        xObj.DrawLine(insPen, insPen.Width, 1.5f * insPen.Width, Width - insPen.Width, 1.5f * insPen.Width);
                        xObj.DrawLine(insPen, 1.5f * insPen.Width, 1.5f * insPen.Width, 1.5f * insPen.Width, Height - insPen.Width);
                    }

                    if (Border.Style == BorderStyle.Inset || Border.Style == BorderStyle.Beveled)
                    {
                        float width = Border.Width;
                        Path path = new Path();
                        path.MoveTo(width, Height - width);
                        path.AddLineTo(2 * width, Height - 2 * width);
                        path.AddLineTo(Width - 2 * width, Height - 2 * width);
                        path.AddLineTo(Width - 2 * width, 2 * width);
                        path.AddLineTo(Width - width, width);
                        path.AddLineTo(Width - width, Height - width);
                        path.ClosePath();

                        Brush br = new SolidBrush(new ColorRGB(192, 192, 192));
                        xObj.DrawPath(br, path);
                    }
                }
            }
        }

        private void drawText(GraphicsTemplate xObj)
        {
            StringBuilder sb = new StringBuilder();
            if (_name != null && _name != "")
                sb.Append("Digitally signed by: " + _name + "\n");
            sb.Append("Date: " + DateTime.Now.ToString() + "\n");
            if (_reason != null && _reason != "")
                sb.Append("Reason: " + _reason + "\n");
            if (_location != null && _location != "")
                sb.Append("Location: " + _location);
            string text = sb.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                xObj.BeginStringEdit();
                xObj.Transform(1, 0, 0, 1, 0, Height);

                drawMultilineText(text, xObj);

                xObj.EndStringEdit();
            }
        }


        private void drawMultilineText(string text, GraphicsTemplate xObj)
        {
            float left = Border.Width * 2;
            if (Border.Style == BorderStyle.Beveled || Border.Style == BorderStyle.Inset)
                left *= 2;

            StringFormat format = new StringFormat();
            format.VerticalAlign = VerticalAlign.Top;
            //if (TextAlign == TextAlign.Center)
            //    format.HorizontalAlign = HorizontalAlign.Center;
            //else if (TextAlign == TextAlign.Left)
                format.HorizontalAlign = HorizontalAlign.Left;
            //else if (TextAlign == TextAlign.Right)
            //    format.HorizontalAlign = HorizontalAlign.Right;

                xObj.DrawString(text, _font, new SolidBrush(_fontColor), new RectangleF(left, left + 2.5f, Width - 2 * left, Height - 2 * left), format);
        }
    }
}
