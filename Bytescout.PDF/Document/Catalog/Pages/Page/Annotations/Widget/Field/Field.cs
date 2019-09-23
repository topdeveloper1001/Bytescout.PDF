using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class Field: Widget
#else
	/// <summary>
    /// Represents an abstract class for fields of the PDF document's interactive form.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class Field: Widget
#endif
	{
	    private JavaScriptAction _onKeyPressed;
	    private JavaScriptAction _onBeforeFormatting;
	    private JavaScriptAction _onChange;
	    private JavaScriptAction _onOtherFieldChanged;

	    /// <summary>
        /// Gets the partial field name.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        public string Name
        {
            get
            {
                PDFString t = Dictionary["T"] as PDFString;
                if (t != null)
                    return t.GetValue();

                PDFDictionary parent = Dictionary["Parent"] as PDFDictionary;
                if (parent != null)
                {
                    t = parent["T"] as PDFString;
                    if (t != null)
                        return t.GetValue();
                }

                return "";
            }
            private set
            {
                if (value == null)
                    return;
                Dictionary.AddItem("T", new PDFString(value));
            }
        }

        /// <summary>
        /// Gets or sets the mapping name to be used when exporting interactive form field data from the document.
        /// </summary>
        /// <value cref="string" href="http://msdn.microsoft.com/en-us/library/system.string.aspx"></value>
        internal string MappingName
        {
            get
            {
                PDFString t = Dictionary["TM"] as PDFString;
                if (t != null)
                    return t.GetValue();
                return "";
            }
            set
            {
                if (value == null)
                    value = "";
                Dictionary.AddItem("TM", new PDFString(value));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow the user to change the value of the field.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public override bool ReadOnly
        {
            get { return getFlagAttribute(1); }
            set { setFlagAttribute(1, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this Bytescout.PDF.Field is required.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool Required
        {
            get { return getFlagAttribute(2); }
            set { setFlagAttribute(2, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this Bytescout.PDF.Field is not exportable.
        /// </summary>
        /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
        public bool NoExport
        {
            get { return getFlagAttribute(3); }
            set { setFlagAttribute(3, value); }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the user types a keystroke into a text field or combo box
        /// or modifies the selection in a scrollable list box.
        /// </summary>
        /// <value cref="JavaScriptAction"></value>
        public JavaScriptAction OnKeyPressed
        {
            get
            {
                if (_onKeyPressed == null)
                    loadAdditionalAction("K");
                return _onKeyPressed;
            }
            set
            {
                setAdditionalAction(value, "K");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed before the field is formatted to display its current value.
        /// </summary>
        /// <value cref="JavaScriptAction"></value>
        public JavaScriptAction OnBeforeFormatting
        {
            get
            {
                if (_onBeforeFormatting == null)
                    loadAdditionalAction("F");
                return _onBeforeFormatting;
            }
            set
            {
                setAdditionalAction(value, "F");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the field’s value is changed.
        /// </summary>
        /// <value cref="JavaScriptAction"></value>
        public JavaScriptAction OnChange
        {
            get
            {
                if (_onChange == null)
                    loadAdditionalAction("V");
                return _onChange;
            }
            set
            {
                setAdditionalAction(value, "V");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed to recalculate the value of this field when that of another field changes.
        /// </summary>
        /// <value cref="JavaScriptAction"></value>
        public JavaScriptAction OnOtherFieldChanged
        {
            get
            {
                if (_onOtherFieldChanged == null)
                    loadAdditionalAction("C");
                return _onOtherFieldChanged;
            }
            set
            {
                setAdditionalAction(value, "C");
            }
        }

        internal string AlternateName
        {
            get
            {
                PDFString t = Dictionary["TU"] as PDFString;
                if (t != null)
                    return t.GetValue();
                return "";
            }
            set
            {
                if (value == null)
                    return;
                Dictionary.AddItem("TU", new PDFString(value));
            }
        }

	    internal uint Ff
        {
            get
            {
                PDFNumber f = Dictionary["Ff"] as PDFNumber;
                if (f == null)
                    return 0;
                return (uint)f.GetValue();
            }
            set
            {
                Dictionary.AddItem("Ff", new PDFNumber(value));
            }
        }

	    internal Field(float left, float top, float width, float height, string name, IDocumentEssential owner)
		    : base(left, top, width, height, owner)
	    {
		    if (string.IsNullOrEmpty(name))
			    throw new ArgumentNullException("name");
		    Name = name;
	    }

	    internal Field(PDFDictionary dict, IDocumentEssential owner)
		    : base(dict, owner) { }

	    internal static void CopyTo(PDFDictionary sourceDict, PDFDictionary destinationDict)
	    {
		    string[] keys = { "H", "FT", "T", "TU", "TM", "Ff", "V", "DV" };
		    for (int i = 0; i < keys.Length; ++i)
		    {
			    IPDFObject obj = sourceDict[keys[i]];
			    if (obj != null)
				    destinationDict.AddItem(keys[i], obj.Clone());
		    }

		    PDFDictionary bs = sourceDict["BS"] as PDFDictionary;
		    if (bs != null)
			    destinationDict.AddItem("BS", AnnotationBorderStyle.Copy(bs));

		    PDFDictionary mk = sourceDict["MK"] as PDFDictionary;
		    if (mk != null)
			    destinationDict.AddItem("MK", AppearanceCharacteristics.Copy(mk));
	    }

	    internal void DrawBackground(GraphicsTemplate xObj)
        {
            if (BackgroundColor != null)
            {
                Brush br = new SolidBrush(BackgroundColor);
                xObj.DrawRectangle(br, new RectangleF(0, 0, Width, Height));
            }
        }

        internal void DrawBorder(GraphicsTemplate xObj)
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

        internal static void SetActions(Field source, Field destionation)
        {
            destionation.OnMouseEnter = source.OnMouseEnter;
            destionation.OnMouseExit = source.OnMouseExit;
            destionation.OnMouseDown = source.OnMouseDown;
            destionation.OnMouseUp = source.OnMouseUp;
            destionation.OnReceiveFocus = source.OnReceiveFocus;
            destionation.OnLoseFocus = source.OnLoseFocus;
            destionation.OnPageOpen = source.OnPageOpen;
            destionation.OnPageClose = source.OnPageClose;
            destionation.OnPageVisible = source.OnPageVisible;
            destionation.OnPageInvisible = source.OnPageInvisible;

            destionation.OnKeyPressed = source.OnKeyPressed;
            destionation.OnBeforeFormatting = source.OnBeforeFormatting;
            destionation.OnChange = source.OnChange;
            destionation.OnOtherFieldChanged = source.OnOtherFieldChanged;

            destionation.OnActivated = source.OnActivated;
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

        private void loadAdditionalAction(string name)
        {
            PDFDictionary aa = Dictionary["AA"] as PDFDictionary;
            if (aa != null)
            {
                PDFDictionary dict = aa[name] as PDFDictionary;
                if (dict != null)
                    setActionValue(new JavaScriptAction(dict, Owner), name);
            }
        }

        private void setAdditionalAction(JavaScriptAction action, string key)
        {
            if (action == null)
            {
                setActionValue(null, key);
                if (Dictionary["AA"] as PDFDictionary != null)
                {
                    (Dictionary["AA"] as PDFDictionary).RemoveItem(key);
                    if ((Dictionary["AA"] as PDFDictionary).Count == 0)
                        Dictionary.RemoveItem("AA");
                }
            }
            else
            {
                JavaScriptAction a = (JavaScriptAction)action.Clone(Owner);
                setActionValue(a, key);
                if (Dictionary["AA"] as PDFDictionary == null)
                    Dictionary.AddItem("AA", new PDFDictionary());
                (Dictionary["AA"] as PDFDictionary).AddItem(key, a.GetDictionary());
            }
        }

        private void setActionValue(JavaScriptAction action, string key)
        {
            switch (key)
            {
                case "K":
                    _onKeyPressed = action;
                    break;
                case "F":
                    _onBeforeFormatting = action;
                    break;
                case "V":
                    _onChange = action;
                    break;
                case "C":
                    _onOtherFieldChanged = action;
                    break;
            }
        }
    }
}
