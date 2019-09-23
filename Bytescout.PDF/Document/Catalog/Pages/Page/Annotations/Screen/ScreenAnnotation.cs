using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ScreenAnnotation : Annotation
#else
	/// <summary>
    /// Represents a screen annotation.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class ScreenAnnotation : Annotation
#endif
	{
	    private Action _onActivated;
	    private Action _onMouseEnter;
	    private Action _onMouseExit;
	    private Action _onMouseDown;
	    private Action _onMouseUp;
	    private Action _onPageOpen;
	    private Action _onPageClose;
	    private Action _onPageVisible;
	    private Action _onPageInvisible;

	    /// <summary>
        /// Gets the Bytescout.PDF.AnnotationType value that specifies the type of this annotation.
        /// </summary>
        /// <value cref="AnnotationType"></value>
        public override AnnotationType Type { get { return AnnotationType.Screen; } }

	    internal Action OnActivated
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
        
	    internal Action OnMouseEnter
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
        
	    internal Action OnMouseExit
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
        
	    internal Action OnMouseDown
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
        
	    internal Action OnMouseUp
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
       
	    internal Action OnPageOpen
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
        
	    internal Action OnPageClose
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
        
	    internal Action OnPageVisible
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
        
	    internal Action OnPageInvisible
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

	    internal ScreenAnnotation(PDFDictionary dict, IDocumentEssential owner)
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

            IPDFObject t = Dictionary["T"];
            if (t != null)
                res.AddItem("T", t.Clone());

            PDFDictionary mk = Dictionary["MK"] as PDFDictionary;
            if (mk != null)
                res.AddItem("MK", AppearanceCharacteristics.Copy(mk));

            ScreenAnnotation annot = new ScreenAnnotation(res, owner);
            annot.SetPage(Page, false);
            annot.SetPage(page, true);

            annot.OnActivated = OnActivated;
            annot.OnMouseDown = OnMouseDown;
            annot.OnMouseEnter = OnMouseEnter;
            annot.OnMouseExit = OnMouseExit;
            annot.OnMouseUp = OnMouseUp;
            annot.OnPageClose = OnPageClose;
            annot.OnPageInvisible = OnPageInvisible;
            annot.OnPageOpen = OnPageOpen;
            annot.OnPageVisible = OnPageVisible;

            return annot;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            Owner = owner;
            if (_onActivated != null)
                _onActivated.ApplyOwner(owner);
            if (_onMouseEnter != null)
                _onMouseEnter.ApplyOwner(owner);
            if (_onMouseExit != null)
                _onMouseExit.ApplyOwner(owner);
            if (_onMouseDown != null)
                _onMouseDown.ApplyOwner(owner);
            if (_onMouseUp != null)
                _onMouseUp.ApplyOwner(owner);
            if (_onPageOpen != null)
                _onPageOpen.ApplyOwner(owner);
            if (_onPageClose != null)
                _onPageClose.ApplyOwner(owner);
            if (_onPageVisible != null)
                _onPageVisible.ApplyOwner(owner);
            if (_onPageInvisible != null)
                _onPageInvisible.ApplyOwner(owner);
        }

	    internal override void CreateApperance()
        {
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
	            PDFDictionary dictionary = Dictionary["AA"] as PDFDictionary;
				if (dictionary != null)
                {
                    dictionary.RemoveItem(key);
                    if (dictionary.Count == 0)
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
