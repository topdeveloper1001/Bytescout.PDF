namespace Bytescout.PDF
{
    internal class Catalog
    {
	    private readonly PDFDictionary _dictionary;

	    private readonly DocumentEssential _documentEssential;
	    private Names _names;
	    private PageCollection _pages;
	    private ViewerPreferences _viewerPreferences;
	    private PageLabelsCollection _pageLabels;
	    private OutlinesCollection _outlines;
	    private AcroForm _acroForm;
	    private OptionalContents _optionalContents;

	    private Action _onOpenAction;
	    private JavaScriptAction _beforeClosing;
	    private JavaScriptAction _beforeSaving;
	    private JavaScriptAction _afterSaving;
	    private JavaScriptAction _beforePrinting;
	    private JavaScriptAction _afterPrinting;

	    public PageCollection Pages
        {
            get 
            {
                if (_pages == null)
                    loadPages();
                return _pages;
            }
        }

        public OptionalContents OptionalContents
        {
            get
            {
                if (_optionalContents == null)
                {
                    PDFDictionary ocProperties = _dictionary["OCProperties"] as PDFDictionary;
                    if (ocProperties != null)
                        _optionalContents = new OptionalContents(ocProperties);
                    else
                    {
                        _optionalContents = new OptionalContents();
                        _dictionary.AddItem("OCProperties", _optionalContents.GetDictionary());
                    }
                }

                return _optionalContents;
            }
        }

        public ViewerPreferences ViewerPreferences
        {
            get 
            {
                if (_viewerPreferences == null)
                    loadViewerPreferences();
                return _viewerPreferences;
            }
        }

        public OutlinesCollection Outlines
        {
            get
            {
                if (_outlines == null)
                    loadOutlines();
                return _outlines;
            }
        }

        internal PageLabelsCollection PageLabels
        {
            get
            {
                if (_pageLabels == null)
                    loadPageLabels();
                return _pageLabels;
            }
        }

        public PageLayout PageLayout
        {
            get { return TypeConverter.PDFNameToPDFPageLayout(_dictionary["PageLayout"] as PDFName); }
            set { _dictionary.AddItem("PageLayout", TypeConverter.PDFPageLayoutToPDFName(value)); }
        }

        public PageMode PageMode
        {
            get { return TypeConverter.PDFNameToPDFPageMode(_dictionary["PageMode"] as PDFName); }
            set { _dictionary.AddItem("PageMode", TypeConverter.PDFPageModeToPDFName(value)); }
        }

        public Action OnOpenDocument
        {
            get
            {
                if (_onOpenAction == null)
                    loadOnOpenAction();
                return _onOpenAction;
            }
            set
            {
                if (value == null)
                {
                    _onOpenAction = null;
                    _dictionary.RemoveItem("OpenAction");
                }
                else
                {
                    _onOpenAction = value.Clone(_documentEssential);
                    _dictionary.AddItem("OpenAction", _onOpenAction.GetDictionary());
                }
            }
        }

        public JavaScriptAction OnBeforeClosing
        {
            get
            {
                if (_beforeClosing == null)
                    loadAdditionalAction("WC");
                return _beforeClosing;
            }
            set
            {
                setAdditionalAction(value, "WC");
            }
        }

        public JavaScriptAction OnBeforeSaving
        {
            get
            {
                if (_beforeSaving == null)
                    loadAdditionalAction("WS");
                return _beforeSaving;
            }
            set
            {
                setAdditionalAction(value, "WS");
            }
        }

        public JavaScriptAction OnAfterSaving
        {
            get
            {
                if (_afterSaving == null)
                    loadAdditionalAction("DS");
                return _afterSaving;
            }
            set
            {
                setAdditionalAction(value, "DS");
            }
        }

        public JavaScriptAction OnBeforePrinting
        {
            get
            {
                if (_beforePrinting == null)
                    loadAdditionalAction("WP");
                return _beforePrinting;
            }
            set
            {
                setAdditionalAction(value, "WP");
            }
        }

        public JavaScriptAction OnAfterPrinting
        {
            get
            {
                if (_afterPrinting == null)
                    loadAdditionalAction("DP");
                return _afterPrinting;
            }
            set
            {
                setAdditionalAction(value, "DP");
            }
        }

        internal Names Names
        {
            get 
            {
                if (_names == null)
                    loadNames();
                return _names;
            }
        }

        internal AcroForm AcroForm
        {
            get
            {
                if (_acroForm == null)
                    loadAcroForm();
                return _acroForm;
            }
        }

	    internal Catalog()
	    {
		    _dictionary = new PDFDictionary();
		    _dictionary.AddItem("Type", new PDFName("Catalog"));
		    _documentEssential = new DocumentEssential(this);
	    }

	    internal Catalog(PDFDictionary dict)
	    {
		    if (dict == null)
			    throw new InvalidDocumentException();

		    _dictionary = dict;
		    _documentEssential = new DocumentEssential(this);
	    }

	    internal PDFDictionary GetDictionary()
        {
            return _dictionary;
        }

        internal void CheckPageLabels(int pageCount)
        {
            if (_pageLabels != null)
                _pageLabels.Check(pageCount);
        }

        private void loadPages()
        {
            PDFDictionary pages = _dictionary["Pages"] as PDFDictionary;
            if (pages != null)
            {
                _pages = new PageCollection(pages, _documentEssential);
            }
            else
            {
                _pages = new PageCollection(_documentEssential);
                _dictionary.AddItem("Pages", _pages.GetDictionary());
            }
        }

        private void loadViewerPreferences()
        {
            PDFDictionary viewerPreferences = _dictionary["ViewerPreferences"] as PDFDictionary;
            if (viewerPreferences != null)
            {
                _viewerPreferences = new ViewerPreferences(viewerPreferences);
            }
            else
            {
                _viewerPreferences = new ViewerPreferences();
                _dictionary.AddItem("ViewerPreferences", _viewerPreferences.GetDictionary());
            }
        }

        private void loadOutlines()
        {
            PDFDictionary outlinesRoot = _dictionary["Outlines"] as PDFDictionary;
            if (outlinesRoot != null)
            {
                _outlines = new OutlinesCollection(outlinesRoot, _documentEssential);
            }
            else
            {
                _outlines = new OutlinesCollection(_documentEssential);
                _dictionary.AddItem("Outlines", _outlines.GetDictionary());
            }
        }

        private void loadPageLabels()
        {
            PDFDictionary pageLabelsRoot = _dictionary["PageLabels"] as PDFDictionary;
            if (pageLabelsRoot != null)
            {
                _pageLabels = new PageLabelsCollection(pageLabelsRoot);
            }
            else
            {
                _pageLabels = new PageLabelsCollection();
                _dictionary.AddItem("PageLabels", _pageLabels.GetDictionary());
            }
        }

        private void loadNames()
        {
            PDFDictionary names = _dictionary["Names"] as PDFDictionary;
            if (names == null)
                _names = new Names();
            else
                _names = new Names(names);
        }

        private void loadAcroForm()
        {
            PDFDictionary acroForm = _dictionary["AcroForm"] as PDFDictionary;
            if (acroForm == null)
            {
                _acroForm = new AcroForm();
                _dictionary.AddItem("AcroForm", _acroForm.GetDictionary());
            }
            else
                _acroForm = new AcroForm(acroForm);
        }

        private void getPageBoundings(int index, ref float left, ref float bottom)
        {
            if (Pages.Count <= index)
                return;

            System.Drawing.RectangleF cropBox = Pages[index].PageRect;
            left = cropBox.Left;
            bottom = cropBox.Bottom;
        }

        private void loadOnOpenAction()
        {
            IPDFObject openAction = _dictionary["OpenAction"] as PDFDictionary;
            if (openAction is PDFDictionary)
            {
                PDFDictionary onOpen = openAction as PDFDictionary;
                if (onOpen != null)
                    _onOpenAction = Action.Create(onOpen, _documentEssential);
            }
            else if (openAction is PDFArray)
            {
                Destination dest = new Destination(openAction, _documentEssential);
                GoToAction action = new GoToAction(dest);
                action.Owner = _documentEssential;
                _onOpenAction = action;
                _dictionary.AddItem("OpenAction", _onOpenAction.GetDictionary());
            }
        }

        private void loadAdditionalAction(string name)
        {
            PDFDictionary aa = _dictionary["AA"] as PDFDictionary;
            if (aa != null)
            {
                PDFDictionary dict = aa[name] as PDFDictionary;
                if (dict != null)
                    setActionValue(new JavaScriptAction(dict, _documentEssential), name);
            }
        }

        private void setAdditionalAction(JavaScriptAction action, string key)
        {
            if (action == null)
            {
                setActionValue(null, key);
                if ((_dictionary["AA"] as PDFDictionary) != null)
                {
                    (_dictionary["AA"] as PDFDictionary).RemoveItem(key);
                    if ((_dictionary["AA"] as PDFDictionary).Count == 0)
                        _dictionary.RemoveItem("AA");
                }
            }
            else
            {
                JavaScriptAction a = (JavaScriptAction)action.Clone(_documentEssential);
                setActionValue(a, key);
                if ((_dictionary["AA"] as PDFDictionary) == null)
                    _dictionary.AddItem("AA", new PDFDictionary());
                (_dictionary["AA"] as PDFDictionary).AddItem(key, a.GetDictionary());
            }
        }

        private void setActionValue(JavaScriptAction action, string key)
        {
            switch (key)
            {
                case "WC":
                    _beforeClosing = action;
                    break;
                case "WS":
                    _beforeSaving = action;
                    break;
                case "DS":
                    _afterSaving = action;
                    break;
                case "WP":
                    _beforePrinting = action;
                    break;
                case "DP":
                    _afterPrinting = action;
                    break;
            }
        }
    }
}
