using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class NamedAction : Action
#else
    /// <summary>
    /// Represents an action which performs the named action.
    /// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
    public class NamedAction : Action
#endif
	{
	    private readonly PDFDictionary _dictionary;

	    /// <summary>
	    /// Gets the Bytescout.PDF.ActionType value that specifies the type of this action.
	    /// </summary>
	    /// <value cref="ActionType"></value>
	    public override ActionType Type { get { return ActionType.Named; } }

	    /// <summary>
	    /// Gets or sets the destination of an action.
	    /// </summary>
	    /// <value cref="NamedActions"></value>
	    public NamedActions Action
	    {
		    get { return TypeConverter.PDFNameToPDFNamedAction(_dictionary["N"] as PDFName); }
		    set { _dictionary.AddItem("N", TypeConverter.PDFNamedActionToPDFName(value)); }
	    }

	    /// <summary>
        /// Initializes a new instance of the Bytescout.PDF.NamedAction.
        /// </summary>
        /// <param name="action">The Bytescout.PDF.NamedActions object representing the destination of an action.</param>
        public NamedAction(NamedActions action)
            : base(null)
        {
            _dictionary = new PDFDictionary();
            _dictionary.AddItem("Type", new PDFName("Action"));
            _dictionary.AddItem("S", new PDFName("Named"));

            Action = action;
        }

        internal NamedAction(PDFDictionary dict, IDocumentEssential owner)
            : base(owner)
        {
            _dictionary = dict;
        }

	    internal override Action Clone(IDocumentEssential owner)
        {
            PDFDictionary dict = new PDFDictionary();
            dict.AddItem("Type", new PDFName("Action"));
            dict.AddItem("S", new PDFName("Named"));

            IPDFObject n = _dictionary["N"];
            if (n != null)
                dict.AddItem("N", n.Clone());

            NamedAction action = new NamedAction(dict, owner);

            IPDFObject next = _dictionary["Next"];
            if (next != null)
            {
                for (int i = 0; i < Next.Count; ++i)
                    action.Next.Add(Next[i]);
            }

            return action;
        }

        internal override PDFDictionary GetDictionary()
        {
            return _dictionary;
        }
    }
}
