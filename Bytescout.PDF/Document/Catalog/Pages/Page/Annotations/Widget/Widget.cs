using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal abstract class Widget: Annotation
#else
	/// <summary>
    /// Represents an abstract class for widget annotations.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class Widget: Annotation
#endif
	{
	    private AnnotationBorderStyle _borderStyle;
	    private AppearanceCharacteristics _appearanceCharacteristics;
	    private Action _onMouseEnter;
	    private Action _onMouseExit;
	    private Action _onMouseDown;
	    private Action _onMouseUp;
	    private Action _onReceiveFocus;
	    private Action _onLoseFocus;
	    private Action _onPageOpen;
	    private Action _onPageClose;
	    private Action _onPageVisible;
	    private Action _onPageInvisible;
	    private Action _onActivated;

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
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value cref="DeviceColor"></value>
        public DeviceColor BackgroundColor
        {
            get { return Apperance.BackgroundColor; }
            set
            {
                Apperance.BackgroundColor = value;
                CreateApperance();
            }
        }

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        /// <value cref="DeviceColor"></value>
        public DeviceColor BorderColor
        {
            get { return Apperance.BorderColor; }
            set
            {
                Apperance.BorderColor = value;
                CreateApperance();
            }
        }

        /// <summary>
        /// Gets or sets the border style.
        /// </summary>
        /// <value cref="PDF.BorderStyle"></value>
        public BorderStyle BorderStyle
        {
            get { return Border.Style; }
            set
            {
                Border.Style = value;
                CreateApperance();
            }
        }

        /// <summary>
        /// Gets or sets the width of the border.
        /// </summary>
        /// <value cref="int" href="http://msdn.microsoft.com/en-us/library/system.int32.aspx"></value>
        public int BorderWidth
        {
            get { return (int)Border.Width; }
            set
            {
                Border.Width = value;
                CreateApperance();
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the annotation is activated.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnActivated
        {
            get
            {
                if (_onActivated == null)
                {
                    PDFDictionary dict = Dictionary["A"] as PDFDictionary;
                    if (dict != null)
                        _onActivated = Action.Create(dict, Owner);
                }

                return _onActivated;
            }
            set
            {
                if (value == null)
                {
                    _onActivated = null;
                    Dictionary.RemoveItem("A");
                }
                else
                {
                    _onActivated = value.Clone(Owner);
                    Dictionary.AddItem("A", _onActivated.GetDictionary());
                }
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the cursor enters the annotation’s active area.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnMouseEnter
        {
            get
            {
                if (_onMouseEnter == null)
                    loadAdditionalAction("E");
                return _onMouseEnter;
            }
            set
            {
                setAdditionalAction(value, "E");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the cursor exits the annotation’s active area.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnMouseExit
        {
            get
            {
                if (_onMouseExit == null)
                    loadAdditionalAction("X");
                return _onMouseExit;
            }
            set
            {
                setAdditionalAction(value, "X");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the mouse button is pressed inside the annotation’s active area.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnMouseDown
        {
            get
            {
                if (_onMouseDown == null)
                    loadAdditionalAction("D");
                return _onMouseDown;
            }
            set
            {
                setAdditionalAction(value, "D");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the mouse button is released inside the annotation’s active area.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnMouseUp
        {
            get
            {
                if (_onMouseUp == null)
                    loadAdditionalAction("U");
                return _onMouseUp;
            }
            set
            {
                setAdditionalAction(value, "U");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the annotation receives the input focus.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnReceiveFocus
        {
            get
            {
                if (_onReceiveFocus == null)
                    loadAdditionalAction("Fo");
                return _onReceiveFocus;
            }
            set
            {
                setAdditionalAction(value, "Fo");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the annotation loses the input focus.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnLoseFocus
        {
            get
            {
                if (_onLoseFocus == null)
                    loadAdditionalAction("Bl");
                return _onLoseFocus;
            }
            set
            {
                setAdditionalAction(value, "Bl");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the page containing the annotation is opened — for example, when the user navigates
        /// to it from the next or previous page or by means of a link annotation or outline item.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnPageOpen
        {
            get
            {
                if (_onPageOpen == null)
                    loadAdditionalAction("PO");
                return _onPageOpen;
            }
            set
            {
                setAdditionalAction(value, "PO");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the page containing the annotation is closed — for example, when the user navigates
        /// to the next or previous page, or follows a link annotation or outline item.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnPageClose
        {
            get
            {
                if (_onPageClose == null)
                    loadAdditionalAction("PC");
                return _onPageClose;
            }
            set
            {
                setAdditionalAction(value, "PC");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the page containing the annotation becomes
        /// visible in the viewer application’s user interface.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnPageVisible
        {
            get
            {
                if (_onPageVisible == null)
                    loadAdditionalAction("PV");
                return _onPageVisible;
            }
            set
            {
                setAdditionalAction(value, "PV");
            }
        }

        /// <summary>
        /// Gets or sets the action to be performed when the page containing the annotation is no longer
        /// visible in the viewer application’s user interface.
        /// </summary>
        /// <value cref="Action"></value>
        public Action OnPageInvisible
        {
            get
            {
                if (_onPageInvisible == null)
                    loadAdditionalAction("PI");
                return _onPageInvisible;
            }
            set
            {
                setAdditionalAction(value, "PI");
            }
        }

        internal AnnotationBorderStyle Border
        {
            get
            {
                if (_borderStyle == null)
                    loadBorderStyle();
                return _borderStyle;
            }
        }

        internal AppearanceCharacteristics Apperance
        {
            get
            {
                if (_appearanceCharacteristics == null)
                    loadAppearanceCharacteristics();
                return _appearanceCharacteristics;
            }
        }

	    internal Widget(float left, float top, float width, float height, IDocumentEssential owner)
		    : base(left, top, width, height, owner)
	    {
		    Dictionary.AddItem("Subtype", new PDFName("Widget"));
	    }

	    internal Widget(PDFDictionary dict, IDocumentEssential owner)
		    : base(dict, owner) { }

	    private void loadBorderStyle()
        {
            PDFDictionary bs = Dictionary["BS"] as PDFDictionary;
            if (bs != null)
                _borderStyle = new AnnotationBorderStyle(bs);
            else
            {
                _borderStyle = new AnnotationBorderStyle();
                Dictionary.AddItem("BS", _borderStyle.GetDictionary());
            }
        }

        private void loadAppearanceCharacteristics()
        {
            PDFDictionary mk = Dictionary["MK"] as PDFDictionary;
            if (mk != null)
                _appearanceCharacteristics = new AppearanceCharacteristics(mk);
            else
            {
                _appearanceCharacteristics = new AppearanceCharacteristics();
                Dictionary.AddItem("MK", _appearanceCharacteristics.GetDictionary());
            }
        }

        private void loadAdditionalAction(string name)
        {
            PDFDictionary aa = Dictionary["AA"] as PDFDictionary;
            if (aa != null)
            {
                PDFDictionary dict = aa[name] as PDFDictionary;
                if (dict != null)
                    setActionValue(Action.Create(dict, Owner), name);
            }
        }

        private void setAdditionalAction(Action action, string key)
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
                Action a = action.Clone(Owner);
                setActionValue(a, key);
                if (Dictionary["AA"] as PDFDictionary == null)
                    Dictionary.AddItem("AA", new PDFDictionary());
                (Dictionary["AA"] as PDFDictionary).AddItem(key, a.GetDictionary());
            }
        }

        private void setActionValue(Action action, string key)
        {
            switch (key)
            {
                case "E":
                    _onMouseEnter = action;
                    break;
                case "X":
                    _onMouseExit = action;
                    break;
                case "D":
                    _onMouseDown = action;
                    break;
                case "U":
                    _onMouseUp = action;
                    break;
                case "Fo":
                    _onReceiveFocus = action;
                    break;
                case "Bl":
                    _onLoseFocus = action;
                    break;
                case "PO":
                    _onPageOpen = action;
                    break;
                case "PC":
                    _onPageClose = action;
                    break;
                case "PV":
                    _onPageVisible = action;
                    break;
                case "PI":
                    _onPageInvisible = action;
                    break;
            }
        }
    }
}
