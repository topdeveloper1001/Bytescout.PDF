using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ComboBox: ChoiceField
#else
	/// <summary>
    /// Represents a combo box field in the PDF Form.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class ComboBox: ChoiceField
#endif
	{
	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.ComboBox; } }

	    /// <summary>
	    /// Gets or sets a value indicating whether the combo box includes an editable text box
	    /// as well as a drop-down list.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public new bool Editable
	    {
		    get { return base.Editable; }
		    set { base.Editable = value; }
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
			    Font.BaseFont.AddStringToEncoding(value);
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.ComboBox class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the combo box.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the combo box.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the combo box.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the combo box.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the combo box control.</param>
        public ComboBox(float left, float top, float width, float height, string name)
            : base(left, top, width, height, name, null)
        {
            Combo = true;
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.ComboBox class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the combo box.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the combo box control.</param>
        public ComboBox(RectangleF boundingBox, string name)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height, name) { }

        internal ComboBox(PDFDictionary dict, IDocumentEssential owner)
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
            ChoiceField.Copy(Dictionary, res);

            ComboBox annot = new ComboBox(res, owner);
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

        internal override void CreateApperance()
        {
        }
    }
}
