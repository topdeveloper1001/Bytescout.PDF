using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class CheckBox: ButtonField
#else
	/// <summary>
    /// Represents a check box field in the PDF form.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class CheckBox: ButtonField
#endif
	{
	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.CheckBox; } }

	    /// <summary>
	    /// Gets or sets a value indicating whether this Bytescout.PDF.CheckBox is checked.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Checked
	    {
		    get
		    {
			    PDFName v = Dictionary["V"] as PDFName;
			    if (v != null)
				    return v.GetValue() != "Off";
			    return false;
		    }
		    set
		    {
			    PDFName name = value ? new PDFName("Yes") : new PDFName("Off");

                PDFDictionary AP = Dictionary["AP"] as PDFDictionary;
                if (AP != null)
                {
                    PDFDictionary N = AP["N"] as PDFDictionary;
                    PDFDictionary D = AP["D"] as PDFDictionary;
                    //PDFDictionary R = AP["D"] as PDFDictionary;
                    if (value)
                    {
                        string[] keys = null;
                        if (N != null && N.Count > 0)
                        {
                            keys = N.GetKeys();
                        }
                        else if (D != null && D.Count > 0)
                        {
                            keys = D.GetKeys();
                        }
                        if (keys.Length > 0 && keys[0] != "Off")
                            name = new PDFName(keys[0]);
                        else if (keys.Length > 1 && keys[1] != "Off")
                            name = new PDFName(keys[1]);
                        else
                            name = new PDFName("Yes");
                    }
                    else
                    {
                        name = new PDFName("Off");
                    }
                }

                Dictionary.AddItem("AS", name);
			    Dictionary.AddItem("V", name);
			    Dictionary.AddItem("DV", name);
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.CheckBox class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the check box.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the check box.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the check box.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the check box.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the check box control.</param>
        public CheckBox(float left, float top, float width, float height, string name)
            : base(left, top, width, height, name, null)
        {
            Apperance.BorderColor = new ColorRGB(0, 0, 0);
            Checked = false;
            CreateApperance();
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.CheckBox class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the check box.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the check box control.</param>
        public CheckBox(RectangleF boundingBox, string name)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height, name) { }

        internal CheckBox(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

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

            CheckBox annot = new CheckBox(res, owner);
            annot.SetPage(Page, false);
            annot.SetPage(page, true);

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
            PDFDictionary n = new PDFDictionary();
            GraphicsTemplate off = new GraphicsTemplate(Width, Height, new float[] { 1, 0, 0, 1, 0, 0 });
            GraphicsTemplate yes = new GraphicsTemplate(Width, Height, new float[] { 1, 0, 0, 1, 0, 0 });

            DrawBackground(off);
            DrawBorder(off);
            DrawBackground(yes);
            DrawBorder(yes);
            drawTick(yes);

            n.AddItem("Off", off.GetDictionaryStream());
            n.AddItem("Yes", yes.GetDictionaryStream());
            ap.AddItem("N", n);
            Dictionary.AddItem("AP", ap);
        }

        private void drawTick(GraphicsTemplate xObj)
        {
            StringFormat format = new StringFormat();
            format.HorizontalAlign = HorizontalAlign.Center;
            format.VerticalAlign = VerticalAlign.Center;

            Brush br;
            if (BorderColor == null)
                br = new SolidBrush(new ColorRGB(0, 0, 0));
            else
                br = new SolidBrush(BorderColor);

            float fontSize = Math.Min(Width, Height) - 2;
            Font fnt = new Font(StandardFonts.ZapfDingbats, fontSize);
            xObj.DrawString("4", fnt, br, new RectangleF(0, 0, Width, Height), format);
        }

        //Opt
    }
}
