using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class RadioButton: ButtonField
#else
	/// <summary>
    /// Represents a radio button field in the PDF form.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class RadioButton: ButtonField
#endif
	{
	    private string _exportValue;

	    /// <summary>
	    /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
	    /// </summary>
	    /// <value cref="AnnotationType"></value>
	    public override AnnotationType Type { get { return AnnotationType.RadioButton; } }

	    /// <summary>
	    /// Gets or sets a value indicating whether this Bytescout.PDF.RadioButton is checked.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Checked
	    {
		    get
		    {
			    PDFName v = Dictionary["V"] as PDFName;
			    if (v != null)
				    return v.GetValue() != "Off";
			    else
			    {
				    PDFDictionary parent = Dictionary["Parent"] as PDFDictionary;
				    if (parent != null)
				    {
					    PDFName dv = parent["DV"] as PDFName;
					    if (dv != null)
					    {
						    if (_exportValue == dv.GetValue())
							    return true;
					    }
				    }
				    else
				    {
					    PDFName AS = Dictionary["AS"] as PDFName;
					    if (AS != null && AS.GetValue() == "Off")
						    return false;
					    else
						    return true;
				    }
			    }
			    return false;
		    }
		    set
		    {
			    PDFName name = value ? new PDFName(_exportValue) : new PDFName("Off");

			    Dictionary.AddItem("AS", name);

			    PDFDictionary parent = Dictionary["Parent"] as PDFDictionary;
			    if (parent != null)
			    {
				    PDFName v = parent["V"] as PDFName;
				    if (v != null)
				    {
					    if (v.GetValue() == _exportValue)
					    {
						    parent.AddItem("V", name);
						    parent.AddItem("DV", name);
					    }
				    }
			    }
		    }
	    }

	    /// <summary>
	    /// Gets or sets the export value of the radio button.
	    /// </summary>
	    /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
	    public string ExportValue
	    {
		    get { return _exportValue; }
		    set
		    {
			    if (string.IsNullOrEmpty(value))
				    throw new ArgumentNullException();

			    try
			    {
				    PDFDictionary ap = Dictionary["AP"] as PDFDictionary;
				    PDFDictionary n = ap["N"] as PDFDictionary;
				    IPDFObject obj = n[_exportValue];
				    n.RemoveItem(_exportValue);

				    PDFDictionary parent = Dictionary["Parent"] as PDFDictionary;
				    if (parent != null)
				    {
					    PDFName v = parent["V"] as PDFName;
					    if (v != null)
					    {
						    if (v.GetValue() == _exportValue)
						    {
							    parent.AddItem("V", new PDFName(value));
							    parent.AddItem("DV", new PDFName(value));
						    }
					    }
				    }

				    PDFName AS = Dictionary["AS"] as PDFName;
				    if (AS != null && AS.GetValue() == _exportValue)
					    Dictionary.AddItem("AS", new PDFName(value));

				    _exportValue = value;
				    n.AddItem(_exportValue, obj);
			    }
			    catch
			    {
				    // ignored
			    }
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.RadioButton class.
        /// </summary>
        /// <param name="left" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The x-coordinate of the upper-left corner of the button.</param>
        /// <param name="top" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The y-coordinate of the upper-left corner of the button.</param>
        /// <param name="width" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The width of the button.</param>
        /// <param name="height" href="http://msdn.microsoft.com/en-us/library/system.single.aspx">The height of the button.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the button control.</param>
        /// <param name="exportValue" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The export value of the radio button.</param>
        public RadioButton(float left, float top, float width, float height, string name, string exportValue)
            : base(left, top, width, height, name, null)
        {
            if (string.IsNullOrEmpty(exportValue))
                throw new ArgumentNullException("exportValue");

            Apperance.BorderColor = new ColorRGB(0, 0, 0);
            Radio = true;
            _exportValue = exportValue;
            CreateApperance();
            NoToggleToOff = true;
            Checked = false;
        }

        /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.RadioButton class.
        /// </summary>
        /// <param name="boundingBox" href="http://msdn.microsoft.com/en-us/library/system.drawing.rectanglef.aspx">Bounds of the button.</param>
        /// <param name="name" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The unique name of the button control.</param>
        /// <param name="exportValue" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The export value of the radio button.</param>
        public RadioButton(RectangleF boundingBox, string name, string exportValue)
            : this(boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height, name, exportValue) { }

        internal RadioButton(PDFDictionary dict, IDocumentEssential owner)
            : base(dict, owner)
        {
            PDFDictionary ap = dict["AP"] as PDFDictionary;
            if (ap != null)
            {
                PDFDictionary n = ap["N"] as PDFDictionary;
                if (n != null)
                {
                    string[] keys = n.GetKeys();
                    if (keys.Length == 0)
                        _exportValue = "value1";
                    else if (keys.Length == 1)
                        _exportValue = keys[0];
                    else
                    {
                        _exportValue = keys[0] == "Off" ? keys[1] : keys[0];
                    }
                }
                else
                    _exportValue = "value1";
            }
            else
                _exportValue = "value1";
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

            RadioButton annot = new RadioButton(res, owner);
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

            drawContents(off);            
            drawContents(yes);
            drawTick(yes);

            n.AddItem("Off", off.GetDictionaryStream());
            n.AddItem(_exportValue, yes.GetDictionaryStream());
            ap.AddItem("N", n);
            Dictionary.AddItem("AP", ap);
        }

        private void drawContents(GraphicsTemplate xObj)
        {
            if (BackgroundColor != null)
            {
                Brush br = new SolidBrush(BackgroundColor);
                xObj.DrawCircle(br, Width / 2, Height / 2, Math.Min(Width, Height) / 2 - BorderWidth);
            }

            if (BorderColor != null)
            {
                Pen pen = new SolidPen(BorderColor);
                pen.Width = BorderWidth;
                xObj.DrawCircle(pen, Width / 2, Height / 2, Math.Min(Width, Height) / 2 - BorderWidth);
            }
        }

        private void drawTick(GraphicsTemplate xObj)
        {
	        Brush br = BorderColor == null 
				? new SolidBrush() 
				: new SolidBrush(BorderColor);
	        xObj.DrawCircle(br, Width / 2, Height / 2, Math.Min(Width, Height) / 4 - BorderWidth);
        }

	    //Opt
    }
}
