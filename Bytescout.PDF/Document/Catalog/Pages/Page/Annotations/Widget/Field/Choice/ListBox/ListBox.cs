using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ListBox: ChoiceField
#else
	/// <summary>
    /// Represents a list box field of the PDF form.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class ListBox: ChoiceField
#endif
	{
	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.ListBox; } }

	    /// <summary>
	    /// Gets or sets a value indicating whether more than one of the field’s option items
	    /// may be selected simultaneously.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public new bool MultiSelect
	    {
		    get { return base.MultiSelect; }
		    set { base.MultiSelect = value; }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.ListBox class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the list box.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the list box.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the list box.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the list box.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the list box control.</param>
        public ListBox(float left, float top, float width, float height, string name)
            : base(left, top, width, height, name, null) { }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.ListBox class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the list box.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the list box control.</param>
        public ListBox(RectangleF boundingBox, string name)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height, name) { }

        internal ListBox(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner) { }

	    internal override void CreateApperance()
        {
            PDFDictionary ap = new PDFDictionary();
            GraphicsTemplate n = new GraphicsTemplate(Width, Height, new float[] { 1, 0, 0, 1, 0, 0 });

            DrawBackground(n);
            DrawBorder(n);
            drawText(n);

            ap.AddItem("N", n.GetDictionaryStream());
            Dictionary.AddItem("AP", ap);
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
            ChoiceField.Copy(Dictionary, res);

            ListBox annot = new ListBox(res, owner);
            annot.SetPage(Page, false);
            annot.SetPage(page, true);

            annot.SetTextProperties(Font, FontColor);
            Field.SetActions(this, annot);

            return annot;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            Owner = owner;
            SetTextProperties(Font, FontColor);

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

        private void drawText(GraphicsTemplate xObj)
        {
            float left = Border.Width * 2;
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < Items.Count; ++i)
            {
                text.Append(Items[i]);
                if (i != Items.Count - 1)
                    text.Append('\n');
            }

            StringFormat format = new StringFormat();
            format.VerticalAlign = VerticalAlign.Top;
            if (TextAlign == TextAlign.Center)
                format.HorizontalAlign = HorizontalAlign.Center;
            else if (TextAlign == TextAlign.Left)
                format.HorizontalAlign = HorizontalAlign.Left;
            else if (TextAlign == TextAlign.Right)
                format.HorizontalAlign = HorizontalAlign.Right;
            xObj.BeginStringEdit();
            xObj.Transform(1, 0, 0, 1, 0, Height);
            xObj.DrawString(text.ToString(), Font, new SolidBrush(FontColor), new RectangleF(left, left + 2.5f, Width - 2 * left, Height - 2 * left), format);
            xObj.EndStringEdit();
        }
    }
}
