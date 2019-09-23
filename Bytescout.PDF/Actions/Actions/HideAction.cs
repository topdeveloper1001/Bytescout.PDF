using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class HideAction : Action
#else
	/// <summary>
    /// Represents an action which hides or shows one or more controls on the screen.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class HideAction : Action
#endif
	{
		private readonly PDFDictionary _dictionary;
		private FieldCollection _fields;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.Hide; } }

	    /// <summary>
	    /// Gets or sets a value indicating whether to hide or show controls associated with this action.
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Hide
	    {
		    get
		    {
			    PDFBoolean h = _dictionary["H"] as PDFBoolean;
			    if (h != null)
				    return h.GetValue();
			    return true;
		    }
		    set
		    {
			    _dictionary.AddItem("H", new PDFBoolean(value));
		    }
	    }

	    /// <summary>
	    /// Gets the fields.
	    /// </summary>
	    /// <value cref="FieldCollection"></value>
	    public FieldCollection Fields
	    {
		    get
		    {
			    if (_fields == null)
				    _fields = new FieldCollection(_dictionary["T"], Owner, _dictionary, "T");
			    return _fields;
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.HideAction.
        /// </summary>
        /// <param name="hide" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">The value indicating whether to hide (if set to true) or show (if set to false)
        /// any field associated later with the action as the result of the action.</param>
        public HideAction(bool hide)
            : base(null)
        {
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("Hide"));
            Hide = hide;
        }

        internal HideAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("Hide"));

            IPDFObject t = _dictionary["T"];
            if (t != null)
                dict.AddItem("T", t.Clone());

            HideAction action = new HideAction(dict, owner);
            action.Hide = Hide;

            IPDFObject next = _dictionary["Next"];
            if (next != null)
            {
                for (int i = 0; i < Next.Count; ++i)
                    action.Next.Add(Next[i]);
            }

            return action;
        }

        internal override void ApplyOwner(IDocumentEssential owner)
        {
            base.ApplyOwner(owner);
            if (_fields != null)
                _fields.Owner = owner;
        }

        internal override PDFDictionary GetDictionary()
        {
            return _dictionary;
        }
    }
}
