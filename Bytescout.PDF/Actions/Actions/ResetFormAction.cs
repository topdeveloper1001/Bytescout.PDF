using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class ResetFormAction : Action
#else
	/// <summary>
    /// Represents the PDF form's reset action.
    /// <remarks>This action allows a user to reset the form fields to their default values.</remarks>
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class ResetFormAction : Action
#endif
	{
	    private readonly PDFDictionary _dictionary;
	    private FieldCollection _fields;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.ResetForm; } }

	    /// <summary>
	    /// Gets or sets a value indicating whether fields contained in Fields collection will be included for resetting.
	    /// <remarks>If Include property is true, only the fields in this collection will be reset. If Include property
	    /// is false, the fields in this collection are not reset and only the remaining form fields are reset.
	    /// If the collection is empty, then all the form fields are reset and the Include property is ignored.</remarks>
	    /// </summary>
	    /// <value cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx"></value>
	    public bool Include
	    {
		    get
		    {
			    PDFNumber num = _dictionary["Flags"] as PDFNumber;
			    if (num == null || num.GetValue() == 0)
				    return false;
			    return true;
		    }
		    set
		    {
			    if (value)
				    _dictionary.AddItem("Flags", new PDFNumber(1));
			    else
				    _dictionary.AddItem("Flags", new PDFNumber(0));
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
				    _fields = new FieldCollection(_dictionary["Fields"], Owner, _dictionary, "Fields");
			    return _fields;
		    }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.ResetFormAction.
        /// </summary>
        public ResetFormAction()
            : base(null)
        {
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("ResetForm"));
        }

        internal ResetFormAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("ResetForm"));

            string[] keys = { "Flags", "Fields" };
            for (int i = 0; i < keys.Length; ++i)
            {
                IPDFObject obj = _dictionary[keys[i]];
                if (obj != null)
                    dict.AddItem(keys[i], obj.Clone());
            }

            ResetFormAction action = new ResetFormAction(dict, owner);

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
